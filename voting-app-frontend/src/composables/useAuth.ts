import { ref } from 'vue'

// Global authentication state
const isAuthenticated = ref(false)

export function useAuth() {
  // Check if user is authenticated
  const checkAuth = () => {
    const token = localStorage.getItem('authToken')
    isAuthenticated.value = !!token
    return !!token
  }

  // Login - set authentication state
  const login = (token: string) => {
    localStorage.setItem('authToken', token)
    isAuthenticated.value = true
  }

  // Logout - clear authentication state
  const logout = () => {
    localStorage.removeItem('authToken')
    isAuthenticated.value = false
  }

  // Initialize auth state
  checkAuth()

  return {
    isAuthenticated: isAuthenticated,
    checkAuth,
    login,
    logout
  }
}
