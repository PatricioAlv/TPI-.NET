# 🚀 Guía de Deployment en Railway

## 📋 Prerequisitos

1. Cuenta en [Railway.app](https://railway.app)
2. Cuenta en GitHub
3. Git instalado

## 🔧 Paso 1: Preparar el Repositorio

```bash
# Inicializa git si no lo has hecho
git init

# Agrega todos los archivos
git add .

# Commit inicial
git commit -m "Initial commit - Sistema de Mensajería"

# Crea repositorio en GitHub y conecta
git remote add origin https://github.com/TU_USUARIO/TPI-NET.git
git branch -M main
git push -u origin main
```

## 🛤️ Paso 2: Configurar Railway

### 2.1 Crear Nuevo Proyecto

1. Ve a [Railway.app](https://railway.app) y haz login
2. Click en **"New Project"**
3. Selecciona **"Deploy from GitHub repo"**
4. Autoriza Railway a acceder a tu GitHub
5. Selecciona tu repositorio `TPI-NET`

### 2.2 Agregar PostgreSQL

1. En tu proyecto Railway, click en **"+ New"**
2. Selecciona **"Database" → "PostgreSQL"**
3. Railway creará automáticamente la base de datos

### 2.3 Crear Servicios

Vas a crear **4 servicios** desde tu repositorio:

#### **Servicio 1: Auth Service**

1. Click **"+ New" → "GitHub Repo"**
2. Selecciona tu repo
3. En **Settings**:
   - **Service Name**: `auth-service`
   - **Root Directory**: deja vacío
   - **Dockerfile Path**: `src/Services/Auth.Service/Dockerfile`
4. En **Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:8080
   ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
   JwtSettings__Secret=tu-super-secreto-de-al-menos-32-caracteres-aqui-cambiar
   JwtSettings__Issuer=ChatSystem
   JwtSettings__Audience=ChatUsers
   JwtSettings__AccessTokenExpirationMinutes=60
   JwtSettings__RefreshTokenExpirationDays=7
   ```

#### **Servicio 2: Messages Service**

1. Click **"+ New" → "GitHub Repo"**
2. En **Settings**:
   - **Service Name**: `messages-service`
   - **Root Directory**: deja vacío
   - **Dockerfile Path**: `src/Services/Messages.Service/Dockerfile`
3. En **Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:8080
   ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
   JwtSettings__Secret=tu-super-secreto-de-al-menos-32-caracteres-aqui-cambiar
   JwtSettings__Issuer=ChatSystem
   JwtSettings__Audience=ChatUsers
   ```

#### **Servicio 3: Groups Service**

1. Click **"+ New" → "GitHub Repo"**
2. En **Settings**:
   - **Service Name**: `groups-service`
   - **Root Directory**: deja vacío
   - **Dockerfile Path**: `src/Services/Groups.Service/Dockerfile`
3. En **Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:8080
   ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
   JwtSettings__Secret=tu-super-secreto-de-al-menos-32-caracteres-aqui-cambiar
   JwtSettings__Issuer=ChatSystem
   JwtSettings__Audience=ChatUsers
   ```

#### **Servicio 4: UI**

1. Click **"+ New" → "GitHub Repo"**
2. En **Settings**:
   - **Service Name**: `ui`
   - **Root Directory**: deja vacío
   - **Dockerfile Path**: `src/UI/Dockerfile`
3. En **Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:8080
   ```

### 2.4 Generar Dominios Públicos

Para cada servicio:

1. Ve a **Settings** del servicio
2. En **Networking** → **Public Networking**
3. Click en **"Generate Domain"**
4. Guarda las URLs generadas

Ejemplo de URLs que obtendrás:
```
auth-service: https://auth-service-production-xxxx.up.railway.app
messages-service: https://messages-service-production-xxxx.up.railway.app
groups-service: https://groups-service-production-xxxx.up.railway.app
ui: https://ui-production-xxxx.up.railway.app
```

## 🔄 Paso 3: Actualizar Configuración

Necesitas actualizar la UI para que apunte a las URLs de Railway:

### 3.1 Actualizar app.js

Edita `src/UI/wwwroot/app.js` y cambia las URLs al inicio del archivo:

```javascript
const API_URLS = {
    auth: 'https://auth-service-production-xxxx.up.railway.app/api/auth',
    messages: 'https://messages-service-production-xxxx.up.railway.app/api/messages',
    groups: 'https://groups-service-production-xxxx.up.railway.app/api/groups',
    chatHub: 'https://messages-service-production-xxxx.up.railway.app/chatHub'
};
```

### 3.2 Actualizar CORS en los servicios

En `src/Services/Auth.Service/Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "https://ui-production-xxxx.up.railway.app", // Tu URL de Railway UI
            "http://localhost:8080",
            "null"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

Repite lo mismo en:
- `src/Services/Messages.Service/Program.cs`
- `src/Services/Groups.Service/Program.cs`

### 3.3 Commit y Push

```bash
git add .
git commit -m "Update URLs for Railway deployment"
git push origin main
```

Railway detectará automáticamente los cambios y redesplegará.

## 🗄️ Paso 4: Ejecutar Migraciones

### Opción A: Desde Railway CLI (Recomendado)

```bash
# Instala Railway CLI
npm i -g @railway/cli

# Login
railway login

# Link al proyecto
railway link

# Ejecuta migraciones para cada servicio
railway run --service auth-service dotnet ef database update
railway run --service messages-service dotnet ef database update
railway run --service groups-service dotnet ef database update
```

### Opción B: Manualmente vía SSH

En Railway Dashboard:
1. Ve a cada servicio
2. Click en **"Settings" → "Build & Deploy"**
3. Agrega comando de inicio:
   ```
   dotnet ef database update && dotnet Auth.Service.dll
   ```

## ✅ Paso 5: Verificar Deployment

1. Abre la URL de tu UI: `https://ui-production-xxxx.up.railway.app`
2. Registra un usuario
3. Inicia sesión
4. Prueba enviar mensajes
5. Crea un grupo

## 🐛 Troubleshooting

### Ver Logs

En cada servicio de Railway:
1. Ve a **"Deployments"**
2. Click en el deployment activo
3. Ve a **"View Logs"**

### Problemas Comunes

**Error de CORS:**
- Verifica que las URLs en CORS coincidan exactamente con tu dominio de Railway
- Asegúrate de tener `AllowCredentials()` configurado
- No olvides el protocolo HTTPS

**Error de Base de Datos:**
- Verifica que `${{Postgres.DATABASE_URL}}` esté en las variables
- Asegúrate de que el servicio PostgreSQL esté corriendo
- Ejecuta las migraciones

**SignalR no conecta:**
- Verifica que las URLs en `app.js` sean HTTPS
- Revisa los logs del Messages Service
- Asegúrate de que CORS permita tu dominio

**Build falla:**
- Revisa que el Dockerfile Path sea correcto
- Verifica los logs de build en Railway
- Asegúrate de que todos los archivos .csproj existan

## 💰 Costos

Railway ofrece:
- **$5 USD/mes gratis** (suficiente para este proyecto en desarrollo)
- Monitoreo del uso en el dashboard
- Alertas cuando te acercas al límite

## 📊 Monitoreo

En Railway Dashboard puedes ver:
- CPU y RAM usage
- Requests por segundo
- Logs en tiempo real
- Métricas de base de datos

## 🔐 Seguridad

**¡IMPORTANTE!** 
- ✅ Usa variables de entorno en Railway, no hardcodees secrets
- ✅ Cambia `JwtSettings__Secret` a un valor único y seguro
- ✅ Mantén tus connection strings en variables de entorno
- ❌ No subas secrets a GitHub

## 📝 Notas Adicionales

- Cada push a `main` redesplegará automáticamente
- Puedes ver los deployments en la pestaña "Deployments"
- Railway te notificará por email si algo falla
- Los logs son en tiempo real y muy útiles para debug

---

¡Listo! Tu sistema de mensajería está en producción 🎉
