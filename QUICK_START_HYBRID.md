# Paso a Paso: Despliegue Híbrido (Backend en tu PC + Frontend en Vercel)

## 📝 Resumen
- **Backend**: Corre en tu PC (Auth, Messages, Groups + PostgreSQL)
- **Frontend**: Desplegado en Vercel (gratis, ilimitado)
- **Túnel**: ngrok expone tu PC al internet (gratis)

## 🚀 Pasos Rápidos

### 1. Instalar ngrok (Solo una vez)

**Opción A - Con Chocolatey:**
```powershell
choco install ngrok
```

**Opción B - Descarga manual:**
1. Ve a https://ngrok.com/download
2. Descarga `ngrok-v3-stable-windows-amd64.zip`
3. Extrae `ngrok.exe` a `C:\Windows\System32\` (o agrega a PATH)

### 2. Configurar ngrok (Solo una vez)

1. Crea cuenta en https://dashboard.ngrok.com/signup
2. Copia tu authtoken de https://dashboard.ngrok.com/get-started/your-authtoken
3. Configura ngrok:

```powershell
ngrok config add-authtoken TU_TOKEN_AQUI
```

4. Edita `ngrok.yml` y reemplaza `TU_TOKEN_AQUI` con tu token

### 3. Actualizar CORS (Solo una vez)

Edita los siguientes archivos para permitir que Vercel acceda a tu backend:

**src/Services/Auth.Service/appsettings.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:8080",
      "https://*.vercel.app"
    ]
  }
}
```

**src/Services/Messages.Service/appsettings.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:8080",
      "https://*.vercel.app"
    ]
  }
}
```

**src/Services/Groups.Service/appsettings.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:8080",
      "https://*.vercel.app"
    ]
  }
}
```

### 4. Iniciar Backend (Cada vez que quieras usarlo)

Abre PowerShell en la raíz del proyecto:

```powershell
.\start-local-backend.ps1
```

Esto abrirá 3 ventanas con:
- Auth Service (puerto 5001)
- Messages Service (puerto 5002)
- Groups Service (puerto 5003)

**Espera ~30 segundos** hasta que veas "Now listening on: http://localhost:XXXX"

### 5. Iniciar ngrok (Cada vez que quieras usarlo)

En otra terminal de PowerShell:

```powershell
ngrok start --all --config ngrok.yml
```

ngrok te mostrará algo como:

```
Forwarding  https://abc123.ngrok-free.app -> http://localhost:5001
Forwarding  https://def456.ngrok-free.app -> http://localhost:5002
Forwarding  https://ghi789.ngrok-free.app -> http://localhost:5003
```

**¡Copia estas URLs!** Las necesitarás en el siguiente paso.

### 6. Configurar Frontend con las URLs de ngrok

Edita `src/UI-static/config.js` y reemplaza las URLs:

```javascript
window.API_CONFIG = {
    auth: 'https://abc123.ngrok-free.app/api/auth',        // ← URL de Auth
    messages: 'https://def456.ngrok-free.app/api/messages', // ← URL de Messages
    groups: 'https://ghi789.ngrok-free.app/api/groups',     // ← URL de Groups
    chatHub: 'https://def456.ngrok-free.app/hubs/chat'      // ← URL de Messages (SignalR)
};
```

### 7. Desplegar Frontend en Vercel (Solo primera vez)

```powershell
# Instalar Vercel CLI (solo una vez)
npm install -g vercel

# Login a Vercel (solo una vez)
vercel login

# Desplegar
cd C:\Proyectos\TPI-.NET
vercel --prod
```

Vercel te hará algunas preguntas:
- **Set up and deploy?** → `Y`
- **Which scope?** → Selecciona tu cuenta
- **Link to existing project?** → `N`
- **Project name?** → `tpi-messaging` (o el que quieras)
- **Directory?** → `.` (punto, es la raíz)
- **Override settings?** → `N`

Vercel desplegará y te dará una URL como:
```
✅ Production: https://tpi-messaging.vercel.app
```

