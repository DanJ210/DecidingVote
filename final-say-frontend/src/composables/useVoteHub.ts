import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr'
import { ref, onUnmounted } from 'vue'

const connectionRef = ref<HubConnection | null>(null)
const started = ref(false)
let startPromise: Promise<void> | null = null

// Pending handlers registered before the connection is available/started
const pendingHandlers = new Map<string, Set<(...args: any[]) => void>>()

export function useVoteHub() {
  async function ensureConnection() {
    if (!connectionRef.value) {
      connectionRef.value = new HubConnectionBuilder()
        .withUrl('/hubs/votes', {
          accessTokenFactory: () => localStorage.getItem('authToken') || ''
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build()

      // Attach any pending handlers to the newly created connection
      for (const [event, handlers] of pendingHandlers.entries()) {
        for (const h of handlers) {
          connectionRef.value.on(event, h)
        }
      }
    }

    // require token before attempting start
    const token = localStorage.getItem('authToken')
    if (!token) throw new Error('No auth token present; not starting SignalR connection yet')

    // start the connection if not already started
    if (!started.value) {
      if (!startPromise) {
        startPromise = connectionRef.value.start()
          .then(() => { started.value = true })
          .catch(err => { startPromise = null; throw err })
      }
      await startPromise
    }

    return connectionRef.value
  }

  async function joinQuestion(questionId: number) {
    const conn = await ensureConnection()
    if (conn && started.value) {
      await conn.invoke('JoinQuestion', questionId)
    }
  }

  async function leaveQuestion(questionId: number) {
    if (!connectionRef.value || !started.value) return
    await connectionRef.value.invoke('LeaveQuestion', questionId)
  }

  function on(event: string, handler: (...args: any[]) => void) {
    // Store handler so it can be attached once the connection is built/started
    let set = pendingHandlers.get(event)
    if (!set) {
      set = new Set()
      pendingHandlers.set(event, set)
    }
    set.add(handler)

    // If connection already exists, attach immediately
    if (connectionRef.value) {
      connectionRef.value.on(event, handler)
    }
  }

  async function disconnect() {
    if (connectionRef.value && started.value) {
      try { await connectionRef.value.stop() } catch { /* ignore */ }
    }
    started.value = false
    startPromise = null
  }

  onUnmounted(() => {
    if (connectionRef.value) {
      connectionRef.value.stop()
      started.value = false
    }
  })

  return { ensureConnection, joinQuestion, leaveQuestion, on, disconnect }
}
