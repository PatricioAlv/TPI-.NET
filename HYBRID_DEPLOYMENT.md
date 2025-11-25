# 🚀 Despliegue Híbrido - Backend Local + Frontend en la Nube

Esta configuración te permite tener el **frontend desplegado gratuitamente** mientras los **servicios backend corren en tu PC**. Perfecto para proyectos de facultad sin gastar dinero.

## 📋 Arquitectura

```
Internet
   ↓
[Vercel/Netlify] ← Frontend estático (GRATIS)
   ↓
[ngrok Tunnel] ← Expone tu PC al internet (GRATIS)
   ↓
[Tu PC] ← Auth, Messages, Groups + PostgreSQL
```

## 🛠️ Opción 1: ngrok (Recomendado - MÁS FÁCIL)

### 1. Instalar ngrok

```powershell
# Opción A: Con Chocolatey
choco install ngrok

# Opción B: Descarga manual
# Ve a https://ngrok.com/download y descarga el .exe
```

### 2. Crear cuenta gratuita en ngrok

1. Ve a https://dashboard.ngrok.com/signup
2. Crea una cuenta gratuita
3. Copia tu authtoken

### 3. Configurar ngrok

```powershell
# Configurar tu authtoken
ngrok config add-authtoken TU_TOKEN_AQUI
```

### 4. Crear archivo de configuración ngrok

Crea `ngrok.yml` en la raíz del proyecto:

```yaml
version: "2"
authtoken: TU_TOKEN_AQUI
tunnels:
  auth:
    proto: http
    addr: 5001
    domain: tu-auth.ngrok-free.app  # Opcional: dominio personalizado
  messages:
    proto: http
    addr: 5002
    domain: tu-messages.ngrok-free.app
  groups:
    proto: http
    addr: 5003
    domain: tu-groups.ngrok-free.app
```

### 5. Iniciar servicios localmente

Terminal 1 - Auth Service:
```powershell
cd src/Services/Auth.Service
dotnet run
```

Terminal 2 - Messages Service:
```powershell
cd src/Services/Messages.Service
dotnet run
```

Terminal 3 - Groups Service:
```powershell
cd src/Services/Groups.Service
dotnet run
```

### 6. Iniciar ngrok

```powershell
# Opción A: Todos los túneles a la vez
ngrok start --all --config ngrok.yml

# Opción B: Túneles individuales (si no tienes config file)
# Terminal 4:
ngrok http 5001 --domain=tu-auth.ngrok-free.app

# Terminal 5:
ngrok http 5002 --domain=tu-messages.ngrok-free.app

# Terminal 6:
ngrok http 5003 --domain=tu-groups.ngrok-free.app
```

ngrok te dará URLs públicas como:
- Auth: `https://tu-auth.ngrok-free.app`
- Messages: `https://tu-messages.ngrok-free.app`
- Groups: `https://tu-groups.ngrok-free.app`

### 7. Actualizar CORS en los servicios

Edita los `appsettings.json` de cada servicio para permitir el frontend de Vercel:

**src/Services/Auth.Service/appsettings.json**:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:8080",
      "https://tu-app.vercel.app"  // ← Agregar esta línea
    ]
  }
}
```

Haz lo mismo para Messages.Service y Groups.Service.

### 8. Desplegar el Frontend en Vercel

#### A. Preparar el frontend como estático

Crea `src/UI-static/index.html` (copia desde src/UI/wwwroot):

```powershell
# Crear carpeta para frontend estático
New-Item -ItemType Directory -Force -Path "src/UI-static"