### 8. Probar la Aplicación

Abre la URL de Vercel en tu navegador:
```
https://tpi-messaging.vercel.app
```

**¡Listo!** Deberías poder:
- Registrarte
- Iniciar sesión
- Enviar mensajes
- Crear grupos
- Ver read receipts

## 🔄 Actualizar el Frontend (Después de cambios)

Si haces cambios en el código del frontend:

```powershell
# 1. Copiar cambios a UI-static
Copy-Item "src\UI\wwwroot\*" "src\UI-static\" -Recurse -Force

# 2. Re-desplegar en Vercel
vercel --prod
```

## 🛑 Detener Todo

1. **Cerrar ngrok**: Presiona `Ctrl+C` en la terminal de ngrok
2. **Cerrar servicios backend**: Cierra las 3 ventanas de PowerShell

## ⚠️ Importante

### URLs de ngrok cambian cada vez
Las URLs que te da ngrok **cambian cada vez que lo reinicias**.

Cada vez que inicies ngrok:
1. Copia las nuevas URLs
2. Actualiza `src/UI-static/config.js`
3. Re-despliega en Vercel con `vercel --prod`

### Mantén tu PC encendida
Mientras quieras que la app esté disponible:
- Tu PC debe estar encendida
- Los servicios backend deben estar corriendo
- ngrok debe estar activo

### Para presentaciones/demos
Inicia todo ~5-10 minutos antes:
1. `.\start-local-backend.ps1` → Espera que todos inicien
2. `ngrok start --all --config ngrok.yml` → Copia URLs
3. Actualiza `config.js` con las URLs
4. `vercel --prod` → Despliega
5. **Comparte la URL de Vercel** (no la de ngrok)

## 💡 Tips

### ngrok con dominio fijo (Opcional - Requiere cuenta paid)
Si pagas $8/mes en ngrok, puedes tener un dominio fijo que no cambia:
```yaml
tunnels:
  auth:
    proto: http
    addr: 5001
    domain: tpi-auth.ngrok-free.app  # ← Fijo
```

Así no tendrías que actualizar `config.js` cada vez.

### Alternativa: Cloudflare Tunnel (Gratis, dominio fijo)
Si quieres evitar actualizar config.js cada vez, usa Cloudflare Tunnel (ver HYBRID_DEPLOYMENT.md).

### Solo para desarrollo local
Si solo quieres probar localmente sin desplegar:
```powershell
# Iniciar backend
.\start-local-backend.ps1

# Abrir frontend local
cd src/UI
dotnet run
# Abre http://localhost:8080
```

## 🆘 Problemas Comunes

### "ngrok not found"
- Asegúrate de haber instalado ngrok
- O usa la ruta completa: `C:\path\to\ngrok.exe start --all`

### "authtoken required"
```powershell
ngrok config add-authtoken TU_TOKEN_DE_NGROK
```

### Error de CORS
- Verifica que agregaste `"https://*.vercel.app"` en los `appsettings.json`
- Reinicia los servicios backend

### Frontend no encuentra backend
- Verifica que las URLs en `config.js` coinciden con ngrok
- Asegúrate de que ngrok está corriendo
- Revisa que los servicios backend estén activos (las 3 ventanas)

### SignalR no conecta
- Verifica que `chatHub` en `config.js` apunta a Messages Service
- ngrok free tier puede tener issues con WebSockets, dale unos segundos

---

## ✅ Checklist Antes de la Demo

- [ ] PostgreSQL corriendo
- [ ] Backend iniciado (`start-local-backend.ps1`)
- [ ] ngrok corriendo (`ngrok start --all`)
- [ ] `config.js` actualizado con URLs de ngrok
- [ ] Frontend desplegado en Vercel (`vercel --prod`)
- [ ] Probado el registro/login
- [ ] Probado envío de mensajes
- [ ] URL de Vercel lista para compartir

**¡Éxito con tu proyecto de facultad!** 🎓
