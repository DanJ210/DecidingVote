<template>
  <div class="home">
    <h1>Welcome to VotingApp</h1>
    <p>A platform where you can create polls and vote on questions that matter to you.</p>
    
    <div class="hero-section">
      <h2>How it works:</h2>
      <div class="steps">
        <div class="step">
          <h3>1. Create</h3>
          <p>Have a discussion to work out between you and your friend? Want to prove your significant other wrong? Let the community decide!</p>
        </div>
        <div class="step">
          <h3>2. Vote</h3>
          <p>Browse questions and share your opinion</p>
        </div>
        <div class="step">
          <h3>3. Discover</h3>
          <p>See what others think about important topics</p>
        </div>
      </div>
    </div>

    <div class="cta-section">
      <RouterLink to="/questions" class="btn">Browse Questions</RouterLink>
      <RouterLink :to="isAuthenticated ? '/questions' : '/register'" class="btn btn-success">Get Started</RouterLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useAuth } from '../composables/useAuth'

const { isAuthenticated } = useAuth()
</script>

<style scoped>
.home {
  text-align: center;
  max-width: 1000px;
  margin: 0 auto;
}

h1 {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: #2c3e50;
}

.hero-section {
  margin: 3rem 0;
}

.steps {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.step {
  padding: 2rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #f8f9fa;
}

.step h3 {
  color: #3498db;
  margin-bottom: 1rem;
}

.cta-section {
  margin-top: 3rem;
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.btn {
  text-decoration: none;
  display: inline-block;
}
</style>
