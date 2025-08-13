<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import { useAuth } from './composables/useAuth'
import { useRouter } from 'vue-router'

const { isAuthenticated, logout: authLogout, checkAuth } = useAuth()
const router = useRouter()

function logout() {
  authLogout()
  router.push('/')
}

// Ensure navbar reflects persisted auth on first render
checkAuth()
</script>

<template>
  <div id="app">
    <header>
      <nav class="navbar">
        <div class="nav-brand">
          <RouterLink to="/">VotingApp</RouterLink>
        </div>
        <div class="nav-links">
          <RouterLink to="/">Home</RouterLink>
          <RouterLink to="/questions">Questions</RouterLink>
          <template v-if="isAuthenticated">
            <RouterLink to="/create">Create Poll</RouterLink>
            <RouterLink to="/profile">Profile</RouterLink>
            <button @click="logout" class="logout-btn">Logout</button>
          </template>
          <template v-else>
            <RouterLink to="/login">Login</RouterLink>
            <RouterLink to="/register">Register</RouterLink>
          </template>
        </div>
      </nav>
    </header>

    <main>
      <RouterView />
    </main>

    <footer>
      <p>&copy; 2025 VotingApp. All rights reserved.</p>
    </footer>
  </div>
</template>

<style scoped>
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background-color: #2c3e50;
  color: white;
}

.nav-brand a {
  font-size: 1.5rem;
  font-weight: bold;
  color: white;
  text-decoration: none;
}

.nav-links {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.nav-links a {
  color: white;
  text-decoration: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: background-color 0.3s;
}

.nav-links a:hover {
  background-color: #34495e;
}

.logout-btn {
  background: #e74c3c;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.logout-btn:hover {
  background-color: #c0392b;
}

main {
  min-height: calc(100vh - 120px);
  padding: 2rem;
}

footer {
  background-color: #2c3e50;
  color: white;
  text-align: center;
  padding: 1rem;
}
</style>
