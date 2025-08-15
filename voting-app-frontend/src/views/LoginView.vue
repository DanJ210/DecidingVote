<template>
  <div class="mx-auto max-w-sm">
    <h1 class="mb-6 text-3xl font-semibold tracking-tight text-slate-800">Login</h1>
    <form @submit.prevent="handleLogin" autocomplete="off" class="card space-y-6">
      <div class="space-y-1.5">
        <label for="email" class="block text-sm font-medium text-slate-700">Email *</label>
        <input id="email" v-model="form.email" type="email" required placeholder="Enter your email" autocomplete="email" class="input" />
      </div>
      <div class="space-y-1.5">
        <label for="password" class="block text-sm font-medium text-slate-700">Password *</label>
        <input id="password" v-model="form.password" type="password" required placeholder="Enter your password" autocomplete="current-password" class="input" />
      </div>
      <div class="pt-2">
        <button type="submit" class="btn btn-success w-full justify-center" :disabled="submitting">
          {{ submitting ? 'Logging in...' : 'Login' }}
        </button>
      </div>
    </form>
    <p class="mt-6 text-center text-sm text-slate-600">Don't have an account? <RouterLink to="/register" class="font-medium text-primary hover:underline">Register here</RouterLink></p>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import api from '../plugins/axios'

const router = useRouter()
const { login } = useAuth()

const form = reactive({
  email: '',
  password: ''
})

const submitting = ref(false)

async function handleLogin() {
  if (!form.email.trim() || !form.password.trim()) {
    alert('Please fill in all fields')
    return
  }
  
  submitting.value = true
  
  try {
    const response = await api.post('/auth/login', form)
    
    // Use the auth composable to handle login
  login(response.data.token)
  // Refresh in-memory auth state
  // (useAuth keeps a shared module-level state, but ensure any guards react)
  // No-op here as login() already updates state.
    
    // Redirect to home page
    router.push('/')
  } catch (err: any) {
    console.error('Login error:', err)
    alert(err.response?.data?.message || 'Login failed. Please check your credentials.')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
/* Post-migration minimal scoped styles (none needed) */
</style>
