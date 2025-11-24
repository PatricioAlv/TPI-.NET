# 🚀 Guía de Deployment en Railway (Plan Gratuito)

## 📋 Prerequisitos

1. Cuenta en [Railway.app](https://railway.app)
2. Cuenta en GitHub
3. Git instalado

## ⚡ Arquitectura Simplificada

Como Railway limita servicios en plan gratuito, vamos a deployar **TODO EN UN SOLO CONTENEDOR**:
- ✅ 1 servicio que contiene Auth + Messages + Groups + UI
- ✅ 1 base de datos PostgreSQL
- ✅ Total: 2 servicios (dentro del límite gratuito)

## 🔧 Paso 1: Preparar el Repositorio

```bash
# Agrega todos los archivos (incluyendo Dockerfile y start-all.sh)
git add .

# Commit
git commit -m "Add Railway deployment files"

# Si no has creado el repo en GitHub, hazlo ahora
# Luego conecta:
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
4. Copia la variable `DATABASE_URL` que Railway genera

### 2.3 Configurar el Servicio Principal

Railway detectará automáticamente el `Dockerfile` en la raíz. Ahora configura las variables:

1. Click en tu servicio
2. Ve a **"Variables"**
3. Agrega estas variables:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
JwtSettings__Secret=tu-super-secreto-seguro-de-al-menos-32-caracteres-cambiar-esto
JwtSettings__Issuer=ChatSystem
JwtSettings__Audience=ChatUsers
JwtSettings__AccessTokenExpirationMinutes=60
JwtSettings__RefreshTokenExpirationDays=7
```

**⚠️ IMPORTANTE**: Cambia `JwtSettings__Secret` a un valor único y seguro

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

### 2.4 Generar Dominio Público

1. En tu servicio principal, ve a **Settings**
2. En **Networking** → **Public Networking**
3. Click en **"Generate Domain"**
4. Guarda la URL generada (ej: `https://tpi-net-production-xxxx.up.railway.app`)

## 🔄 Paso 3: Actualizar URLs en el Código

Como todo corre en el mismo contenedor, los servicios backend están en localhost internamente.

### 3.1 Actualizar app.js

Edita `src/UI/wwwroot/app.js` y cambia las URLs:

```javascript
const API_URLS = {
    auth: 'http://localhost:5001/api/auth',
    messages: 'http://localhost:5002/api/messages',
    groups: 'http://localhost:5003/api/groups',
    chatHub: 'http://localhost:5002/chatHub'
};
```

**NOTA**: Usamos localhost porque todos los servicios corren en el mismo contenedor. El usuario accede por el puerto 8080 (UI), que internamente se comunica con los otros puertos.

### 3.2 Actualizar CORS

Los servicios necesitan permitir solicitudes desde el dominio Railway. En cada `Program.cs`:

**`src/Services/Auth.Service/Program.cs`**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "https://tpi-net-production-xxxx.up.railway.app", // Cambia por tu URL
            "http://localhost:8080",
            "null"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

Repite en:
- `src/Services/Messages.Service/Program.cs`
- `src/Services/Groups.Service/Program.cs`

### 3.3 Commit y Push

```bash
git add .
git commit -m "Update for single-container Railway deployment"
git push origin main
```

Railway redesplegará automáticamente.

## 🗄️ Paso 4: Ejecutar Migraciones

Necesitas acceder al contenedor para ejecutar las migraciones:

### Opción A: Railway CLI

```bash
# Instala Railway CLI
npm i -g @railway/cli

# Login
railway login

# Link al proyecto
railway link

# Ejecuta bash en el contenedor
railway shell

# Dentro del contenedor:
cd /app/auth && dotnet ef database update
cd /app/messages && dotnet ef database update
cd /app/groups && dotnet ef database update
```

### Opción B: Conectar directamente a PostgreSQL

Desde tu máquina local:

```bash
# Copia la DATABASE_URL de Railway
# Ejecuta migraciones localmente apuntando a Railway DB
$env:ConnectionStrings__DefaultConnection="TU_DATABASE_URL_DE_RAILWAY"

cd src/Services/Auth.Service
dotnet ef database update

cd ../Messages.Service
dotnet ef database update

cd ../Groups.Service
dotnet ef database update
```

## ✅ Paso 5: Verificar Deployment

1. Abre tu URL de Railway: `https://tpi-net-production-xxxx.up.railway.app`
2. Deberías ver la UI de login/registro
3. Registra un usuario
4. Inicia sesión
5. Prueba enviar mensajes
6. Crea un grupo

## 🐛 Troubleshooting

### Ver Logs

1. En Railway Dashboard, click en tu servicio
2. Ve a **"Deployments"** 
3. Click en el deployment activo
4. Click en **"View Logs"**

Verás logs de todos los servicios (Auth, Messages, Groups, UI)

### Problemas Comunes

**Los servicios no inician:**
- Revisa los logs, busca errores de conexión a DB
- Verifica que `${{Postgres.DATABASE_URL}}` esté configurado
- Asegúrate de que `start-all.sh` tenga permisos de ejecución
**Error de Base de Datos:**
- Ejecuta las migraciones según Paso 4
- Verifica que la conexión a PostgreSQL funcione

**SignalR no conecta:**
- Verifica que el ChatHub esté usando localhost:5002
- Revisa los logs del contenedor
- Asegúrate de que CORS permita tu dominio Railway

**Build falla:**
- Revisa los logs de build en Railway
- Verifica que `start-all.sh` esté en el repositorio
- Asegúrate de que todos los .csproj existan

## 💰 Costos

Con esta arquitectura simplificada:
- ✅ **1 servicio principal** (todos los microservicios en uno)
- ✅ **1 PostgreSQL**
- ✅ **Total: 2 servicios** = Perfecto para plan gratuito de Railway
- 💵 **$5 USD/mes gratis** es más que suficiente

## 📊 Monitoreo

En Railway Dashboard:
- CPU y RAM del contenedor único
- Requests por segundo
- Logs consolidados de todos los servicios
- Métricas de PostgreSQL

## 🔐 Seguridad

**¡IMPORTANTE!**
- ✅ Variables de entorno en Railway, NO en código
- ✅ Cambia `JwtSettings__Secret` a algo único
- ✅ No subas secrets a GitHub
- ✅ Usa `${{Postgres.DATABASE_URL}}` para la DB

## 📝 Arquitectura del Contenedor

```
Puerto 8080 (público) → UI
    ↓
Puerto 5001 (interno) → Auth Service
Puerto 5002 (interno) → Messages Service + SignalR
Puerto 5003 (interno) → Groups Service
    ↓
PostgreSQL (Railway DB)
```

Todo corre en un solo contenedor, Railway expone solo el puerto 8080.

---

¡Listo! Tu sistema completo en 1 solo servicio Railway 🎉

