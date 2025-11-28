# 💬 Sistema de Mensajería en Tiempo Real - TPI Programación IV

Sistema de mensajería en tiempo real similar a WhatsApp/Telegram, implementado con arquitectura de microservicios en .NET 9.

## 🌐 Aplicación en Producción

**🚀 Accede a la app desplegada:**
```
https://tpi-messaging-frontend-2nf0aiotg-patricios-projects-3063c8f8.vercel.app
```

**📱 Compatible con:**
- ✅ Desktop (Windows, Mac, Linux)
- ✅ Mobile (iOS, Android)
- ✅ Tablets

---


## 🏗️ Arquitectura de Microservicios

```
┌─────────────────────────────────────────────────────────────┐
│                        FRONTEND (UI)                         │
│            HTML/JS con SignalR Client + REST API             │
└────────────┬────────────────────────────────┬───────────────┘
             │                                │
             │ HTTP/REST                      │ WebSocket/SignalR
             │                                │
┌────────────▼────────────────────────────────▼───────────────┐
│                    API GATEWAY (Opcional)                    │
└────────┬──────────────┬────────────────┬────────────────────┘
         │              │                │
    ┌────▼─────┐  ┌────▼─────┐    ┌────▼─────┐
    │   AUTH   │  │ MESSAGES │    │  GROUPS  │
    │ SERVICE  │  │ SERVICE  │    │ SERVICE  │
    │          │  │          │    │          │
    │ - JWT    │  │ - SignalR│    │ - CRUD   │
    │ - Login  │  │ - Hub    │    │ - Members│
    │ - Reg    │  │ - Chat   │    │          │
    └────┬─────┘  └────┬─────┘    └────┬─────┘
         │             │                │
    ┌────▼─────┐  ┌───▼──────┐    ┌───▼──────┐
    │PostgreSQL│  │PostgreSQL│    │PostgreSQL│
    │   Auth   │  │ Messages │    │  Groups  │
    └──────────┘  └──────────┘    └──────────┘
```

## 🔧 Stack Tecnológico

**Backend:**
- ✅ .NET 9 - Framework principal
- ✅ SignalR - WebSockets en tiempo real
- ✅ JWT - Autenticación con Access/Refresh Tokens
- ✅ Entity Framework Core - ORM
- ✅ PostgreSQL 18 - Base de datos relacional
- ✅ YARP - API Gateway/Reverse Proxy
- ✅ Swagger/OpenAPI - Documentación de APIs

**Frontend:**
- ✅ HTML5/CSS3/JavaScript (Vanilla)
- ✅ SignalR Client - Cliente WebSocket
- ✅ Vercel - Hosting del frontend

**DevOps:**
- ✅ ngrok - Túnel HTTPS para exposición local
- ✅ pgAdmin - Gestión de base de datos
- ✅ PowerShell - Scripts de automatización

## 📁 Estructura del Proyecto

```
TPI-.NET/
├── src/
│   ├── Services/
│   │   ├── Auth.Service/              # Microservicio de Autenticación
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── Services/
│   │   │   ├── Data/
│   │   │   └── Program.cs
│   │   │
│   │   ├── Messages.Service/          # Microservicio de Mensajería
│   │   │   ├── Controllers/
│   │   │   ├── Hubs/                 # SignalR Hubs
│   │   │   ├── Models/
│   │   │   ├── Services/
│   │   │   ├── Data/
│   │   │   └── Program.cs
│   │   │
│   │   └── Groups.Service/            # Microservicio de Grupos
│   │       ├── Controllers/
│   │       ├── Models/
│   │       ├── Services/
│   │       ├── Data/
│   │       └── Program.cs
│   │
│   ├── Shared/                        # Código compartido
│   │   ├── DTOs/
│   │   ├── Contracts/
│   │   └── Utilities/
│   │
│   └── UI/                            # Interfaz de Usuario
│       └── wwwroot/
│           ├── index.html
│           ├── app.js
│           └── styles.css
│
├── docs/
│   └── architecture.md
│
└── README.md
```

## 🚀 Características Principales

### Auth Service
- ✅ Registro de usuarios con validación
- ✅ Login con JWT (Access + Refresh Token)
- ✅ Validación y renovación de tokens
- ✅ Gestión de perfil de usuario

### Messages Service
- ✅ Envío de mensajes 1:1 y grupales
- ✅ Listado paginado de mensajes
- ✅ Indicadores en tiempo real ("escribiendo...")
- ✅ Acuses de lectura ("Visto") con timestamp
- ✅ SignalR Hub para comunicación bidireccional

### Groups Service
- ✅ Creación y eliminación de grupos
- ✅ Gestión de miembros (agregar/quitar)
- ✅ Listado de grupos del usuario
- ✅ Listado de miembros de un grupo

## 🔐 Seguridad

- **JWT Authentication**: Tokens de acceso y renovación
- **Autorización por recursos**: Un usuario solo accede a sus chats
- **Validación en SignalR**: Todos los Hubs validan tokens
- **HTTPS**: Comunicación segura
- **Password Hashing**: Contraseñas encriptadas con bcrypt

## 📦 Requisitos Previos

**Para ejecutar localmente:**
- .NET 9 SDK
- PostgreSQL 14+ (usamos PostgreSQL 18)
- ngrok (para exponer backend)
- Vercel CLI (para deploy del frontend)

