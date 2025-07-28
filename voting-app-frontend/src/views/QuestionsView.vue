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
            <button @click="vote(question.id, true)" class="btn btn-success">
              Yes ({{ question.yesVotes }})
            </button>
            <button @click="vote(question.id, false)" class="btn btn-danger">
              No ({{ question.noVotes }})
            </button>
          </div>
          
          <div v-else class="vote-results">
            <div class="vote-stats">
              <div class="vote-option">
                <span class="vote-label">Yes:</span>
                <span class="vote-count">{{ question.yesVotes }}</span>
                <div class="vote-bar">
                  <div class="vote-fill yes" :style="{ width: getYesPercentage(question) + '%' }"></div>
                </div>
              </div>
              <div class="vote-option">
                <span class="vote-label">No:</span>
                <span class="vote-count">{{ question.noVotes }}</span>
                <div class="vote-bar">
                  <div class="vote-fill no" :style="{ width: getNoPercentage(question) + '%' }"></div>
                </div>
              </div>
            </div>
            <p class="total-votes">Total votes: {{ question.yesVotes + question.noVotes }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { RouterLink } from 'vue-router'
import axios from 'axios'

interface Question {
  id: number
  title: string
  description: string
  author: string
  createdAt: string
  yesVotes: number
  noVotes: number
}

const questions = ref<Question[]>([])
const loading = ref(true)
const error = ref('')
const userVotes = ref<Record<number, boolean>>({})

const isAuthenticated = computed(() => !!localStorage.getItem('authToken'))

onMounted(async () => {
  await fetchQuestions()
  if (isAuthenticated.value) {
    await fetchUserVotes()
  }
})

async function fetchQuestions() {
  try {
    const response = await axios.get('/api/questions')
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
    const token = localStorage.getItem('authToken')
    const response = await axios.get('/api/votes/user', {
      headers: { Authorization: `Bearer ${token}` }
    })
    userVotes.value = response.data.reduce((acc: Record<number, boolean>, vote: any) => {
      acc[vote.questionId] = vote.isYes
      return acc
    }, {})
  } catch (err) {
    console.error('Error fetching user votes:', err)
  }
}

async function vote(questionId: number, isYes: boolean) {
  try {
    const token = localStorage.getItem('authToken')
    await axios.post('/api/votes', 
      { questionId, isYes },
      { headers: { Authorization: `Bearer ${token}` } }
    )
    
    userVotes.value[questionId] = isYes
    
    // Update the question vote counts
    const question = questions.value.find(q => q.id === questionId)
    if (question) {
      if (isYes) {
        question.yesVotes++
      } else {
        question.noVotes++
      }
    }
  } catch (err) {
    console.error('Error voting:', err)
    alert('Failed to submit vote. Please try again.')
  }
}

function hasUserVoted(questionId: number): boolean {
  return questionId in userVotes.value
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}

function getYesPercentage(question: Question): number {
  const total = question.yesVotes + question.noVotes
  return total === 0 ? 0 : (question.yesVotes / total) * 100
}

function getNoPercentage(question: Question): number {
  const total = question.yesVotes + question.noVotes
  return total === 0 ? 0 : (question.noVotes / total) * 100
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

.vote-fill.yes {
  background: #27ae60;
}

.vote-fill.no {
  background: #e74c3c;
}

.total-votes {
  margin-top: 1rem;
  font-size: 0.9rem;
  color: #666;
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
