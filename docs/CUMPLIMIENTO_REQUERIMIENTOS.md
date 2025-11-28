# Cumplimiento de Requerimientos Funcionales

Este documento detalla dónde se cumple cada requerimiento funcional del proyecto.

---

## REQ-01: Gestión de Usuarios y Autenticación

### ✅ Registro de usuarios con validación de datos

**Ubicación:**
- **Controlador**: `src/Services/Auth.Service/Controllers/AuthController.cs` (líneas 22-38)
  - Endpoint: `POST /api/auth/register`
  - Valida que el email sea único
  - Valida que la contraseña tenga al menos 8 caracteres

- **Servicio**: `src/Services/Auth.Service/Services/AuthService.cs` (líneas 32-53)
  - Verifica que usuario y email no existan previamente
  - Hashea la contraseña usando BCrypt

- **Modelo**: `src/Services/Auth.Service/Models/User.cs` (líneas 1-66)
  - Validaciones de modelo: `[Required]`, `[EmailAddress]`, `[MaxLength]`

### ✅ Inicio de sesión con tokens

**Ubicación:**
- **Controlador**: `src/Services/Auth.Service/Controllers/AuthController.cs` (líneas 40-52)
  - Endpoint: `POST /api/auth/login`
  - Retorna access token y refresh token

- **Servicio**: `src/Services/Auth.Service/Services/AuthService.cs` (líneas 55-72)
  - Verifica credenciales con BCrypt
  - Actualiza estado de conexión (IsOnline, LastSeen)

- **Token Service**: `src/Services/Auth.Service/Services/TokenService.cs` (líneas 26-48)
  - Genera access token JWT con claims (userId, username, email)
  - Genera refresh token seguro (líneas 50-56)

### ✅ Gestión de perfil de usuario

**Ubicación:**
- **Consultar perfil**: `src/Services/Auth.Service/Controllers/AuthController.cs` (líneas 68-80)
  - Endpoint: `GET /api/auth/profile`
  - Requiere autenticación JWT

- **Actualizar perfil**: `src/Services/Auth.Service/Controllers/AuthController.cs` (líneas 82-95)
  - Endpoint: `PUT /api/auth/profile`
  - Permite actualizar nombre, avatar, etc.

- **Servicio**: `src/Services/Auth.Service/Services/AuthService.cs`
  - `GetUserProfileAsync` (línea 95+)
  - `UpdateUserProfileAsync` (línea 103+)

### ✅ Hash de contraseñas seguro

**Ubicación:**
- Uso de BCrypt en `src/Services/Auth.Service/Services/AuthService.cs`
  - Hasheo: línea 46 (`BCrypt.Net.BCrypt.HashPassword`)
  - Verificación: línea 62 (`BCrypt.Net.BCrypt.Verify`)

---

## REQ-02: API Usuarios/Auth Service

### ✅ Módulo de servicio completo

**Ubicación:**
- **Registro**: `POST /api/auth/register`
- **Login**: `POST /api/auth/login`
- **Refresh Token**: `POST /api/auth/refresh` (líneas 54-66)
- **Perfil**: `GET /api/auth/profile`
- **Actualizar Perfil**: `PUT /api/auth/profile`
- **Logout**: `POST /api/auth/logout` (línea 97+)
- **Listar Usuarios**: `GET /api/auth/users` (implementado en AuthService.cs línea 151+)

**Validación y emisión de tokens JWT:**
- `src/Services/Auth.Service/Services/TokenService.cs` (todo el archivo)
- Configuración JWT: `src/Services/Auth.Service/Program.cs` (líneas 25-39)

---

## REQ-03: API Mensajes Service

### ✅ Envío y listado de mensajes

**Ubicación:**
- **Enviar mensaje**: `src/Services/Messages.Service/Controllers/MessagesController.cs`
  - Endpoint REST: `POST /api/messages/send` (línea 137+)
  - SignalR Hub: `src/Services/Messages.Service/Hubs/ChatHub.cs` método `SendMessage` (líneas 52-116)