# Copiar archivos
Copy-Item "src/UI/wwwroot/*" "src/UI-static/" -Recurse
```

#### B. Crear configuración para Vercel

Crea `vercel.json` en la raíz:

```json
{
  "version": 2,
  "public": true,
  "builds": [
    {
      "src": "src/UI-static/**",
      "use": "@vercel/static"
    }
  ],
  "routes": [
    {
      "src": "/(.*)",
      "dest": "/src/UI-static/$1"
    }
  ]
}
```

#### C. Actualizar config.js con las URLs de ngrok

Edita `src/UI-static/config.js`:

```javascript
window.API_CONFIG = {
    auth: 'https://tu-auth.ngrok-free.app/api/auth',
    messages: 'https://tu-messages.ngrok-free.app/api/messages',
    groups: 'https://tu-groups.ngrok-free.app/api/groups',
    chatHub: 'https://tu-messages.ngrok-free.app/hubs/chat'
};
```

#### D. Desplegar en Vercel

```powershell
# Instalar Vercel CLI
npm install -g vercel

# Login
vercel login

# Desplegar
vercel --prod
```

Vercel te dará una URL como: `https://tu-app.vercel.app`

---

## 🛠️ Opción 2: Cloudflare Tunnel (Alternativa)

Si ngrok no funciona, puedes usar Cloudflare Tunnel (también gratis).

### 1. Instalar cloudflared

```powershell
# Descarga desde https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/install-and-setup/installation/
```

### 2. Login a Cloudflare

```powershell
cloudflared tunnel login
```

### 3. Crear túnel

```powershell
cloudflared tunnel create tpi-backend
```

### 4. Configurar túnel

Crea `cloudflared-config.yml`:

```yaml
tunnel: TU_TUNNEL_ID
credentials-file: C:/Users/TU_USUARIO/.cloudflared/TU_TUNNEL_ID.json

ingress:
  - hostname: auth-tpi.tudominio.com
    service: http://localhost:5001
  - hostname: messages-tpi.tudominio.com
    service: http://localhost:5002
  - hostname: groups-tpi.tudominio.com
    service: http://localhost:5003
  - service: http_status:404
```

### 5. Iniciar túnel

```powershell
cloudflared tunnel run tpi-backend
```

---

## 🛠️ Opción 3: Solo Frontend Estático (MÁS SIMPLE)

Si quieres algo súper simple para demostrar:

### 1. Exportar el frontend como archivos estáticos

```powershell
# Crear carpeta
New-Item -ItemType Directory -Force -Path "frontend-static"

# Copiar archivos
Copy-Item "src/UI/wwwroot/*" "frontend-static/" -Recurse
```

### 2. Actualizar config.js con tu IP pública

```javascript
window.API_CONFIG = {
    auth: 'http://TU_IP_PUBLICA:5001/api/auth',
    messages: 'http://TU_IP_PUBLICA:5002/api/messages',
    groups: 'http://TU_IP_PUBLICA:5003/api/groups',
    chatHub: 'http://TU_IP_PUBLICA:5002/hubs/chat'
};
```

### 3. Subir a GitHub Pages / Netlify / Vercel

**GitHub Pages:**
```powershell
# Crear repo gh-pages
git checkout -b gh-pages
git add frontend-static
git commit -m "Deploy to GitHub Pages"
git push origin gh-pages
```

Activa GitHub Pages en Settings → Pages → Source: gh-pages

**Netlify Drop:**
- Ve a https://app.netlify.com/drop
- Arrastra la carpeta `frontend-static`
- ¡Listo!

---

## 🔧 Script de Inicio Automático

Crea `start-local-backend.ps1`:

```powershell
# Iniciar todos los servicios en background

Write-Host "Iniciando Auth Service..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'src/Services/Auth.Service'; dotnet run"

Start-Sleep -Seconds 2

Write-Host "Iniciando Messages Service..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'src/Services/Messages.Service'; dotnet run"

Start-Sleep -Seconds 2

Write-Host "Iniciando Groups Service..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'src/Services/Groups.Service'; dotnet run"

Write-Host ""
Write-Host "Todos los servicios iniciados!" -ForegroundColor Cyan
Write-Host "Presiona Ctrl+C en cada ventana para detener" -ForegroundColor Yellow
```

Luego ejecuta:
```powershell
.\start-local-backend.ps1
```

---

## 📊 Comparación de Opciones

