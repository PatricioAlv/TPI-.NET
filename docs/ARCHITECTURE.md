# Arquitectura del Sistema de Mensajería

## Diagrama de Arquitectura de Microservicios

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│                           CAPA DE PRESENTACIÓN                          │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                  UI (HTML/CSS/JavaScript)                        │  │
│  │                                                                  │  │
│  │  - Login/Registro                    - SignalR Client           │  │
│  │  - Lista de Chats                    - REST API Calls           │  │
│  │  - Mensajería en tiempo real         - Token Management         │  │
│  │  - Gestión de Grupos                 - Estado Local             │  │
│  │                                                                  │  │
│  └────────┬─────────────────────────────────┬────────────────────────┘  │
│           │ HTTP/REST                       │ WebSocket                 │
└───────────┼─────────────────────────────────┼───────────────────────────┘
            │                                 │
            │                                 │
┌───────────▼─────────────────────────────────▼───────────────────────────┐
│                                                                         │
│                         CAPA DE SERVICIOS (APIs)                        │
│                                                                         │
│  ┌─────────────────┐  ┌──────────────────┐  ┌─────────────────────┐   │
│  │                 │  │                  │  │                     │   │
│  │  AUTH SERVICE   │  │ MESSAGES SERVICE │  │  GROUPS SERVICE     │   │
│  │   (Puerto 5001) │  │  (Puerto 5002)   │  │   (Puerto 5003)     │   │
│  │                 │  │                  │  │                     │   │
│  ├─────────────────┤  ├──────────────────┤  ├─────────────────────┤   │
│  │                 │  │                  │  │                     │   │
│  │ • Register      │  │ • SendMessage    │  │ • CreateGroup       │   │
│  │ • Login         │  │ • GetMessages    │  │ • GetGroups         │   │
│  │ • RefreshToken  │  │ • MarkAsRead     │  │ • AddMember         │   │
│  │ • GetProfile    │  │ • SyncUser       │  │ • RemoveMember      │   │
│  │ • UpdateProfile │  │                  │  │ • DeleteGroup       │   │
│  │                 │  │ ┌──────────────┐ │  │                     │   │
│  │                 │  │ │ SIGNALR HUB  │ │  │                     │   │
│  │                 │  │ │              │ │  │                     │   │
│  │                 │  │ │ • SendMsg    │ │  │                     │   │
│  │                 │  │ │ • JoinChat   │ │  │                     │   │
│  │                 │  │ │ • LeaveChat  │ │  │                     │   │
│  │                 │  │ │ • Typing     │ │  │                     │   │
│  │                 │  │ │ • MarkRead   │ │  │                     │   │
│  │                 │  │ └──────────────┘ │  │                     │   │
│  │                 │  │                  │  │                     │   │
│  └────────┬────────┘  └────────┬─────────┘  └──────────┬──────────┘   │
│           │                    │                       │              │
│           │                    │                       │              │
└───────────┼────────────────────┼───────────────────────┼──────────────┘
            │                    │                       │
            │                    │                       │
┌───────────▼────────────────────▼───────────────────────▼──────────────┐
│                                                                        │
│                      CAPA DE PERSISTENCIA                              │
│                                                                        │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐    │
│  │                  │  │                  │  │                  │    │
│  │   PostgreSQL     │  │   PostgreSQL     │  │   PostgreSQL     │    │
│  │    auth_db       │  │   messages_db    │  │    groups_db     │    │
│  │                  │  │                  │  │                  │    │
│  ├──────────────────┤  ├──────────────────┤  ├──────────────────┤    │
│  │                  │  │                  │  │                  │    │
│  │ • Users          │  │ • Messages       │  │ • Groups         │    │
│  │ • RefreshTokens  │  │ • ChatParticip.  │  │ • GroupMembers   │    │
│  │                  │  │                  │  │                  │    │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘    │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

## Diagrama de Flujo de Autenticación

