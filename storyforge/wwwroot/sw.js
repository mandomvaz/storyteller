const CACHE_NAME = 'storyforge-cache-v1';
const ASSETS_TO_CACHE = [
  '/',
  '/index.html',
  '/audio-recorder.module.css',
  '/manifest.json',
  '/icon.png',
  'https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js',
  'https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js',
  'https://cdn.jsdelivr.net/npm/@microsoft/signalr-protocol-msgpack@8.0.0/dist/browser/signalr-protocol-msgpack.min.js'
];

// Install Event - Pre-cache essential frontend shell
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('[Service Worker] Pre-caching offline assets shell...');
        return cache.addAll(ASSETS_TO_CACHE);
      })
      .then(() => self.skipWaiting())
  );
});

// Activate Event - Clean up obsolete caches
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cache => {
          if (cache !== CACHE_NAME) {
            console.log('[Service Worker] Clearing old cache:', cache);
            return caches.delete(cache);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch Event - Serve cached assets when offline, otherwise fetch from network
self.addEventListener('fetch', event => {
  // Only intercept GET requests (skip API endpoints and SignalR WS traffic)
  if (event.request.method !== 'GET' || event.request.url.includes('/api/') || event.request.url.includes('/storyHub')) {
    return;
  }

  event.respondWith(
    caches.match(event.request)
      .then(cachedResponse => {
        if (cachedResponse) {
          // Serve from cache but fetch fresh version in background (Stale-While-Revalidate)
          fetch(event.request).then(networkResponse => {
            if (networkResponse.status === 200) {
              caches.open(CACHE_NAME).then(cache => cache.put(event.request, networkResponse));
            }
          }).catch(() => {/* ignore background fetch errors when offline */});
          
          return cachedResponse;
        }

        return fetch(event.request).catch(() => {
          // If offline and request is for page, return cached index
          if (event.request.mode === 'navigate') {
            return caches.match('/index.html');
          }
        });
      })
  );
});
