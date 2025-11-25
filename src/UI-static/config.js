// Configuración para despliegue híbrido con API Gateway
// Solo necesitas exponer el puerto 8000 con ngrok

window.API_CONFIG = {
    auth: 'https://uncranked-linelike-bryanna.ngrok-free.dev/api/auth',
    messages: 'https://uncranked-linelike-bryanna.ngrok-free.dev/api/messages',
    groups: 'https://uncranked-linelike-bryanna.ngrok-free.dev/api/groups',
    chatHub: 'https://uncranked-linelike-bryanna.ngrok-free.dev/hubs/chat'
};


// INSTRUCCIONES:
// 1. Ejecuta: .\start-local-backend.ps1 (inicia los 4 servicios)
// 2. Ejecuta: ngrok http 8000
// 3. Copia la URL que te muestra ngrok
// 4. Reemplaza TU_URL_NGROK arriba con esa URL (sin la barra final)
// 5. Despliega: vercel --prod
