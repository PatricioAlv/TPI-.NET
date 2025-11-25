# 🎯 Resumen de Configuración para Render.com

## ✅ Archivos Creados

### Configuración de Despliegue
- **`render.yaml`** - Blueprint completo con 4 servicios web + 3 bases de datos PostgreSQL
- **`RENDER_DEPLOYMENT.md`** - Guía detallada paso a paso
- **`prepare-render.ps1`** - Script de verificación pre-despliegue
- **`.dockerignore`** - Optimización de builds Docker

### Configuración del Frontend
- **`src/UI/wwwroot/config.js`** - URLs para desarrollo (localhost)
- **`src/UI/wwwroot/config.production.js`** - URLs para producción (Render)

## 🔧 Modificaciones Realizadas

### Servicios Backend (Auth, Messages, Groups)
- ✅ Agregado endpoint `/health` para health checks
- ✅ Configurado auto-ejecución de migraciones en producción
- ✅ CORS ya configurado para usar variables de entorno

### Frontend (UI)
- ✅ Actualizado `index.html` para cargar `config.js`
- ✅ Modificado `app.js` para usar configuración dinámica
- ✅ Dockerfile actualizado para usar config de producción

## 📦 Servicios Configurados en Render

| Servicio | Tipo | Puerto | URL Esperada |
|----------|------|--------|--------------|
| tpi-auth-service | Web Service | 8080 | https://tpi-auth-service.onrender.com |
| tpi-messages-service | Web Service | 8080 | https://tpi-messages-service.onrender.com |
| tpi-groups-service | Web Service | 8080 | https://tpi-groups-service.onrender.com |
| tpi-ui | Web Service | 8080 | https://tpi-ui.onrender.com |
| tpi-auth-db | PostgreSQL | 5432 | (conexión interna) |
| tpi-messages-db | PostgreSQL | 5432 | (conexión interna) |
| tpi-groups-db | PostgreSQL | 5432 | (conexión interna) |

## 🚀 Próximos Pasos

### 1. Conectar con Render
```
1. Ve a https://dashboard.render.com
2. Haz clic en "New +" → "Blueprint"
3. Selecciona el repositorio "PatricioAlv/TPI-.NET"
4. Render detectará automáticamente el render.yaml
5. Haz clic en "Apply"
```

### 2. Configurar JWT Secret (IMPORTANTE)
Después del despliegue inicial:
1. Ir a `tpi-auth-service` → Environment
2. Copiar el valor de `JWT__Secret` (generado automáticamente)
3. Agregar la misma variable en `tpi-messages-service` y `tpi-groups-service`

**O generar uno manualmente:**
```bash
openssl rand -base64 32
```

### 3. Esperar el Despliegue
- Bases de datos: ~2-3 minutos
- Servicios: ~10-15 minutos cada uno

### 4. Probar la Aplicación
- Abrir: https://tpi-ui.onrender.com
- Registrar usuario
- Probar mensajería 1:1
- Crear grupos
- Verificar read receipts

## ⚙️ Variables de Entorno Configuradas

### Auth Service
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection` → Desde tpi-auth-db
- `JWT__Secret` → Auto-generado
- `CORS__AllowedOrigins=https://tpi-ui.onrender.com`

### Messages Service
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection` → Desde tpi-messages-db
- `JWT__Secret` → **Debe sincronizarse manualmente**
- `CORS__AllowedOrigins=https://tpi-ui.onrender.com`

### Groups Service
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection` → Desde tpi-groups-db
- `JWT__Secret` → **Debe sincronizarse manualmente**
- `CORS__AllowedOrigins=https://tpi-ui.onrender.com`

### UI Service
- `ASPNETCORE_ENVIRONMENT=Production`
- URLs de servicios configuradas en `config.production.js`

## 🔍 Health Checks

Todos los servicios tienen endpoints de health:
- https://tpi-auth-service.onrender.com/health
- https://tpi-messages-service.onrender.com/health
- https://tpi-groups-service.onrender.com/health

## 📊 Características del Free Tier

### Bases de Datos PostgreSQL
- ✅ 90 días de uso gratuito
- 256 MB RAM
- 1 GB almacenamiento
- ⚠️ Los datos se pierden después de 90 días

### Web Services
- ✅ 750 horas/mes gratis
- Se suspenden después de 15 minutos de inactividad
- ~30-60 segundos para despertar
- HTTPS automático

## 🛠️ Solución de Problemas

### Si el build falla:
1. Revisar logs en el dashboard de Render
2. Verificar que el código compila localmente
3. Revisar que los Dockerfiles son correctos

### Si no conecta a la base de datos:
1. Esperar a que las DBs estén completamente disponibles
2. Verificar variables de entorno
3. Revisar logs del servicio

### Si hay error de CORS:
1. Verificar `CORS__AllowedOrigins` en los servicios
2. Asegurarse de que NO hay espacios en las URLs
3. Reiniciar los servicios

### Si SignalR no funciona:
1. Verificar que `tpi-messages-service` está corriendo
2. Abrir consola del navegador (F12) para ver errores
3. Esperar ~30 segundos si el servicio estaba dormido

## 📝 Archivos Eliminados (Railway)

Los siguientes archivos de Railway fueron eliminados:
- ~~Dockerfile~~ (raíz, reemplazado por Dockerfiles individuales)
- ~~railway.json~~
- ~~RAILWAY_DEPLOYMENT.md~~
- ~~prepare-railway.ps1~~
- ~~start-all.sh~~

## ✨ Commit Realizado

```
commit 09c46c1
Author: PatricioAlv
Date: 24 de noviembre de 2025

Configurar despliegue en Render.com
- Agregar render.yaml con 7 servicios
- Health checks en todos los backends
- Auto-migraciones en producción
- Configuración dinámica del frontend
- Documentación completa de despliegue
```

## 🎓 URLs Finales

Una vez desplegado:
- **App Principal**: https://tpi-ui.onrender.com
- **API Auth (Swagger)**: https://tpi-auth-service.onrender.com/swagger
- **API Messages (Swagger)**: https://tpi-messages-service.onrender.com/swagger
- **API Groups (Swagger)**: https://tpi-groups-service.onrender.com/swagger

---

**Estado**: ✅ TODO LISTO PARA DESPLEGAR
**Última actualización**: 24 de noviembre de 2025