```
┌─────────┐              ┌──────────────┐              ┌──────────┐
│         │              │              │              │          │
│   UI    │              │ Auth Service │              │ Database │
│         │              │              │              │          │
└────┬────┘              └──────┬───────┘              └─────┬────┘
     │                          │                            │
     │  1. POST /register       │                            │
     ├─────────────────────────>│                            │
     │  (username, email, pwd)  │                            │
     │                          │  2. Hash Password          │
     │                          ├────────────┐               │
     │                          │            │               │
     │                          │<───────────┘               │
     │                          │                            │
     │                          │  3. INSERT User            │
     │                          ├───────────────────────────>│
     │                          │                            │
     │                          │  4. User Created           │
     │                          │<───────────────────────────┤
     │                          │                            │
     │                          │  5. Generate JWT           │
     │                          ├────────────┐               │
     │                          │            │               │
     │                          │<───────────┘               │
     │                          │                            │
     │                          │  6. Store Refresh Token    │
     │                          ├───────────────────────────>│
     │                          │                            │
     │  7. Return Tokens        │                            │
     │<─────────────────────────┤                            │
     │  (AccessToken, Refresh)  │                            │
     │                          │                            │
     │  8. Store in LocalStorage│                            │
     ├──────────────┐           │                            │
     │              │           │                            │
     │<─────────────┘           │                            │
     │                          │                            │
```

## Diagrama de Flujo de Mensajería en Tiempo Real

```
┌──────────┐         ┌─────────────────┐         ┌──────────┐
│          │         │                 │         │          │
│  User 1  │         │ Messages Service│         │  User 2  │
│   (UI)   │         │   (SignalR)     │         │   (UI)   │
│          │         │                 │         │          │
└────┬─────┘         └────────┬────────┘         └────┬─────┘
     │                        │                       │
     │  1. Connect (JWT)      │                       │
     ├───────────────────────>│                       │
     │                        │                       │
     │  2. Connected          │  3. Connect (JWT)     │
     │<───────────────────────┤<──────────────────────┤
     │                        │                       │
     │  4. JoinChat(userId2)  │  5. JoinChat(userId1) │
     ├───────────────────────>│<──────────────────────┤
     │                        │                       │
     │  6. Typing...          │                       │
     ├───────────────────────>│                       │
     │                        │  7. UserTyping Event  │
     │                        ├──────────────────────>│
     │                        │                       │
     │  8. SendMessage("Hi")  │                       │
     ├───────────────────────>│                       │
     │                        │  9. Save to DB        │
     │                        ├────────────┐          │
     │                        │            │          │
     │                        │<───────────┘          │
     │                        │                       │
     │  10. MessageSent       │  11. ReceiveMessage   │
     │<───────────────────────┼──────────────────────>│
     │                        │                       │
     │                        │  12. MarkAsRead(msgId)│
     │                        │<──────────────────────┤
     │                        │  13. Update DB        │
     │                        ├────────────┐          │
     │                        │            │          │
     │                        │<───────────┘          │
     │  14. MessageRead Event │                       │
     │<───────────────────────┤                       │
     │   (Show blue check)    │                       │
     │                        │                       │
```

## Patrones de Diseño Implementados

### 1. **Microservicios**
- Servicios independientes y desacoplados
- Cada servicio con su propia base de datos
- Comunicación vía HTTP/REST y WebSockets

### 2. **Repository Pattern**
- Entity Framework Core como ORM
- DbContext como repositorio
- Abstracción de la capa de datos

### 3. **DTO Pattern**
- Objetos de transferencia de datos (DTOs)
- Separación entre modelos de dominio y API
- Proyecto `Shared` para DTOs comunes

### 4. **Dependency Injection**
- Inyección nativa de .NET
- Services registrados en `Program.cs`
- Scoped lifetime para DbContext

### 5. **Authentication & Authorization**
- JWT Bearer Tokens
- Claims-based authorization
- Refresh token pattern
- Token validation middleware

### 6. **Real-time Communication**
- SignalR para WebSockets
- Hub pattern para broadcast
- Connection management

## Seguridad Implementada

### JWT Authentication
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Token Validation
- Signature verification
- Expiration check
- Issuer/Audience validation
- Claims extraction

### CORS Configuration
```json
{
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173",
    "http://127.0.0.1:5500"
  ]
}
```

### Password Hashing
- BCrypt algorithm
- Salt rounds: 11
- One-way encryption

## Escalabilidad

### Horizontal Scaling
- Múltiples instancias de cada servicio
- Load balancer (Nginx, Azure App Gateway)
- Shared JWT secret key

### Database Scaling
- PostgreSQL replication
- Read replicas
- Connection pooling

### SignalR Scaling
- Azure SignalR Service
- Redis backplane
- Sticky sessions

## Próximas Mejoras

1. **API Gateway** (Ocelot, YARP)
2. **Service Discovery** (Consul, Eureka)
3. **Event Bus** (RabbitMQ, Azure Service Bus)
4. **Distributed Caching** (Redis)
5. **Monitoring** (Prometheus, Grafana)
6. **Logging** (Serilog, ELK Stack)
7. **Containerization** (Docker, Kubernetes)
8. **CI/CD** (GitHub Actions, Azure DevOps)
