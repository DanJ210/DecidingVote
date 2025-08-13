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
      
      <div class="voting-options">
        <h3>Voting Options</h3>
        <div class="options-grid">
          <div class="form-group">
            <label for="side1Text">Option 1 *</label>
            <input
              id="side1Text"
              v-model="form.side1Text"
              type="text"
              required
              placeholder="e.g., Pizza, Yes, Option A"
              maxlength="200"
            />
          </div>
          
          <div class="form-group">
            <label for="side2Text">Option 2 *</label>
            <input
              id="side2Text"
              v-model="form.side2Text"
              type="text"
              required
              placeholder="e.g., Tacos, No, Option B"
              maxlength="200"
            />
          </div>
        </div>
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
import api from '../plugins/axios'

const router = useRouter()

const form = reactive({
  title: '',
  description: '',
  side1Text: '',
  side2Text: ''
})

const submitting = ref(false)

async function submitPoll() {
  if (!form.title.trim() || !form.description.trim() || !form.side1Text.trim() || !form.side2Text.trim()) {
    alert('Please fill in all required fields')
    return
  }
  
  submitting.value = true
  
  try {
  await api.post('/questions', form)
    
    // Redirect to questions page
    router.push('/questions')
  } catch (err: any) {
    console.error('Error creating poll:', err)
    console.error('Error response:', err.response?.data)
    console.error('Error status:', err.response?.status)
    
    if (err.response?.status === 401) {
      alert('Authentication failed. Please log in again.')
      router.push('/login')
    } else {
      alert(err.response?.data?.message || 'Failed to create poll. Please try again.')
    }
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

.voting-options {
  margin-top: 2rem;
}

.voting-options h3 {
  margin-bottom: 1rem;
  color: #333;
  border-bottom: 2px solid #e0e0e0;
  padding-bottom: 0.5rem;
}

.options-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 1rem;
}

@media (max-width: 768px) {
  .options-grid {
    grid-template-columns: 1fr;
  }
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