- **Listado paginado - Mensajes directos**: `src/Services/Messages.Service/Controllers/MessagesController.cs` (líneas 24-73)
  - Endpoint: `GET /api/messages/chat/{otherUserId}?pageNumber=1&pageSize=50`
  - Soporte de paginación completo

- **Listado paginado - Mensajes grupales**: (líneas 75-133)
  - Endpoint: `GET /api/messages/group/{groupId}?pageNumber=1&pageSize=50`
  - Validación de membresía del grupo

### ✅ Manejo de eventos "escribiendo..." con SignalR

**Ubicación:**
- **Hub SignalR**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Método `NotifyTyping` (líneas 141-160)
  - Recibe notificación del cliente y la retransmite a otros usuarios

- **Cliente UI**: `src/UI/wwwroot/app.js`
  - Función `onTyping()` (línea 655+)
  - Envía evento cada 1 segundo mientras escribe
  - Listener `UserTyping` (línea 490+)

### ✅ Registro de acuses de lectura

**Ubicación:**
- **Modelo**: `src/Services/Messages.Service/Models/Message.cs`
  - Clase `MessageRead` (líneas 63-76)
  - Registra messageId, userId y timestamp de lectura

- **Hub SignalR**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Método `MarkMessageAsRead` (líneas 162-200+)
  - Valida autorización
  - Persiste en base de datos
  - Notifica en tiempo real

- **Controller REST**: `src/Services/Messages.Service/Controllers/MessagesController.cs`
  - Endpoint alternativo: `POST /api/messages/mark-read` (línea 216+)

### ✅ Persistencia con timestamps

**Ubicación:**
- **Modelo Message**: `src/Services/Messages.Service/Models/Message.cs` (líneas 7-33)
  - Campo `SentAt` (DateTime)
  - Campo `ReadAt` (DateTime nullable)
  - Campo `IsRead` (bool)

---

## REQ-04: API Grupos Service

### ✅ Creación y eliminación de grupos

**Ubicación:**
- **Crear grupo**: `src/Services/Groups.Service/Controllers/GroupsController.cs`
  - Endpoint: `POST /api/groups` (líneas 100-186)
  - Asigna automáticamente rol de Owner al creador

- **Eliminar grupo**: (líneas 224-245)
  - Endpoint: `DELETE /api/groups/{groupId}`
  - Solo el creador puede eliminar
  - Soft delete (marca `IsDeleted = true`)

### ✅ Gestión de miembros

**Ubicación:**
- **Agregar miembro**: `src/Services/Groups.Service/Controllers/GroupsController.cs` (líneas 247-284)
  - Endpoint: `POST /api/groups/{groupId}/members`
  - Solo admins/owners pueden agregar

- **Quitar miembro**: (líneas 286-313)
  - Endpoint: `DELETE /api/groups/{groupId}/members/{memberId}`
  - Admins/owners pueden quitar a otros
  - Cualquier usuario puede salirse del grupo

### ✅ Listado de grupos y miembros

**Ubicación:**
- **Listar grupos del usuario**: `src/Services/Groups.Service/Controllers/GroupsController.cs` (líneas 24-55)
  - Endpoint: `GET /api/groups`
  - Retorna solo grupos donde el usuario es miembro activo

- **Obtener grupo específico**: (líneas 57-98)
  - Endpoint: `GET /api/groups/{groupId}`
  - Incluye lista completa de miembros

- **Listar miembros**: (líneas 315-342)
  - Endpoint: `GET /api/groups/{groupId}/members`

---

## REQ-05: Funcionalidades de Mensajería - Chats 1:1

### ✅ Iniciar conversación

**Ubicación:**
- **UI**: `src/UI/wwwroot/app.js`
  - Función `selectUser()` (línea 305+)
  - Modal de nueva conversación: función `showNewChatModal()` (línea 713+)

