import { ref, watchEffect } from 'vue'

const STORAGE_KEY = 'color-mode'

type Mode = 'light' | 'dark'

const mode = ref<Mode>('light')
let initialized = false

function applyMode(value: Mode) {
  const root = document.documentElement
  if (value === 'dark') {
    root.classList.add('dark')
  } else {
    root.classList.remove('dark')
  }
}

export function useColorMode() {
  if (!initialized && typeof window !== 'undefined') {
    initialized = true
    // Load from storage or prefers-color-scheme
    const stored = localStorage.getItem(STORAGE_KEY) as Mode | null
    if (stored === 'dark' || stored === 'light') {
      mode.value = stored
    } else {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
      mode.value = prefersDark ? 'dark' : 'light'
    }
    applyMode(mode.value)
    // React to system preference changes if user hasn't explicitly set
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
      if (!localStorage.getItem(STORAGE_KEY)) {
        mode.value = e.matches ? 'dark' : 'light'
        applyMode(mode.value)
      }
    })
  }

  function toggle() {
    mode.value = mode.value === 'dark' ? 'light' : 'dark'
    localStorage.setItem(STORAGE_KEY, mode.value)
    applyMode(mode.value)
  }

  function set(value: Mode) {
    mode.value = value
    localStorage.setItem(STORAGE_KEY, value)
    applyMode(value)
  }

  watchEffect(() => {
    // Keep DOM synced (in case external changes)
    applyMode(mode.value)
  })

  return { mode, toggle, set }
}
