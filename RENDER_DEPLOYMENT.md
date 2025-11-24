# 🚀 Despliegue en Render.com - TPI Sistema de Mensajería

Esta guía te ayudará a desplegar el Sistema de Mensajería en Render.com de forma gratuita.

## 📋 Requisitos Previos

1. **Cuenta en Render.com**: Crea una cuenta gratuita en [render.com](https://render.com)
2. **Cuenta en GitHub**: Tu repositorio debe estar en GitHub
3. **Git**: Asegúrate de tener los últimos cambios commiteados

## 🔧 Configuración Inicial

### 1. Preparar el Repositorio

Asegúrate de que todos los archivos estén commiteados:

```bash
git add .
git commit -m "Preparar para despliegue en Render"
git push origin master
```

### 2. Conectar Render con GitHub

1. Ve a [dashboard.render.com](https://dashboard.render.com)
2. Haz clic en **"New +"** → **"Blueprint"**
3. Conecta tu repositorio de GitHub (`PatricioAlv/TPI-.NET`)
4. Render detectará automáticamente el archivo `render.yaml`

## 📦 Servicios que se Desplegarán

El archivo `render.yaml` configurará automáticamente:

### **Bases de Datos (PostgreSQL - Free Tier)**
- `tpi-auth-db` - Base de datos de autenticación
- `tpi-messages-db` - Base de datos de mensajes
- `tpi-groups-db` - Base de datos de grupos

### **Servicios Web (Docker - Free Tier)**
- `tpi-auth-service` - Servicio de autenticación y JWT
- `tpi-messages-service` - Servicio de mensajes y SignalR
- `tpi-groups-service` - Servicio de grupos
- `tpi-ui` - Interfaz de usuario (frontend)

## 🚀 Proceso de Despliegue

### 1. Desplegar desde Blueprint

1. En Render Dashboard, haz clic en **"New +"** → **"Blueprint"**
2. Selecciona tu repositorio `TPI-.NET`
3. Render mostrará todos los servicios definidos en `render.yaml`
4. Haz clic en **"Apply"**

### 2. Configurar el JWT Secret

⚠️ **IMPORTANTE**: Después del despliegue inicial, debes sincronizar el `JWT__Secret` entre todos los servicios:

1. Ve a `tpi-auth-service` → **Environment**
2. Copia el valor de `JWT__Secret` (se generó automáticamente)
3. Ve a `tpi-messages-service` → **Environment**
   - Agrega la variable `JWT__Secret` con el mismo valor
4. Ve a `tpi-groups-service` → **Environment**
   - Agrega la variable `JWT__Secret` con el mismo valor

**O bien**, genera un secreto fuerte manualmente:

```bash
# Generar un secreto seguro de 256 bits
openssl rand -base64 32
```

Y usa ese valor en los 3 servicios.

### 3. Esperar el Despliegue

- **Bases de datos**: ~2-3 minutos
- **Servicios**: ~10-15 minutos (build + deploy)

Puedes ver los logs en tiempo real haciendo clic en cada servicio.

## 🔗 URLs de los Servicios

Una vez desplegado, tus servicios estarán disponibles en:

- **Auth Service**: `https://tpi-auth-service.onrender.com`
- **Messages Service**: `https://tpi-messages-service.onrender.com`
- **Groups Service**: `https://tpi-groups-service.onrender.com`
- **UI (Frontend)**: `https://tpi-ui.onrender.com` ← **Esta es la URL pública**

## 🗄️ Migraciones de Base de Datos

Las migraciones se ejecutarán automáticamente la primera vez que cada servicio se inicie:

```csharp
// Cada servicio tiene este código en Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContext>();
    db.Database.Migrate(); // Aplica migraciones automáticamente
}
```

### Verificar Migraciones

Puedes verificar que las migraciones se aplicaron correctamente en los logs:

1. Ve a cada servicio en Render Dashboard
2. Haz clic en **"Logs"**
3. Busca mensajes como: `Applied migration 'InitialCreate'`

## 🧪 Probar la Aplicación

1. Abre `https://tpi-ui.onrender.com`
2. Registra un nuevo usuario
3. Inicia sesión
4. Crea conversaciones 1:1 y grupos
5. Prueba el envío de mensajes en tiempo real
6. Verifica los indicadores de lectura (✓ y ✓✓)

## ⚠️ Limitaciones del Free Tier

### Render Free Tier incluye:

✅ **Bases de datos PostgreSQL**
- 90 días de uso gratuito
- Luego expiran (necesitarás crear nuevas)
- 256 MB RAM
- 1 GB almacenamiento

✅ **Servicios Web**
- 750 horas/mes gratis (suficiente para varios servicios)
- Se suspenden después de 15 minutos de inactividad
- Tardan ~30-60 segundos en despertar (primera request después de inactividad)

### Consejos para el Free Tier:

1. **Tiempo de arranque**: La primera request después de inactividad será lenta (~30-60 segundos)
2. **Mantener activo**: Puedes usar servicios como [cron-job.org](https://cron-job.org) para hacer ping cada 10 minutos
3. **Base de datos temporal**: Los datos se perderán después de 90 días en el free tier

## 🔧 Solución de Problemas

### Error: "Build failed"

1. Revisa los logs del servicio que falló
2. Verifica que el Dockerfile está en la ruta correcta
3. Asegúrate de que el código compila localmente

### Error: "Database connection failed"

1. Verifica que las bases de datos se crearon correctamente
2. Revisa que la variable `ConnectionStrings__DefaultConnection` está configurada
3. Espera a que las bases de datos estén completamente disponibles (pueden tardar 2-3 minutos)

### Error: CORS / "No 'Access-Control-Allow-Origin'"

1. Verifica que `CORS__AllowedOrigins` está configurado con `https://tpi-ui.onrender.com`
2. Asegúrate de que NO hay espacios extras en la URL
3. Reinicia los servicios después de cambiar variables de entorno

### SignalR no conecta

1. Verifica que `tpi-messages-service` está corriendo
2. Abre la consola del navegador (F12) y busca errores de WebSocket
3. Verifica que la URL del hub en `config.production.js` es correcta
4. SignalR tarda ~30 segundos en conectar si el servicio estaba dormido

### El servicio tarda mucho en responder

- **Primera request**: Normal, el servicio estaba dormido (15 minutos de inactividad)
- **Posteriores requests**: Deberían ser rápidas (<1 segundo)
- Si todo es lento, revisa los logs del servicio

## 📊 Monitoreo

### Ver Logs en Tiempo Real

```bash
# Opción 1: Dashboard web
1. Ve a Render Dashboard
2. Selecciona el servicio
3. Click en "Logs"

# Opción 2: Render CLI (opcional)
render logs tpi-auth-service --tail
```

### Verificar Estado de Salud

Cada servicio tiene un endpoint `/health`:

- `https://tpi-auth-service.onrender.com/health`
- `https://tpi-messages-service.onrender.com/health`
- `https://tpi-groups-service.onrender.com/health`

## 🔄 Actualizar la Aplicación

Para desplegar nuevos cambios:

```bash
# 1. Hacer cambios en el código
# 2. Commitear y pushear
git add .
git commit -m "Descripción de los cambios"
git push origin master

# Render detectará los cambios automáticamente y redesplegará
```

### Re-despliegue Manual

Si necesitas redesplegar sin cambios en el código:

1. Ve al servicio en Render Dashboard
2. Haz clic en **"Manual Deploy"** → **"Deploy latest commit"**

## 🎓 URLs Finales

Una vez completado el despliegue, comparte estas URLs:

- **Aplicación Web**: `https://tpi-ui.onrender.com`
- **API Auth**: `https://tpi-auth-service.onrender.com/swagger`
- **API Messages**: `https://tpi-messages-service.onrender.com/swagger`
- **API Groups**: `https://tpi-groups-service.onrender.com/swagger`

## 📝 Notas Importantes

1. ⏱️ **Primer acceso lento**: Los servicios tardan en despertar (~30-60s)
2. 🔒 **JWT Secret**: DEBE ser el mismo en todos los servicios
3. 🗄️ **Datos temporales**: Las bases de datos gratuitas expiran en 90 días
4. 🌐 **HTTPS**: Render provee HTTPS automáticamente
5. 📊 **Logs**: Revisa los logs si algo no funciona

## 🆘 Soporte

Si encuentras problemas:

1. Revisa los logs de cada servicio
2. Verifica que todas las variables de entorno están configuradas
3. Consulta la documentación de Render: [render.com/docs](https://render.com/docs)

---

**¡Listo!** Tu Sistema de Mensajería está desplegado en producción 🎉
