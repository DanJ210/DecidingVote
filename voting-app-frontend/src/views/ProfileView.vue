<template>
  <div class="mx-auto max-w-4xl">
  <h1 class="mb-6 text-3xl font-semibold tracking-tight text-slate-800 dark:text-slate-100">User Profile</h1>
    <div v-if="loading" class="text-center py-10 text-slate-500">Loading profile...</div>
    <div v-else-if="error" class="mb-6 rounded-md border border-danger/40 bg-danger/10 p-4 text-danger">{{ error }}</div>
    <div v-else class="flex flex-col gap-8">
  <div class="card dark:border-slate-700 dark:bg-slate-800">
        <div class="mb-6 border-b border-slate-200 pb-4">
          <h2 class="text-2xl font-bold text-slate-800">{{ user.username }}</h2>
          <p class="text-slate-600">{{ user.email }}</p>
          <p class="mt-1 text-xs text-slate-500">Joined {{ formatDate(user.createdAt) }}</p>
        </div>
        <div class="grid gap-4 sm:grid-cols-2">
          <div class="flex flex-col items-center rounded-md border border-slate-200 bg-slate-50 p-6 text-center dark:border-slate-600 dark:bg-slate-700/60">
            <span class="text-3xl font-bold text-primary">{{ user.questionsCount }}</span>
            <span class="mt-2 text-xs font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">Questions Created</span>
          </div>
          <div class="flex flex-col items-center rounded-md border border-slate-200 bg-slate-50 p-6 text-center dark:border-slate-600 dark:bg-slate-700/60">
            <span class="text-3xl font-bold text-secondary">{{ user.votesCount }}</span>
            <span class="mt-2 text-xs font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">Votes Cast</span>
          </div>
        </div>
      </div>
      <div class="card dark:border-slate-700 dark:bg-slate-800">
        <h3 class="mb-4 text-xl font-semibold text-slate-800">Your Questions</h3>
        <div v-if="userQuestions.length === 0" class="text-center py-8">
          <p class="text-slate-600">You haven't created any questions yet.</p>
          <RouterLink to="/create" class="btn mt-4">Create Your First Question</RouterLink>
        </div>
        <div v-else class="space-y-4">
          <div v-for="question in userQuestions" :key="question.id" class="rounded-md border border-slate-200 bg-slate-50 p-4 dark:border-slate-600 dark:bg-slate-700/60">
            <h4 class="text-lg font-medium text-slate-800 dark:text-slate-100">{{ question.title }}</h4>
            <p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
              {{ question.side1Votes }} {{ question.side1Text }} • {{ question.side2Votes }} {{ question.side2Text }} •
              Total: {{ question.side1Votes + question.side2Votes }} votes
            </p>
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-500">{{ formatDate(question.createdAt) }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import api from '../plugins/axios'

interface User {
  id: number
  username: string
  email: string
  createdAt: string
  questionsCount: number
  votesCount: number
}

interface Question {
  id: number
  title: string
  description: string
  side1Text: string
  side2Text: string
  createdAt: string
  side1Votes: number
  side2Votes: number
}

const user = ref<User>({} as User)
const userQuestions = ref<Question[]>([])
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  await fetchProfile()
  await fetchUserQuestions()
})

async function fetchProfile() {
  try {
    const response = await api.get('/auth/profile')
    user.value = response.data
  } catch (err) {
    error.value = 'Failed to load profile'
    console.error('Error fetching profile:', err)
  } finally {
    loading.value = false
  }
}

async function fetchUserQuestions() {
  try {
    const response = await api.get('/questions/user')
    userQuestions.value = response.data
  } catch (err) {
    console.error('Error fetching user questions:', err)
  }
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}
</script>

<style scoped>
/* Post-migration minimal scoped styles (none needed) */
</style>