| Característica | ngrok | Cloudflare Tunnel | Solo IP Pública |
|---------------|-------|-------------------|-----------------|
| **Costo** | Gratis | Gratis | Gratis |
| **HTTPS** | ✅ Sí | ✅ Sí | ❌ No (solo HTTP) |
| **Configuración** | Fácil | Media | Muy Fácil |
| **Límites** | 1 sesión concurrente | Sin límites | Depende de tu ISP |
| **Requiere dominio** | No | Opcional | No |
| **Mejor para** | Desarrollo/Demo | Producción ligera | Demo rápido |

---

## ⚠️ Consideraciones Importantes

### 1. Tu PC debe estar encendida
- Los servicios backend corren en tu PC
- Si apagas la PC, el backend deja de funcionar
- El frontend seguirá accesible pero sin datos

### 2. Seguridad
- ngrok/Cloudflare ya tienen HTTPS
- No expongas credenciales sensibles
- Considera usar JWT con expiración corta

### 3. Rendimiento
- Tu velocidad de subida de internet afectará la velocidad
- Para demos de facultad es más que suficiente

### 4. Límites del Free Tier de ngrok
- 1 usuario autenticado
- 1 agente/sesión
- 40 conexiones/minuto
- Más que suficiente para un proyecto de facultad

---

## 🎓 Flujo Completo Recomendado

### Para la demostración/entrega:

1. **Antes de la demo:**
   ```powershell
   # Iniciar servicios backend
   .\start-local-backend.ps1
   
   # Iniciar ngrok
   ngrok start --all
   ```

2. **Durante la demo:**
   - Compartir la URL de Vercel: `https://tu-app.vercel.app`
   - El backend corre en tu PC (invisible para el usuario)

3. **Después de la demo:**
   - Cerrar ngrok (Ctrl+C)
   - Cerrar servicios backend
   - El frontend sigue online pero sin funcionalidad

### Para desarrollo continuo:

Mantén ngrok corriendo mientras desarrollas:
```powershell
ngrok start --all
```

Los cambios en el código se reflejan al reiniciar el servicio correspondiente.

---

## 🆘 Solución de Problemas

### ngrok dice "Session Limit Exceeded"
- Solo puedes tener 1 sesión activa en el free tier
- Cierra otras sesiones de ngrok que tengas abiertas
- O usa Cloudflare Tunnel (sin límite de sesiones)

### CORS Error
- Verifica que agregaste la URL de Vercel en `appsettings.json`
- Reinicia los servicios backend después de cambiar CORS

### SignalR no conecta
- ngrok free tier a veces tiene problemas con WebSockets
- Usa Cloudflare Tunnel para mejor soporte de WebSockets
- O configura ngrok con `--scheme=https`

### El frontend no encuentra los servicios
- Verifica que las URLs en `config.js` coincidan con las de ngrok
- Asegúrate de que ngrok esté corriendo
- Revisa que los servicios backend estén activos

---

## 💡 Recomendación Final

**Para tu proyecto de facultad:**

1. **Usa ngrok** para exponer tu backend (súper fácil, 5 minutos setup)
2. **Despliega el frontend en Vercel** (gratis, ilimitado, profesional)
3. **Mantén tu PC encendida durante la demo/presentación**

**Ventajas:**
- ✅ 100% gratis
- ✅ Tus datos en tu PC (privacidad)
- ✅ URL profesional para mostrar
- ✅ HTTPS automático
- ✅ Sin límites de uso razonables para facultad

**Script de inicio rápido:**
```powershell
# 1. Iniciar backend
.\start-local-backend.ps1

# 2. Iniciar ngrok (en otra terminal)
ngrok start --all

# 3. Copiar las URLs de ngrok y actualizar config.js

# 4. Desplegar frontend
cd src/UI-static
vercel --prod

# ¡LISTO! Compartir la URL de Vercel
```

---

**¿Necesitas ayuda con algún paso específico?** Puedo ayudarte a configurar cualquiera de estas opciones.
