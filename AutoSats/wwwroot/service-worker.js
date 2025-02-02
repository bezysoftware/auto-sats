﻿self.addEventListener('install', async event => {
    console.log('Installing service worker...');
    self.skipWaiting();
});

self.addEventListener('fetch', event => {
    return null;
});

self.addEventListener('push', event => {
    const payload = event.data.json();
    event.waitUntil(
        self.registration.showNotification('AutoSats', {
            body: payload.message + '\n' + new Date(payload.timestamp).toLocaleString(),
            icon: 'https://raw.githubusercontent.com/bezysoftware/autosats/main/Assets/LogoNotification.png',
            vibrate: [100, 50, 100],
            data: { url: '/' }
        })
    );
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data.url));
});