import { ref } from 'vue'

// Global authentication state
const isAuthenticated = ref(false)
const tokenPayload = ref<Record<string, any> | null>(null)

function parseJwt(token: string): Record<string, any> | null {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    )
    return JSON.parse(jsonPayload)
  } catch {
    return null
  }
}

export function useAuth() {
  // Check if user is authenticated
  const checkAuth = () => {
    const token = localStorage.getItem('authToken')
    if (!token) {
      tokenPayload.value = null
      isAuthenticated.value = false
      return false
    }
    const payload = parseJwt(token)
    tokenPayload.value = payload
    const nowSeconds = Math.floor(Date.now() / 1000)
    const notExpired = !!payload && typeof payload.exp === 'number' ? payload.exp > nowSeconds : true
    isAuthenticated.value = !!token && notExpired
    return isAuthenticated.value
  }

  // Login - set authentication state
  const login = (token: string) => {
    localStorage.setItem('authToken', token)
    tokenPayload.value = parseJwt(token)
    isAuthenticated.value = true
  // Ensure derived state is consistent
  checkAuth()
  }

  // Logout - clear authentication state
  const logout = () => {
    localStorage.removeItem('authToken')
    tokenPayload.value = null
    isAuthenticated.value = false
  }

  // Initialize auth state
  checkAuth()

  // Keep auth state in sync across tabs/windows and any external localStorage changes
  if (typeof window !== 'undefined') {
    window.addEventListener('storage', (e) => {
      if (e.key === 'authToken') {
        checkAuth()
      }
    })
  }

  return {
    isAuthenticated: isAuthenticated,
    tokenPayload,
    checkAuth,
    login,
    logout,
    isTokenExpired() {
      const payload = tokenPayload.value
      if (!payload || typeof payload.exp !== 'number') return false
      const nowSeconds = Math.floor(Date.now() / 1000)
      return payload.exp <= nowSeconds
    }
  }
}
