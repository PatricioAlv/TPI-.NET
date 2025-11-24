# APIs REST - Documentación Completa

## Auth Service (Puerto 5001)

### POST /api/auth/register
Registrar un nuevo usuario.

**Request Body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "displayName": "string (opcional)"
}
```

**Response 200:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64string",
  "user": {
    "id": 1,
    "username": "usuario1",
    "email": "user@example.com",
    "displayName": "Usuario Uno",
    "avatarUrl": null,
    "createdAt": "2025-11-24T10:00:00Z",
    "isOnline": true,
    "lastSeen": "2025-11-24T10:00:00Z"
  },
  "expiresAt": "2025-11-24T11:00:00Z"
}
```

**Errores:**
- `400`: Usuario o email ya existe
- `400`: Contraseña debe tener al menos 8 caracteres

---

### POST /api/auth/login
Iniciar sesión.

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response 200:** (igual que register)

**Errores:**
- `401`: Credenciales inválidas

---

### POST /api/auth/refresh
Renovar access token usando refresh token.

**Request Body:**
```json
{
  "refreshToken": "string"
}
```

**Response 200:** (igual que login)

**Errores:**
- `401`: Token inválido o expirado

---

### GET /api/auth/profile
Obtener perfil del usuario autenticado.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response 200:**
```json
{
  "id": 1,
  "username": "usuario1",
  "email": "user@example.com",
  "displayName": "Usuario Uno",
  "avatarUrl": null,
  "createdAt": "2025-11-24T10:00:00Z",
  "isOnline": true,
  "lastSeen": "2025-11-24T10:00:00Z"
}
```

---

### PUT /api/auth/profile
Actualizar perfil del usuario.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "displayName": "Nuevo Nombre",
  "avatarUrl": "https://example.com/avatar.jpg"
}
```

**Response 200:**
```json
{
  "message": "Perfil actualizado exitosamente"
}
```

---

### POST /api/auth/logout
Cerrar sesión (revoca refresh token).

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "refreshToken": "string"
}
```

**Response 200:**
```json
{
  "message": "Sesión cerrada exitosamente"
}
```

---

## Messages Service (Puerto 5002)

### GET /api/messages/chat/{otherUserId}
Obtener mensajes de un chat 1:1.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 50)

**Response 200:**
```json
{
  "messages": [
    {
      "id": 1,
      "senderId": 1,
      "senderUsername": "usuario1",
      "senderDisplayName": "Usuario Uno",
      "receiverId": 2,
      "groupId": null,
      "content": "Hola!",
      "sentAt": "2025-11-24T10:00:00Z",
      "isRead": true,
      "readAt": "2025-11-24T10:01:00Z",
      "type": 0
    }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 50,
  "hasNextPage": true
}
```

---

### GET /api/messages/group/{groupId}
Obtener mensajes de un grupo.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 50)

**Response 200:** (igual estructura que chat)

---

### POST /api/messages/send
Enviar mensaje (alternativa a SignalR).

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "receiverId": 2,      // Para chat 1:1
  "groupId": null,      // Para grupo (uno de los dos)
  "content": "Hola!",
  "type": 0             // 0=Text, 1=Image, 2=File, 3=Audio, 4=Video
}
```

**Response 200:**
```json
{
  "messageId": 123,
  "sentAt": "2025-11-24T10:00:00Z"
}
```

---

### PUT /api/messages/{messageId}/read
Marcar mensaje como leído (alternativa a SignalR).

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response 200:**
```json
{
  "messageId": 123,
  "readAt": "2025-11-24T10:01:00Z"
}
```

---

### POST /api/messages/sync-user
Sincronizar información de usuario (uso interno).

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "id": 1,
  "username": "usuario1",
  "displayName": "Usuario Uno",
  "avatarUrl": null,
  "isOnline": true,
  "lastSeen": "2025-11-24T10:00:00Z"
}
```

---

## Groups Service (Puerto 5003)

### GET /api/groups
Listar todos los grupos del usuario.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response 200:**
```json
[
  {
    "id": 1,
    "name": "Grupo de Estudio",
    "description": "Grupo para estudiar programación",
    "createdByUserId": 1,
    "createdAt": "2025-11-24T10:00:00Z",
    "avatarUrl": null,
    "membersCount": 5,
    "members": [
      {
        "userId": 1,
        "username": "usuario1",
        "displayName": "Usuario Uno",
        "avatarUrl": null,
        "role": 2,        // 0=Member, 1=Admin, 2=Owner
        "joinedAt": "2025-11-24T10:00:00Z",
        "isOnline": true
      }
    ]
  }
]
```

---

### GET /api/groups/{groupId}
Obtener detalles de un grupo específico.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response 200:** (igual estructura que elemento del array anterior)

**Errores:**
- `404`: Grupo no encontrado
- `403`: Usuario no es miembro del grupo

---

### POST /api/groups
Crear un nuevo grupo.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "name": "Nuevo Grupo",
  "description": "Descripción del grupo (opcional)",
  "memberIds": [2, 3, 4]  // IDs de usuarios a agregar
}
```

**Response 201:**
```json
{
  "groupId": 1
}
```

---

### PUT /api/groups/{groupId}
Actualizar información del grupo.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "name": "Nuevo Nombre (opcional)",
  "description": "Nueva descripción (opcional)",
  "avatarUrl": "https://... (opcional)"
}
```