### ✅ Enviar y recibir en tiempo real

**Ubicación:**
- **Envío SignalR**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Método `SendMessage` (líneas 52-116)
  - Envía al receptor específico si está conectado

- **Recepción UI**: `src/UI/wwwroot/app.js`
  - Listener `ReceiveMessage` (línea 477)
  - Función `handleNewMessage()` (línea 507+)

### ✅ Visualizar historial paginado

**Ubicación:**
- **Backend**: `src/Services/Messages.Service/Controllers/MessagesController.cs`
  - Endpoint `GET /api/messages/chat/{otherUserId}` (líneas 24-73)
  - Retorna objeto con `Messages`, `TotalCount`, `PageNumber`, `PageSize`, `HasNextPage`

- **UI**: `src/UI/wwwroot/app.js`
  - Función `loadConversation()` (línea 338+)
  - Función `renderMessages()` (línea 368+)

---

## REQ-06: Chats Grupales

### ✅ Crear grupos con participantes

**Ubicación:**
- **UI**: `src/UI/wwwroot/app.js`
  - Función `createGroup()` (línea 872+)
  - Modal con selección de miembros

- **Backend**: Ya documentado en REQ-04

### ✅ Mensajes a todos los miembros

**Ubicación:**
- **Hub SignalR**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Líneas 111-115: `Clients.Group($"Group_{messageDto.GroupId}").SendAsync("ReceiveMessage", messageResponse)`
  - Usa grupos de SignalR para broadcast

### ✅ Agregar/remover miembros

**Ubicación:**
- Ya documentado en REQ-04
- **UI**: `src/UI/wwwroot/app.js`
  - Funciones integradas en la gestión de grupos

### ✅ Visualizar lista de participantes

**Ubicación:**
- **Backend**: Incluido en el DTO `GroupDto` con lista de `Members`
- **UI**: Los miembros se muestran en la información del grupo

---

## REQ-07: Indicadores en Tiempo Real - "Está escribiendo..."

### ✅ Detectar cuando usuario está tecleando

**Ubicación:**
- **UI**: `src/UI/wwwroot/app.js`
  - Función `onTyping()` (líneas 655-671)
  - Detecta input en el campo de mensaje
  - Usa debounce de 1 segundo

### ✅ Transmitir evento vía SignalR

**Ubicación:**
- **Cliente**: `src/UI/wwwroot/app.js`
  - Invoca `connection.invoke('NotifyTyping', chatId, isGroup, true)`

- **Hub**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Método `NotifyTyping` (líneas 141-160)
  - Envía a otros participantes: `Clients.OthersInGroup(groupName).SendAsync("UserTyping", notification)`

### ✅ Ocultar indicador después de 3 segundos

**Ubicación:**
- **UI**: `src/UI/wwwroot/app.js`
  - Función `showTypingIndicator()` (línea 673+)
  - Usa `setTimeout` con 3000ms
  - Limpia automáticamente el indicador

### ✅ Visualización en UI

**Ubicación:**
- **HTML**: `src/UI/wwwroot/index.html` (línea 103)
  - `<div id="typing-indicator" class="typing-indicator">`

---

## REQ-08: Acuses de Lectura / "Visto"

### ✅ Registrar cuándo cada usuario lee cada mensaje

**Ubicación:**
- **Modelo**: `src/Services/Messages.Service/Models/Message.cs`
  - Clase `MessageRead` con `ReadAt` (timestamp)

- **Hub SignalR**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Método `MarkMessageAsRead` (líneas 162-200+)

### ✅ Persistir en base de datos

**Ubicación:**
- **DbContext**: `src/Services/Messages.Service/Data/MessagesDbContext.cs`
  - DbSet `MessageReads`

- **Migración**: Incluido en las migraciones de Messages.Service

### ✅ Sincronizar estado en tiempo real