**Credenciales de Base de Datos:**
- Host: `localhost`
- Puerto: `5432`
- Usuario: `postgres`
- Contraseña: `pato12`
- Bases de datos: `auth_db`, `messages_db`, `groups_db`

## 🏃 Ejecución Local

### Opción 1: Script Automático (Recomendado)

```powershell
# 1. Inicia todos los servicios automáticamente
.\start-local-backend.ps1

# 2. En otra terminal, exponer con ngrok
ngrok http 8000 --request-header-add='ngrok-skip-browser-warning: true'

# 3. La app ya está desplegada en Vercel
# Accede a: https://tpi-messaging-frontend-2nf0aiotg-patricios-projects-3063c8f8.vercel.app
```

Esto abrirá 4 ventanas de PowerShell con:
- Auth Service (puerto 5001)
- Messages Service (puerto 5002)  
- Groups Service (puerto 5003)
- API Gateway (puerto 8000) ← Usar este con ngrok

### Opción 2: Manual

### Opción 2: Manual

#### 1. Configurar Base de Datos

```bash
# Crear bases de datos en PostgreSQL
createdb auth_db
createdb messages_db
createdb groups_db
```

#### 2. Configurar Connection Strings

Actualizar `appsettings.json` en cada servicio con la cadena de conexión correspondiente.

#### 3. Ejecutar Migraciones

```bash
# En cada servicio
cd src/Services/Auth.Service
dotnet ef database update

cd ../Messages.Service
dotnet ef database update

cd ../Groups.Service
dotnet ef database update
```

#### 4. Ejecutar Servicios

#### 4. Ejecutar Servicios

```bash
# Terminal 1 - Auth Service (puerto 5001)
cd src/Services/Auth.Service
dotnet run

# Terminal 2 - Messages Service (puerto 5002)
cd src/Services/Messages.Service
dotnet run

# Terminal 3 - Groups Service (puerto 5003)
cd src/Services/Groups.Service
dotnet run
```

#### 5. Abrir UI

```bash
cd src/UI
dotnet run
# Abre http://localhost:8080
```

## 📚 Consultas SQL Útiles

### Ver usuarios registrados (en pgAdmin - auth_db):
```sql
SELECT id, username, display_name, email, created_at 
FROM "Users" 
ORDER BY username;
```

### Ver mensajes recientes (en messages_db):
```sql
SELECT m.id, m.content, m.sent_at, 
       sender.username as sender, 
       receiver.username as receiver
FROM "Messages" m
LEFT JOIN "ChatParticipants" sender ON m.sender_id = sender.user_id
LEFT JOIN "ChatParticipants" receiver ON m.receiver_id = receiver.user_id
ORDER BY m.sent_at DESC
LIMIT 50;
```

### Ver grupos y sus miembros (en groups_db):
```sql
SELECT g.id, g.name, g.description, 
       creator.username as created_by,
       COUNT(gm.user_id) as member_count
FROM "Groups" g
JOIN "Users" creator ON g.created_by_id = creator.id
LEFT JOIN "GroupMembers" gm ON g.id = gm.group_id
GROUP BY g.id, g.name, g.description, creator.username
ORDER BY g.created_at DESC;
```

---

## 📚 API Endpoints

### Auth Service (http://localhost:5001)
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/refresh` - Renovar token
- `GET /api/auth/profile` - Obtener perfil
- `PUT /api/auth/profile` - Actualizar perfil

### Messages Service (http://localhost:5002)
- `GET /api/messages/{chatId}` - Listar mensajes (paginado)
- `POST /api/messages/send` - Enviar mensaje
- `PUT /api/messages/{messageId}/read` - Marcar como leído
- `WS /hubs/chat` - SignalR Hub

### Groups Service (http://localhost:5003)
- `GET /api/groups` - Listar grupos del usuario
- `POST /api/groups` - Crear grupo
- `DELETE /api/groups/{groupId}` - Eliminar grupo
- `POST /api/groups/{groupId}/members` - Agregar miembro
- `DELETE /api/groups/{groupId}/members/{userId}` - Quitar miembro

## 📝 Eventos SignalR

### Cliente → Servidor
- `SendMessage` - Enviar mensaje
- `JoinChat` - Unirse a un chat
- `LeaveChat` - Salir de un chat
- `Typing` - Notificar que está escribiendo

### Servidor → Cliente
- `ReceiveMessage` - Recibir nuevo mensaje
- `UserTyping` - Alguien está escribiendo
- `MessageRead` - Mensaje leído
- `UserOnline` - Usuario conectado
- `UserOffline` - Usuario desconectado

## 👥 Autor

Patricio Alvarez (mApache)  
Trabajo Integrador - Programación IV  
TUP - UTN 2025

## 📄 Licencia

Proyecto académico - UTN

---

## 📚 Documentación Adicional

- **[DEPLOYMENT_OPTIONS.md](DEPLOYMENT_OPTIONS.md)** - Opciones de despliegue
- **[QUICK_START_HYBRID.md](QUICK_START_HYBRID.md)** - Despliegue híbrido (gratis)
- **[HYBRID_DEPLOYMENT.md](HYBRID_DEPLOYMENT.md)** - Detalles técnicos del despliegue híbrido
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** - Arquitectura detallada del sistema
- **[docs/API_REFERENCE.md](docs/API_REFERENCE.md)** - Referencia completa de APIs
- **[docs/EVALUACION.md](docs/EVALUACION.md)** - Cumplimiento de requisitos del TPI
