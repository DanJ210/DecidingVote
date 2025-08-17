// Basic service worker for FinalSay (cache-first for static assets, network-first for API)
const CACHE_NAME = 'finalsay-static-v1'
// NOTE: Removed '/favicon.ico' because it does not exist in /public currently.
// If you add one, put it back or let the resilient installer pick it up.
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/manifest.webmanifest'
]

self.addEventListener('install', event => {
  event.waitUntil(
    (async () => {
      const cache = await caches.open(CACHE_NAME)
      // Add each asset individually; ignore failures (e.g., 404 during dev)
      await Promise.all(
        STATIC_ASSETS.map(async url => {
          try {
            await cache.add(url)
          } catch (e) {
            console.warn('[SW] Skipping asset (failed to cache):', url, e)
          }
        })
      )
    })()
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
