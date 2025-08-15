// Basic service worker for DecidingVote (cache-first for static assets, network-first for API)
const CACHE_NAME = 'decidingvote-static-v1'
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/manifest.webmanifest',
  '/favicon.ico'
]

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => cache.addAll(STATIC_ASSETS))
  )
  self.skipWaiting()
})

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys => Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k))))
  )
  self.clients.claim()
})

// Helper: determine if request is API
function isApiRequest(request) {
  return request.url.includes('/api/')
}

self.addEventListener('fetch', event => {
  const { request } = event
  // Bypass non-GET
  if (request.method !== 'GET') return

  if (isApiRequest(request)) {
    // Network-first for API
    event.respondWith(
      fetch(request).then(resp => {
        return resp
      }).catch(() => caches.match(request))
    )
  } else {
    // Cache-first for static
    event.respondWith(
      caches.match(request).then(cached => {
        if (cached) return cached
        return fetch(request).then(resp => {
          const respClone = resp.clone()
          caches.open(CACHE_NAME).then(cache => cache.put(request, respClone))
          return resp
        })
      })
    )
  }
})

// Optional: listen for skip waiting message
self.addEventListener('message', event => {
  if (event.data === 'SKIP_WAITING') self.skipWaiting()
})
