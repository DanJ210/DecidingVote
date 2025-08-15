<template>
  <div class="mx-auto max-w-2xl">
    <h1 class="mb-6 text-3xl font-semibold tracking-tight text-slate-800">Create New Poll</h1>
    <form @submit.prevent="submitPoll" class="card space-y-6">
      <div class="space-y-1.5">
        <label for="title" class="block text-sm font-medium text-slate-700">Question Title *</label>
        <input
          id="title"
          v-model="form.title"
          type="text"
          required
          placeholder="Enter your question title"
          maxlength="200"
          class="input"
        />
      </div>
      <div class="space-y-1.5">
        <label for="description" class="block text-sm font-medium text-slate-700">Question Description *</label>
        <textarea
          id="description"
          v-model="form.description"
          required
          placeholder="Provide more details about your question"
          maxlength="1000"
          class="textarea"
        ></textarea>
      </div>
      <div>
        <h3 class="mb-4 border-b border-slate-200 pb-2 text-lg font-medium text-slate-800">Voting Options</h3>
        <div class="grid gap-4 md:grid-cols-2">
          <div class="space-y-1.5">
            <label for="side1Text" class="block text-sm font-medium text-slate-700">Option 1 *</label>
            <input
              id="side1Text"
              v-model="form.side1Text"
              type="text"
              required
              placeholder="e.g., Pizza, Yes, Option A"
              maxlength="200"
              class="input"
            />
          </div>
          <div class="space-y-1.5">
            <label for="side2Text" class="block text-sm font-medium text-slate-700">Option 2 *</label>
            <input
              id="side2Text"
              v-model="form.side2Text"
              type="text"
              required
              placeholder="e.g., Tacos, No, Option B"
              maxlength="200"
              class="input"
            />
          </div>
        </div>
      </div>
      <div class="flex flex-wrap items-center gap-4 pt-2">
        <button type="submit" class="btn btn-success" :disabled="submitting">
          {{ submitting ? 'Creating...' : 'Create Poll' }}
        </button>
        <RouterLink to="/questions" class="btn btn-outline">Cancel</RouterLink>
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
/* Scoped block intentionally minimal post-migration */
</style>
