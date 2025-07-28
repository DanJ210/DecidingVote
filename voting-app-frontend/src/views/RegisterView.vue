<template>
  <div class="register">
    <h1>Register</h1>
    
    <form @submit.prevent="handleRegister" class="form-container">
      <div class="form-group">
        <label for="username">Username *</label>
        <input
          id="username"
          v-model="form.username"
          type="text"
          required
          placeholder="Choose a username"
          minlength="3"
          maxlength="50"
        />
      </div>
      
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
          placeholder="Create a password"
          minlength="6"
        />
      </div>
      
      <div class="form-group">
        <label for="confirmPassword">Confirm Password *</label>
        <input
          id="confirmPassword"
          v-model="form.confirmPassword"
          type="password"
          required
          placeholder="Confirm your password"
          minlength="6"
        />
      </div>
      
      <div class="form-actions">
        <button type="submit" class="btn btn-success" :disabled="submitting">
          {{ submitting ? 'Creating Account...' : 'Register' }}
        </button>
      </div>
    </form>
    
    <p class="login-link">
      Already have an account? <RouterLink to="/login">Login here</RouterLink>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import axios from 'axios'

const router = useRouter()

const form = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const submitting = ref(false)

async function handleRegister() {
  if (!form.username.trim() || !form.email.trim() || !form.password.trim()) {
    alert('Please fill in all fields')
    return
  }
  
  if (form.password !== form.confirmPassword) {
    alert('Passwords do not match')
    return
  }
  
  if (form.password.length < 6) {
    alert('Password must be at least 6 characters long')
    return
  }
  
  submitting.value = true
  
  try {
    const response = await axios.post('/api/auth/register', {
      username: form.username,
      email: form.email,
      password: form.password
    })
    
    // Store the token
    localStorage.setItem('authToken', response.data.token)
    
    // Redirect to home page
    router.push('/')
  } catch (err: any) {
    console.error('Registration error:', err)
    alert(err.response?.data?.message || 'Registration failed. Please try again.')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.register {
  max-width: 400px;
  margin: 0 auto;
}

.form-actions {
  margin-top: 2rem;
}

.login-link {
  text-align: center;
  margin-top: 2rem;
}

.login-link a {
  color: #3498db;
  text-decoration: none;
}

.login-link a:hover {
  text-decoration: underline;
}
</style>
