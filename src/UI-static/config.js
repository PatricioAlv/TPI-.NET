// Configuración para despliegue híbrido
// Actualiza estas URLs con las que te dé ngrok cuando ejecutes: ngrok start --all

window.API_CONFIG = {
    // Reemplaza con tus URLs de ngrok
    auth: 'https://XXXXXXXX.ngrok-free.app/api/auth',
    messages: 'https://YYYYYYYY.ngrok-free.app/api/messages',
    groups: 'https://ZZZZZZZZ.ngrok-free.app/api/groups',
    chatHub: 'https://YYYYYYYY.ngrok-free.app/hubs/chat'
};

// INSTRUCCIONES:
// 1. Ejecuta: ngrok start --all --config ngrok.yml
// 2. Copia las URLs que te muestra ngrok
// 3. Reemplaza XXXXXXXX, YYYYYYYY, ZZZZZZZZ con tus URLs
// 4. Despliega en Vercel/Netlify