**Ubicación:**
- **Hub**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Línea 201+: Notifica `MessageRead` a otros clientes
  - Broadcast a grupo o usuario específico

### ✅ Mostrar indicadores visuales (doble check)

**Ubicación:**
- **UI**: `src/UI/wwwroot/app.js`
  - Función `renderMessages()` (líneas 368-434)
  - Líneas 410-412: Muestra ✓ (enviado) o ✓✓ (leído)
  - CSS con clase `.read` para check azul

---

## REQ-09: Listado de usuarios con lectura y fecha/hora

### ✅ Visualizar historial de lectura de un mensaje

**Ubicación:**
- **Backend**: `src/Services/Messages.Service/Controllers/MessagesController.cs`
  - Endpoint: `GET /api/messages/{messageId}/reads` (líneas 258-298)
  - Retorna lista de usuarios con:
    - userId
    - username
    - displayName
    - readAt (timestamp)

- **Modelo**: Usa tabla `MessageReads` para tracking individual

---

## REQ-10: Autenticación y Seguridad

### ✅ Endpoints requieren JWT (excepto register/login)

**Ubicación:**
- **Auth Service**: `src/Services/Auth.Service/Controllers/AuthController.cs`
  - Atributo `[Authorize]` en endpoints protegidos (líneas 68, 82, 97, 115)

- **Messages Service**: `src/Services/Messages.Service/Controllers/MessagesController.cs`
  - Atributo `[Authorize]` en toda la clase (línea 13)

- **Groups Service**: `src/Services/Groups.Service/Controllers/GroupsController.cs`
  - Atributo `[Authorize]` en toda la clase (línea 13)

### ✅ SignalR valida tokens

**Ubicación:**
- **Hub**: `src/Services/Messages.Service/Hubs/ChatHub.cs`
  - Atributo `[Authorize]` en la clase (línea 11)
  - Validación automática de JWT en conexión

- **Cliente**: `src/UI/wwwroot/app.js` (línea 459)
  - `accessTokenFactory: () => appState.accessToken`

- **Configuración**: `src/Services/Messages.Service/Program.cs`
  - Configuración JWT en SignalR (líneas con `MapHub`)

### ✅ Autorización por recursos

**Ubicación:**
- **Messages Controller**: Valida que usuario participa del chat
  - Línea 237-241: Validación para mensajes 1:1
  - Línea 91: Validación de membresía grupal

- **Groups Controller**: Verifica membresía antes de permitir acceso
  - Líneas 70-72: Valida que usuario es miembro
  - Líneas 256-259: Solo admins pueden agregar miembros

### ✅ Hash de contraseñas con BCrypt

**Ubicación:**
- Ya documentado en REQ-01
- Algoritmo seguro con salt automático

### ✅ Configuración CORS

**Ubicación:**
- **Auth Service**: `src/Services/Auth.Service/Program.cs` (líneas 44-54)
- **Messages Service**: `src/Services/Messages.Service/Program.cs` (similar)
- **Groups Service**: `src/Services/Groups.Service/Program.cs` (similar)
- Lee orígenes permitidos desde configuración (`appsettings.json`)

---

## REQ-11: Interfaz de Usuario (UI)

### ✅ UI funcional implementada

**Ubicación principal:**
- `src/UI/wwwroot/index.html` (166 líneas)
- `src/UI/wwwroot/app.js` (1041 líneas)
- `src/UI/wwwroot/styles.css`

### ✅ Login/registro

**Ubicación:**
- **HTML**: `src/UI/wwwroot/index.html` (líneas 10-40)
  - Formulario de login (líneas 20-27)
  - Formulario de registro (líneas 29-37)

- **JS**: `src/UI/wwwroot/app.js`
  - Función `login()` (línea 87+)
  - Función `register()` (línea 80+)

### ✅ Listar chats

**Ubicación:**
- **HTML**: `src/UI/wwwroot/index.html` (líneas 64-68)
  - `<ul id="user-list">`