**Response 200:**
```json
{
  "message": "Grupo actualizado exitosamente"
}
```

**Errores:**
- `403`: Solo admins/owners pueden actualizar

---

### DELETE /api/groups/{groupId}
Eliminar un grupo.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response 200:**
```json
{
  "message": "Grupo eliminado exitosamente"
}
```

**Errores:**
- `403`: Solo el creador puede eliminar el grupo

---

### POST /api/groups/{groupId}/members
Agregar miembro al grupo.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "userId": 5
}
```

**Response 200:**
```json
{
  "message": "Miembro agregado exitosamente"
}
```

**Errores:**
- `403`: Solo admins/owners pueden agregar miembros
- `400`: Usuario ya es miembro

---

### DELETE /api/groups/{groupId}/members/{userId}
Remover miembro del grupo.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response 200:**
```json
{
  "message": "Miembro removido exitosamente"
}
```

**Errores:**
- `403`: Solo admins/owners pueden remover (o el usuario mismo puede salir)
- `400`: Owner debe transferir propiedad antes de salir

---

### GET /api/groups/{groupId}/members
Listar miembros de un grupo.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response 200:**
```json
[
  {
    "userId": 1,
    "username": "usuario1",
    "displayName": "Usuario Uno",
    "avatarUrl": null,
    "role": 2,
    "joinedAt": "2025-11-24T10:00:00Z",
    "isOnline": true
  }
]
```

---

## SignalR Hub - Messages Service

### Endpoint
```
ws://localhost:5002/hubs/chat?access_token={JWT}
```

### Eventos del Cliente → Servidor

#### SendMessage
```javascript
connection.invoke('SendMessage', {
  receiverId: 2,      // o null para grupo
  groupId: null,      // o ID del grupo
  content: "Mensaje",
  type: 0            // MessageType
});
```

#### JoinChat
```javascript
connection.invoke('JoinChat', chatId, isGroup);
// chatId: ID del usuario o grupo
// isGroup: true/false
```

#### LeaveChat
```javascript
connection.invoke('LeaveChat', chatId, isGroup);
```

#### NotifyTyping
```javascript
connection.invoke('NotifyTyping', chatId, isGroup, isTyping);
// isTyping: true cuando empieza, false cuando termina
```

#### MarkMessageAsRead
```javascript
connection.invoke('MarkMessageAsRead', messageId);
```

---

### Eventos del Servidor → Cliente

#### ReceiveMessage
```javascript
connection.on('ReceiveMessage', (message) => {
  // message: MessageDto
});
```

#### MessageSent
```javascript
connection.on('MessageSent', (message) => {
  // Confirmación de envío exitoso
});
```

#### UserTyping
```javascript
connection.on('UserTyping', (notification) => {
  // notification: TypingNotificationDto
  // { chatId, userId, username, isTyping, isGroup }
});
```

#### MessageRead
```javascript
connection.on('MessageRead', (data) => {
  // data: MessageReadDto
  // { messageId, readByUserId, readAt }
});
```

#### UserOnline
```javascript
connection.on('UserOnline', (data) => {
  // { UserId, ConnectionId }
});
```

#### UserOffline
```javascript
connection.on('UserOffline', (data) => {
  // { UserId, LastSeen }
});
```

#### Error
```javascript
connection.on('Error', (errorMessage) => {
  console.error(errorMessage);
});
```

---

## Códigos de Estado HTTP

| Código | Significado |
|--------|-------------|
| 200 | OK - Solicitud exitosa |
| 201 | Created - Recurso creado exitosamente |
| 400 | Bad Request - Error en la solicitud |
| 401 | Unauthorized - Token inválido o ausente |
| 403 | Forbidden - Sin permisos para el recurso |
| 404 | Not Found - Recurso no encontrado |
| 500 | Internal Server Error - Error del servidor |

---

## Tipos de Mensajes (MessageType)

```csharp
public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    Audio = 3,
    Video = 4
}
```

## Roles de Grupo (GroupMemberRole)

```csharp
public enum GroupMemberRole
{
    Member = 0,  // Miembro regular
    Admin = 1,   // Administrador (puede agregar/quitar miembros)
    Owner = 2    // Propietario (todos los permisos + eliminar grupo)
}
```

---

## Ejemplos de Uso con cURL

### Registro
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test1",
    "email": "test1@test.com",
    "password": "password123",
    "displayName": "Test User 1"
  }'
```

### Login
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test1",
    "password": "password123"
  }'
```

### Obtener Perfil
```bash
curl -X GET http://localhost:5001/api/auth/profile \
  -H "Authorization: Bearer {accessToken}"
```

### Enviar Mensaje
```bash
curl -X POST http://localhost:5002/api/messages/send \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "receiverId": 2,
    "content": "Hola desde cURL!",
    "type": 0
  }'
```

### Crear Grupo
```bash
curl -X POST http://localhost:5003/api/groups \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "name": "Grupo de Prueba",
    "description": "Grupo creado desde cURL",
    "memberIds": [2, 3]
  }'
```
