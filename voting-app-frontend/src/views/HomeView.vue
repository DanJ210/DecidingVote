<template>
  <div class="mx-auto max-w-6xl text-center">
    <h1 class="text-4xl font-bold tracking-tight text-slate-800 dark:text-slate-100 sm:text-5xl">Welcome to DecidingVote</h1>
    <p class="mx-auto mt-4 max-w-2xl text-lg text-slate-600 dark:text-slate-400">Create two‑sided questions with fully custom labels and discover what the community thinks.</p>
    <div class="mt-16">
  <h2 class="text-2xl font-semibold text-slate-800 dark:text-slate-100">How it works</h2>
      <div class="mt-8 grid gap-6 md:grid-cols-3">
  <div class="flex flex-col rounded-lg border border-slate-200 bg-white p-6 text-left shadow-card dark:border-slate-700 dark:bg-slate-800">
          <h3 class="mb-2 text-lg font-semibold text-primary">1. Create</h3>
          <p class="text-sm leading-relaxed text-slate-600">Post a question—settle debates, resolve friendly disputes, or just satisfy curiosity.</p>
        </div>
  <div class="flex flex-col rounded-lg border border-slate-200 bg-white p-6 text-left shadow-card dark:border-slate-700 dark:bg-slate-800">
          <h3 class="mb-2 text-lg font-semibold text-secondary">2. Vote</h3>
          <p class="text-sm leading-relaxed text-slate-600">Cast your vote on either side and (if needed) change it within 24 hours.</p>
        </div>
  <div class="flex flex-col rounded-lg border border-slate-200 bg-white p-6 text-left shadow-card dark:border-slate-700 dark:bg-slate-800">
          <h3 class="mb-2 text-lg font-semibold text-success">3. Discover</h3>
          <p class="text-sm leading-relaxed text-slate-600">See live percentages and learn where consensus really lies.</p>
        </div>
      </div>
    </div>
    <div class="mt-14 flex flex-wrap justify-center gap-4">
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
/* Post-migration minimal scoped styles (none needed) */
</style>