- **JS**: `src/UI/wwwroot/app.js`
  - Función `loadUsers()` (línea 175+)
  - Función `renderUserList()` (línea 207+)

### ✅ Abrir chat y ver mensajes

**Ubicación:**
- **HTML**: `src/UI/wwwroot/index.html` (líneas 88-127)
  - Área de mensajes: `<div id="messages-container">`

- **JS**: `src/UI/wwwroot/app.js`
  - Función `selectUser()` (línea 305+)
  - Función `loadConversation()` (línea 338+)
  - Función `renderMessages()` (línea 368+)

### ✅ Enviar mensajes

**Ubicación:**
- **HTML**: `src/UI/wwwroot/index.html` (líneas 119-127)
  - Input de mensaje y botón enviar

- **JS**: `src/UI/wwwroot/app.js`
  - Función `sendMessage()` (línea 600+)
  - Usa SignalR para envío en tiempo real

### ✅ Ver indicadores "escribiendo" y "visto"

**Ubicación:**
- **Indicador "escribiendo"**:
  - HTML: línea 103 (`<div id="typing-indicator">`)
  - JS: Función `showTypingIndicator()` (línea 673+)

- **Indicador "visto"**:
  - JS: En `renderMessages()` (líneas 410-412)
  - Muestra ✓ o ✓✓ según estado de lectura

### ✅ Crear grupos y agregar miembros

**Ubicación:**
- **HTML**: `src/UI/wwwroot/index.html` (líneas 146-157)
  - Modal de creación de grupo

- **JS**: `src/UI/wwwroot/app.js`
  - Función `showNewGroupModal()` (línea 754+)
  - Función `createGroup()` (línea 872+)
  - Lista de usuarios con checkboxes para seleccionar miembros

---

## Resumen de Cumplimiento

| Requerimiento | Estado | Archivos Principales |
|---------------|--------|----------------------|
| REQ-01 | ✅ Completo | Auth.Service/Controllers/AuthController.cs, AuthService.cs, User.cs |
| REQ-02 | ✅ Completo | Todo Auth.Service/ |
| REQ-03 | ✅ Completo | Messages.Service/Controllers/MessagesController.cs, ChatHub.cs |
| REQ-04 | ✅ Completo | Groups.Service/Controllers/GroupsController.cs |
| REQ-05 | ✅ Completo | MessagesController.cs, ChatHub.cs, app.js |
| REQ-06 | ✅ Completo | GroupsController.cs, ChatHub.cs, app.js |
| REQ-07 | ✅ Completo | ChatHub.cs (NotifyTyping), app.js (onTyping) |
| REQ-08 | ✅ Completo | ChatHub.cs (MarkMessageAsRead), MessageRead model |
| REQ-09 | ✅ Completo | MessagesController.cs (GetMessageReads endpoint) |
| REQ-10 | ✅ Completo | [Authorize] en controllers, JWT config, BCrypt, CORS |
| REQ-11 | ✅ Completo | UI/wwwroot/ (index.html, app.js, styles.css) |

**Todos los requerimientos están implementados y funcionales.**

---

## Tecnologías Utilizadas

- **Backend**: ASP.NET Core 8.0
- **Base de datos**: PostgreSQL
- **ORM**: Entity Framework Core
- **Autenticación**: JWT (JSON Web Tokens)
- **Hash de contraseñas**: BCrypt.Net-Next
- **Tiempo real**: SignalR
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **Cliente SignalR**: Microsoft SignalR JavaScript Client

## Arquitectura

El proyecto sigue una arquitectura de microservicios con:
- **Auth.Service**: Puerto 5001 - Gestión de usuarios y autenticación
- **Messages.Service**: Puerto 5002 - Mensajes y comunicación en tiempo real
- **Groups.Service**: Puerto 5003 - Gestión de grupos
- **UI**: Interfaz web estática

Cada servicio tiene su propia base de datos PostgreSQL independiente.
