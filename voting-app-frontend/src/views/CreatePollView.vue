<template>
  <div class="create-poll">
    <h1>Create New Poll</h1>
    
    <form @submit.prevent="submitPoll" class="form-container">
      <div class="form-group">
        <label for="title">Question Title *</label>
        <input
          id="title"
          v-model="form.title"
          type="text"
          required
          placeholder="Enter your question title"
          maxlength="200"
        />
      </div>
      
      <div class="form-group">
        <label for="description">Question Description *</label>
        <textarea
          id="description"
          v-model="form.description"
          required
          placeholder="Provide more details about your question"
          maxlength="1000"
        ></textarea>
      </div>
      
      <div class="form-actions">
        <button type="submit" class="btn btn-success" :disabled="submitting">
          {{ submitting ? 'Creating...' : 'Create Poll' }}
        </button>
        <RouterLink to="/questions" class="btn">Cancel</RouterLink>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import axios from 'axios'

const router = useRouter()

const form = reactive({
  title: '',
  description: ''
})

const submitting = ref(false)

async function submitPoll() {
  if (!form.title.trim() || !form.description.trim()) {
    alert('Please fill in all required fields')
    return
  }
  
  submitting.value = true
  
  try {
    const token = localStorage.getItem('authToken')
    await axios.post('/api/questions', form, {
      headers: { Authorization: `Bearer ${token}` }
    })
    
    // Redirect to questions page
    router.push('/questions')
  } catch (err) {
    console.error('Error creating poll:', err)
    alert('Failed to create poll. Please try again.')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.create-poll {
  max-width: 600px;
  margin: 0 auto;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.form-actions .btn {
  text-decoration: none;
}
</style>
