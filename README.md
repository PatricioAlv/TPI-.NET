# Sistema de Mensajería en Tiempo Real - TPI Programación IV

## 📋 Descripción
Sistema de mensajería en tiempo real similar a WhatsApp/Telegram, implementado con arquitectura de microservicios en .NET 9.

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

## 🔧 Tecnologías Utilizadas

- **.NET 9**: Framework principal
- **SignalR**: Comunicación bidireccional en tiempo real
- **JWT**: Autenticación con Access y Refresh Tokens
- **Entity Framework Core**: ORM para persistencia
- **PostgreSQL**: Base de datos relacional
- **Swagger/OpenAPI**: Documentación de APIs

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

- .NET 9 SDK
- PostgreSQL 14+
- Node.js (para el cliente SignalR en UI)

## 🏃 Ejecución

### 1. Configurar Base de Datos

```bash
# Crear bases de datos en PostgreSQL
createdb auth_db
createdb messages_db
createdb groups_db
```

### 2. Configurar Connection Strings

Actualizar `appsettings.json` en cada servicio con la cadena de conexión correspondiente.

### 3. Ejecutar Migraciones

```bash
# En cada servicio
cd src/Services/Auth.Service
dotnet ef database update

cd ../Messages.Service
dotnet ef database update

cd ../Groups.Service
dotnet ef database update
```

### 4. Ejecutar Servicios

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

### 5. Abrir UI

Abrir `src/UI/wwwroot/index.html` en el navegador o servir con un servidor HTTP local.

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
mApache
Trabajo Integrador - Programación IV
TUP - UTN 2025

## 📄 Licencia

Proyecto académico de mApache - UTN
