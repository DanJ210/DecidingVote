<template>
  <div class="questions">
    <h1>All Questions</h1>
    
    <div v-if="loading" class="loading">
      Loading questions...
    </div>
    
    <div v-else-if="error" class="error">
      {{ error }}
    </div>
    
    <div v-else>
      <div v-if="questions.length === 0" class="no-questions">
        <p>No questions found. Be the first to create one!</p>
        <RouterLink to="/create" class="btn">Create Question</RouterLink>
      </div>
      
      <div v-else class="questions-list">
        <div v-for="question in questions" :key="question.id" class="card">
          <div class="card-header">
            <h3>{{ question.title }}</h3>
            <p class="author">By {{ question.author }} • {{ formatDate(question.createdAt) }}</p>
          </div>
          
          <p class="question-text">{{ question.description }}</p>
          
          <div class="vote-buttons" v-if="isAuthenticated && !hasUserVoted(question.id)">
            <button @click="vote(question.id, 1)" class="btn btn-primary">
              {{ question.side1Text }} ({{ question.side1Votes }})
            </button>
            <button @click="vote(question.id, 2)" class="btn btn-secondary">
              {{ question.side2Text }} ({{ question.side2Votes }})
            </button>
          </div>
          
          <div v-else-if="isAuthenticated && hasUserVoted(question.id) && canChangeVote(question.id)" class="vote-change-section">
            <div class="current-vote">
              <p><strong>Your vote:</strong> {{ getUserVoteChoice(question.id) === 1 ? question.side1Text : question.side2Text }}</p>
              <p class="time-remaining">{{ getTimeRemaining(question.id) }} to change your vote</p>
            </div>
            <div class="vote-buttons">
              <button 
                @click="changeVote(question.id, 1)" 
                class="btn"
                :class="getUserVoteChoice(question.id) === 1 ? 'btn-primary' : 'btn-outline'"
              >
                {{ question.side1Text }} ({{ question.side1Votes }})
              </button>
              <button 
                @click="changeVote(question.id, 2)" 
                class="btn"
                :class="getUserVoteChoice(question.id) === 2 ? 'btn-secondary' : 'btn-outline'"
              >
                {{ question.side2Text }} ({{ question.side2Votes }})
              </button>
            </div>
          </div>
          
          <div v-else class="vote-results">
            <div class="vote-stats">
              <div class="vote-option">
                <span class="vote-label">{{ question.side1Text }}:</span>
                <span class="vote-count">{{ question.side1Votes }}</span>
                <div class="vote-bar">
                  <div class="vote-fill side1" :style="{ width: getSide1Percentage(question) + '%' }"></div>
                </div>
              </div>
              <div class="vote-option">
                <span class="vote-label">{{ question.side2Text }}:</span>
                <span class="vote-count">{{ question.side2Votes }}</span>
                <div class="vote-bar">
                  <div class="vote-fill side2" :style="{ width: getSide2Percentage(question) + '%' }"></div>
                </div>
              </div>
            </div>
            <p class="total-votes">Total votes: {{ question.side1Votes + question.side2Votes }}</p>
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
