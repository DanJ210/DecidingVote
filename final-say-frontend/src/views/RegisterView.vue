<template>
  <div class="mx-auto max-w-md">
    <h1 class="mb-6 text-3xl font-semibold tracking-tight text-slate-800">Register</h1>
    <div v-if="generalErrors.length" class="mb-6 rounded-md border border-danger/40 bg-danger/10 p-4 text-sm text-danger">
      <ul class="list-disc space-y-1 pl-5">
        <li v-for="(e, i) in generalErrors" :key="i">{{ e }}</li>
      </ul>
    </div>
    <form @submit.prevent="handleRegister" autocomplete="off" class="card space-y-6">
      <div class="space-y-1.5">
        <label for="username" class="block text-sm font-medium text-slate-700">Username *</label>
        <input id="username" v-model="form.username" type="text" required placeholder="Choose a username" minlength="3" maxlength="50" pattern="[A-Za-z0-9._@+\-]+" title="Use letters, numbers, and . _ @ + - only" autocomplete="username" class="input" />
        <ul v-if="fieldErrors.username.length" class="space-y-0.5 pl-4 text-xs text-danger list-disc">
          <li v-for="(e, i) in fieldErrors.username" :key="'u-' + i">{{ e }}</li>
        </ul>
      </div>
      <div class="space-y-1.5">
        <label for="email" class="block text-sm font-medium text-slate-700">Email *</label>
        <input id="email" v-model="form.email" type="email" required placeholder="Enter your email" autocomplete="email" class="input" />
        <ul v-if="fieldErrors.email.length" class="space-y-0.5 pl-4 text-xs text-danger list-disc">
          <li v-for="(e, i) in fieldErrors.email" :key="'e-' + i">{{ e }}</li>
        </ul>
      </div>
      <div class="space-y-1.5">
        <label for="password" class="block text-sm font-medium text-slate-700">Password *</label>
        <input id="password" v-model="form.password" type="password" required placeholder="Create a password" minlength="6" autocomplete="new-password" class="input" />
        <ul v-if="fieldErrors.password.length" class="space-y-0.5 pl-4 text-xs text-danger list-disc">
          <li v-for="(e, i) in fieldErrors.password" :key="'p-' + i">{{ e }}</li>
        </ul>
      </div>
      <div class="space-y-1.5">
        <label for="confirmPassword" class="block text-sm font-medium text-slate-700">Confirm Password *</label>
        <input id="confirmPassword" v-model="form.confirmPassword" type="password" required placeholder="Confirm your password" minlength="6" autocomplete="new-password" class="input" />
        <ul v-if="fieldErrors.confirmPassword.length" class="space-y-0.5 pl-4 text-xs text-danger list-disc">
          <li v-for="(e, i) in fieldErrors.confirmPassword" :key="'c-' + i">{{ e }}</li>
        </ul>
      </div>
      <div class="pt-2">
        <button type="submit" class="btn btn-success w-full justify-center" :disabled="submitting">
          {{ submitting ? 'Creating Account...' : 'Register' }}
        </button>
      </div>
    </form>
    <p class="mt-6 text-center text-sm text-slate-600">Already have an account? <RouterLink to="/login" class="font-medium text-primary hover:underline">Login here</RouterLink></p>
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
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const submitting = ref(false)
const generalErrors = ref<string[]>([])
const fieldErrors = reactive({
  username: [] as string[],
  email: [] as string[],
  password: [] as string[],
  confirmPassword: [] as string[]
})

async function handleRegister() {
  generalErrors.value = []
  fieldErrors.username = []
  fieldErrors.email = []
  fieldErrors.password = []
  fieldErrors.confirmPassword = []
  if (!form.username.trim() || !form.email.trim() || !form.password.trim()) {
    if (!form.username.trim()) fieldErrors.username.push('Username is required')
    if (!form.email.trim()) fieldErrors.email.push('Email is required')
    if (!form.password.trim()) fieldErrors.password.push('Password is required')
    return
  }
  
  if (form.password !== form.confirmPassword) {
    fieldErrors.confirmPassword.push('Passwords do not match')
    return
  }
  
  if (form.password.length < 6) {
    fieldErrors.password.push('Password must be at least 6 characters long')
    return
  }

  // Match backend Identity requirement: at least one digit
  if (!/\d/.test(form.password)) {
    fieldErrors.password.push('Password must contain at least one number (0-9)')
    return
  }

  
  submitting.value = true
  
  try {
    const response = await api.post('/auth/register', {
      username: form.username,
      email: form.email,
      password: form.password
    })
    
    // Use the auth composable to handle login after registration
    login(response.data.token)
    
    // Redirect to home page
    router.push('/')
  } catch (err: any) {
    console.error('Registration error:', err)
    // Try to extract ASP.NET Core ValidationProblemDetails errors
    const data = err?.response?.data
    if (data?.errors && typeof data.errors === 'object') {
      for (const [key, val] of Object.entries<any>(data.errors)) {
        const messages: string[] = Array.isArray(val) ? (val as string[]) : []
        const k = (key || '').toString().toLowerCase()
        if (k === 'username') fieldErrors.username.push(...messages)
        else if (k === 'email') fieldErrors.email.push(...messages)
        else if (k === 'password') fieldErrors.password.push(...messages)
        else generalErrors.value.push(...messages)
      }
      if (generalErrors.value.length || fieldErrors.username.length || fieldErrors.email.length || fieldErrors.password.length) {
        return
      }
    }

    // Fall back to common shapes from Identity or generic message
    const fallback = data?.message || data?.title || 'Registration failed. Please check your inputs.'
    generalErrors.value = [fallback]
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
/* Post-migration minimal scoped styles (none needed) */
</style>
