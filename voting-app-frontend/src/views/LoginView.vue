<template>
  <div class="login">
    <h1>Login</h1>
    
    <form @submit.prevent="handleLogin" class="form-container">
      <div class="form-group">
        <label for="email">Email *</label>
        <input
          id="email"
          v-model="form.email"
          type="email"
          required
          placeholder="Enter your email"
        />
      </div>
      
      <div class="form-group">
        <label for="password">Password *</label>
        <input
          id="password"
          v-model="form.password"
          type="password"
          required
          placeholder="Enter your password"
        />
      </div>
      
      <div class="form-actions">
        <button type="submit" class="btn btn-success" :disabled="submitting">
          {{ submitting ? 'Logging in...' : 'Login' }}
        </button>
      </div>
    </form>
    
    <p class="register-link">
      Don't have an account? <RouterLink to="/register">Register here</RouterLink>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import axios from 'axios'

const router = useRouter()

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
    const response = await axios.post('/api/auth/login', form)
    
    // Store the token
    localStorage.setItem('authToken', response.data.token)
    
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
.login {
  max-width: 400px;
  margin: 0 auto;
}

.form-actions {
  margin-top: 2rem;
}

.register-link {
  text-align: center;
  margin-top: 2rem;
}

.register-link a {
  color: #3498db;
  text-decoration: none;
}

.register-link a:hover {
  text-decoration: underline;
}
</style>
