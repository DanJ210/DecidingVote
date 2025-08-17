<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import { useAuth } from './composables/useAuth'
import { useRouter } from 'vue-router'
import { useColorMode } from './composables/useColorMode'

const { isAuthenticated, logout: authLogout, checkAuth } = useAuth()
const { mode, toggle: toggleColorMode } = useColorMode()
const router = useRouter()

function logout() {
  authLogout()
  router.push('/')
}

// Ensure navbar reflects persisted auth on first render
checkAuth()
</script>

<template>
  <div id="app" class="flex flex-col min-h-screen bg-slate-50 dark:bg-slate-900">
    <header class="border-b border-slate-200 bg-white/90 backdrop-blur supports-[backdrop-filter]:bg-white/60 dark:border-slate-700 dark:bg-slate-900/80 dark:supports-[backdrop-filter]:bg-slate-900/60">
      <nav class="flex items-center justify-between gap-6 px-6 py-4 mx-auto max-w-7xl">
        <div class="flex items-center gap-4">
          <RouterLink to="/" class="text-xl font-semibold tracking-tight text-slate-800 dark:text-slate-100">FinalSay</RouterLink>
          <button @click="toggleColorMode" class="flex items-center gap-1 btn btn-outline btn-sm" :aria-label="mode === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'">
            <span v-if="mode === 'dark'">🌞 Light</span>
            <span v-else>🌙 Dark</span>
          </button>
        </div>
        <div class="flex items-center gap-2 text-sm font-medium">
          <RouterLink to="/" class="nav-link">Home</RouterLink>
          <RouterLink to="/questions" class="nav-link">Questions</RouterLink>
          <template v-if="isAuthenticated">
            <RouterLink to="/create" class="nav-link">Create Poll</RouterLink>
            <RouterLink to="/profile" class="nav-link">Profile</RouterLink>
            <button @click="logout" class="btn btn-danger btn-sm">Logout</button>
          </template>
          <template v-else>
            <RouterLink to="/login" class="nav-link">Login</RouterLink>
            <RouterLink to="/register" class="btn btn-success btn-sm">Register</RouterLink>
          </template>
        </div>
      </nav>
    </header>
    <main class="flex-1 px-6 py-10">
      <RouterView />
    </main>
    <footer class="py-6 text-xs text-center border-t border-slate-200 bg-white/80 text-slate-500 dark:border-slate-700 dark:bg-slate-900/80 dark:text-slate-400">
  <p>&copy; 2025 FinalSay. All rights reserved.</p>
    </footer>
  </div>
</template>

<style scoped>
.nav-link {
  @apply rounded-md px-3 py-2 text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-900 dark:text-slate-300 dark:hover:bg-slate-700 dark:hover:text-slate-100;
}
/* Post-migration scoped utility composition for nav links */
</style>
