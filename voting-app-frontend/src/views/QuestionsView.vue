<template>
  <div class="max-w-5xl mx-auto px-4">
    <h1 class="text-3xl font-bold mb-6 text-slate-800 dark:text-slate-100">All Questions</h1>
    
  <div v-if="loading" class="py-8 text-center text-gray-500 dark:text-slate-400">Loading questions...</div>
  <div v-else-if="error" class="mb-6 rounded border border-red-200 bg-red-50 p-4 text-red-700 dark:border-red-400/30 dark:bg-red-400/10 dark:text-red-300">{{ error }}</div>
    
    <div v-else>
      <div v-if="questions.length === 0" class="py-16 text-center">
        <p class="text-gray-600 dark:text-slate-400">No questions found. Be the first to create one!</p>
        <RouterLink to="/create" class="mt-4 inline-block rounded bg-primary px-5 py-2.5 font-medium text-white transition hover:bg-primary/80">Create Question</RouterLink>
      </div>
      
      <div v-else class="space-y-6">
        <div v-for="question in questions" :key="question.id" class="rounded-lg border border-gray-200 bg-white p-6 shadow-sm dark:border-slate-700 dark:bg-slate-800">
          <div class="mb-4 border-b border-gray-100 pb-4 dark:border-slate-700">
            <h3 class="text-xl font-semibold leading-snug text-slate-800 dark:text-slate-100">{{ question.title }}</h3>
            <p class="mt-1 text-sm text-gray-500 dark:text-slate-400">By {{ question.author }} • {{ formatDate(question.createdAt) }}</p>
          </div>
          <p class="mb-4 leading-relaxed text-gray-700 dark:text-slate-300">{{ question.description }}</p>
          
          <div v-if="isAuthenticated && !hasUserVoted(question.id)" class="flex gap-3 mt-2">
            <button @click="vote(question.id, 1)" class="px-4 py-2.5 rounded bg-primary text-white font-medium hover:bg-primary/80 transition text-sm">
              {{ question.side1Text }} ({{ question.side1Votes }})
            </button>
            <button @click="vote(question.id, 2)" class="px-4 py-2.5 rounded bg-secondary text-white font-medium hover:bg-secondary/80 transition text-sm">
              {{ question.side2Text }} ({{ question.side2Votes }})
            </button>
          </div>
          
          <div v-else-if="isAuthenticated && hasUserVoted(question.id) && canChangeVote(question.id)" class="mt-3">
            <div class="mb-3 rounded-md border border-gray-200 bg-gray-50 p-3 text-sm dark:border-slate-600 dark:bg-slate-700/50 dark:text-slate-200">
              <p class="mb-1"><span class="font-semibold">Your vote:</span> {{ getUserVoteChoice(question.id) === 1 ? question.side1Text : question.side2Text }}</p>
              <p class="font-medium text-green-600 dark:text-green-400">{{ getTimeRemaining(question.id) }} to change your vote</p>
            </div>
            <div class="flex gap-3">
              <button 
                @click="changeVote(question.id, 1)"
                class="px-4 py-2.5 rounded font-medium border text-sm transition"
                :class="getUserVoteChoice(question.id) === 1 
                  ? 'bg-primary text-white border-primary hover:bg-primary/80' 
                  : 'border-gray-300 text-gray-600 hover:bg-gray-100 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700'"
              >
                {{ question.side1Text }} ({{ question.side1Votes }})
              </button>
              <button 
                @click="changeVote(question.id, 2)"
                class="px-4 py-2.5 rounded font-medium border text-sm transition"
                :class="getUserVoteChoice(question.id) === 2 
                  ? 'bg-secondary text-white border-secondary hover:bg-secondary/80' 
                  : 'border-gray-300 text-gray-600 hover:bg-gray-100 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700'"
              >
                {{ question.side2Text }} ({{ question.side2Votes }})
              </button>
            </div>
          </div>
          
          <div v-else class="mt-4">
            <div class="flex flex-col gap-4">
              <div class="flex items-center gap-3">
                <span class="min-w-16 text-sm font-medium text-gray-700 dark:text-slate-300">{{ question.side1Text }}:</span>
                <span class="w-8 text-right text-sm text-gray-600 dark:text-slate-400">{{ question.side1Votes }}</span>
                <div class="flex-1 h-5 overflow-hidden rounded-full bg-gray-200 dark:bg-slate-700">
                  <div class="h-full bg-primary transition-all" :style="{ width: getSide1Percentage(question) + '%' }"></div>
                </div>
              </div>
              <div class="flex items-center gap-3">
                <span class="min-w-16 text-sm font-medium text-gray-700 dark:text-slate-300">{{ question.side2Text }}:</span>
                <span class="w-8 text-right text-sm text-gray-600 dark:text-slate-400">{{ question.side2Votes }}</span>
                <div class="flex-1 h-5 overflow-hidden rounded-full bg-gray-200 dark:bg-slate-700">
                  <div class="h-full bg-secondary transition-all" :style="{ width: getSide2Percentage(question) + '%' }"></div>
                </div>
              </div>
            </div>
            <p class="mt-3 text-xs uppercase tracking-wide text-gray-500 dark:text-slate-400">Total votes: {{ question.side1Votes + question.side2Votes }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { RouterLink } from 'vue-router'
import api from '../plugins/axios'
import { useAuth } from '../composables/useAuth'

interface Question {
  id: number
  title: string
  description: string
  side1Text: string
  side2Text: string
  author: string
  createdAt: string
  side1Votes: number
  side2Votes: number
}

interface VoteChangeStatus {
  canChange: boolean
  hasVoted: boolean
  currentChoice?: number
  hoursSinceVote?: number
  hoursRemaining?: number
  votedAt?: string
  lastModified?: string
}

const questions = ref<Question[]>([])
const loading = ref(true)
const error = ref('')
const userVotes = ref<Record<number, number>>({})
const voteChangeStatus = ref<Record<number, VoteChangeStatus>>({})

// Use global auth state so navbar and views stay in sync
const { isAuthenticated } = useAuth()

onMounted(async () => {
  await fetchQuestions()
  if (isAuthenticated.value) {
    await fetchUserVotes()
  }
})

async function fetchQuestions() {
  try {
  const response = await api.get('/questions')
    questions.value = response.data
  } catch (err) {
    error.value = 'Failed to load questions'
    console.error('Error fetching questions:', err)
  } finally {
    loading.value = false
  }
}

async function fetchUserVotes() {
  try {
    const response = await api.get('/votes/user')
    userVotes.value = response.data.reduce((acc: Record<number, number>, vote: any) => {
      acc[vote.questionId] = vote.choice
      return acc
    }, {})
    
    // Fetch vote change status for each voted question
    for (const questionId of Object.keys(userVotes.value)) {
      await checkVoteChangeStatus(parseInt(questionId))
    }
  } catch (err) {
    console.error('Error fetching user votes:', err)
  }
}

async function checkVoteChangeStatus(questionId: number) {
  try {
    const response = await api.get(`/votes/can-change/${questionId}`)
    voteChangeStatus.value[questionId] = response.data
  } catch (err) {
    console.error('Error checking vote change status:', err)
  }
}

async function vote(questionId: number, choice: number) {
  try {
    await api.post('/votes', { questionId, choice })
    
    userVotes.value[questionId] = choice
    
    // Update the question vote counts
    const question = questions.value.find(q => q.id === questionId)
    if (question) {
      if (choice === 1) {
        question.side1Votes++
      } else {
        question.side2Votes++
      }
    }
    
    // Check vote change status after voting
    await checkVoteChangeStatus(questionId)
  } catch (err) {
    console.error('Error voting:', err)
    alert('Failed to submit vote. Please try again.')
  }
}

async function changeVote(questionId: number, newChoice: number) {
  const currentChoice = userVotes.value[questionId]
  
  try {
    await api.put('/votes/change', { questionId, choice: newChoice })
    
    // Update local vote record
    userVotes.value[questionId] = newChoice
    
    // Update question vote counts
    const question = questions.value.find(q => q.id === questionId)
    if (question) {
      // Remove old vote count
      if (currentChoice === 1) {
        question.side1Votes--
      } else {
        question.side2Votes--
      }
      
      // Add new vote count
      if (newChoice === 1) {
        question.side1Votes++
      } else {
        question.side2Votes++
      }
    }
    
    // Update vote change status
    await checkVoteChangeStatus(questionId)
  } catch (err: any) {
    console.error('Error changing vote:', err)
    alert(err.response?.data?.message || 'Failed to change vote. Please try again.')
  }
}

function hasUserVoted(questionId: number): boolean {
  return questionId in userVotes.value
}

function canChangeVote(questionId: number): boolean {
  const status = voteChangeStatus.value[questionId]
  return status?.canChange ?? false
}

function getUserVoteChoice(questionId: number): number | undefined {
  return userVotes.value[questionId]
}

function getTimeRemaining(questionId: number): string {
  const status = voteChangeStatus.value[questionId]
  if (!status?.hoursRemaining) return ''
  
  const hours = Math.floor(status.hoursRemaining)
  const minutes = Math.round((status.hoursRemaining - hours) * 60)
  
  if (hours > 0) {
    return `${hours}h ${minutes}m remaining`
  } else {
    return `${minutes}m remaining`
  }
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}

function getSide1Percentage(question: Question): number {
  const total = question.side1Votes + question.side2Votes
  return total === 0 ? 0 : (question.side1Votes / total) * 100
}

function getSide2Percentage(question: Question): number {
  const total = question.side1Votes + question.side2Votes
  return total === 0 ? 0 : (question.side2Votes / total) * 100
}
</script>

<style scoped>
.questions {
  max-width: 800px;
  margin: 0 auto;
}

.questions-list {
  margin-top: 2rem;
}

.author {
  color: #666;
  font-size: 0.9rem;
  margin-top: 0.5rem;
}

.question-text {
  margin: 1rem 0;
  line-height: 1.6;
}

.vote-stats {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.vote-option {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.vote-label {
  font-weight: 500;
  min-width: 40px;
}

.vote-count {
  min-width: 30px;
  text-align: right;
}

.vote-bar {
  flex: 1;
  height: 20px;
  background: #eee;
  border-radius: 10px;
  overflow: hidden;
}

.vote-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.vote-fill.side1 {
  background: #3498db;
}

.vote-fill.side2 {
  background: #9b59b6;
}

.total-votes {
  margin-top: 1rem;
  font-size: 0.9rem;
  color: #666;
}

.vote-change-section {
  margin-top: 1rem;
}

.current-vote {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
}

.current-vote p {
  margin: 0 0 0.5rem 0;
}

.current-vote p:last-child {
  margin-bottom: 0;
}

.time-remaining {
  color: #28a745;
  font-size: 0.9rem;
  font-weight: 500;
}

.btn-outline {
  background: transparent;
  border: 2px solid #ddd;
  color: #666;
}

.btn-outline:hover {
  background: #f8f9fa;
  border-color: #bbb;
  color: #333;
}

.no-questions {
  text-align: center;
  padding: 3rem;
}

.no-questions .btn {
  margin-top: 1rem;
  text-decoration: none;
  display: inline-block;
}
</style>
