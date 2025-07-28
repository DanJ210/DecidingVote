<template>
  <div class="profile">
    <h1>User Profile</h1>
    
    <div v-if="loading" class="loading">
      Loading profile...
    </div>
    
    <div v-else-if="error" class="error">
      {{ error }}
    </div>
    
    <div v-else class="profile-content">
      <div class="card">
        <div class="card-header">
          <h2>{{ user.username }}</h2>
          <p>{{ user.email }}</p>
          <p class="joined-date">Joined {{ formatDate(user.createdAt) }}</p>
        </div>
        
        <div class="stats">
          <div class="stat">
            <span class="stat-number">{{ user.questionsCount }}</span>
            <span class="stat-label">Questions Created</span>
          </div>
          <div class="stat">
            <span class="stat-number">{{ user.votesCount }}</span>
            <span class="stat-label">Votes Cast</span>
          </div>
        </div>
      </div>
      
      <div class="card">
        <h3>Your Questions</h3>
        <div v-if="userQuestions.length === 0" class="no-content">
          <p>You haven't created any questions yet.</p>
          <RouterLink to="/create" class="btn">Create Your First Question</RouterLink>
        </div>
        <div v-else class="questions-list">
          <div v-for="question in userQuestions" :key="question.id" class="question-item">
            <h4>{{ question.title }}</h4>
            <p class="question-stats">
              {{ question.yesVotes }} Yes • {{ question.noVotes }} No • 
              Total: {{ question.yesVotes + question.noVotes }} votes
            </p>
            <p class="question-date">{{ formatDate(question.createdAt) }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import axios from 'axios'

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
  createdAt: string
  yesVotes: number
  noVotes: number
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
    const token = localStorage.getItem('authToken')
    const response = await axios.get('/api/auth/profile', {
      headers: { Authorization: `Bearer ${token}` }
    })
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
    const token = localStorage.getItem('authToken')
    const response = await axios.get('/api/questions/user', {
      headers: { Authorization: `Bearer ${token}` }
    })
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
.profile {
  max-width: 800px;
  margin: 0 auto;
}

.profile-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.joined-date {
  color: #666;
  font-size: 0.9rem;
  margin-top: 0.5rem;
}

.stats {
  display: flex;
  gap: 2rem;
  margin-top: 1rem;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  flex: 1;
}

.stat-number {
  font-size: 2rem;
  font-weight: bold;
  color: #3498db;
}

.stat-label {
  font-size: 0.9rem;
  color: #666;
  margin-top: 0.5rem;
}

.questions-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 1rem;
}

.question-item {
  padding: 1rem;
  border: 1px solid #eee;
  border-radius: 4px;
  background: #f9f9f9;
}

.question-item h4 {
  margin-bottom: 0.5rem;
  color: #2c3e50;
}

.question-stats {
  color: #666;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

.question-date {
  color: #999;
  font-size: 0.8rem;
}

.no-content {
  text-align: center;
  padding: 2rem;
}

.no-content .btn {
  margin-top: 1rem;
  text-decoration: none;
  display: inline-block;
}
</style>
