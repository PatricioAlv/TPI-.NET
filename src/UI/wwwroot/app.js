// Configuración de URLs de los servicios
const API_URLS = {
    auth: 'http://localhost:5001/api/auth',
    messages: 'http://localhost:5002/api/messages',
    groups: 'http://localhost:5003/api/groups',
    chatHub: 'http://localhost:5002/hubs/chat'
};

// Estado global de la aplicación
let appState = {
    user: null,
    accessToken: null,
    refreshToken: null,
    currentChat: null,
    connection: null,
    users: [],
    conversations: new Map(), // Mapa de conversaciones por userId
    groups: [],
    typingTimeout: null
};

// ===== INICIALIZACIÓN =====

document.addEventListener('DOMContentLoaded', () => {
    // Cargar estado guardado
    loadSavedState();
    
    // Si hay sesión guardada, ir directamente al chat
    if (appState.accessToken && appState.user) {
        showChatScreen();
        loadUsers();
        connectToSignalR();
    }
});

function loadSavedState() {
    const saved = localStorage.getItem('chatAppState');
    if (saved) {
        try {
            const state = JSON.parse(saved);
            appState.user = state.user;
            appState.accessToken = state.accessToken;
            appState.refreshToken = state.refreshToken;
            
            // Cargar conversaciones guardadas
            const savedConversations = localStorage.getItem('conversations');
            if (savedConversations) {
                const convData = JSON.parse(savedConversations);
                appState.conversations = new Map(convData);
            }
        } catch (error) {
            console.error('Error cargando estado:', error);
            localStorage.removeItem('chatAppState');
        }
    }
}

function saveState() {
    const state = {
        user: appState.user,
        accessToken: appState.accessToken,
        refreshToken: appState.refreshToken
    };
    localStorage.setItem('chatAppState', JSON.stringify(state));
    
    // Guardar conversaciones
    const convArray = Array.from(appState.conversations.entries());
    localStorage.setItem('conversations', JSON.stringify(convArray));
}

// ===== AUTENTICACIÓN =====

function showTab(tabName) {
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.auth-form').forEach(form => form.classList.remove('active'));
    
    event.target.classList.add('active');
    document.getElementById(`${tabName}-form`).classList.add('active');
}

async function register() {
    const username = document.getElementById('register-username').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;
    const displayName = document.getElementById('register-displayname').value;
    
    const errorEl = document.getElementById('register-error');
    errorEl.textContent = '';

    if (!username || !email || !password) {
        errorEl.textContent = 'Por favor completa todos los campos requeridos';
        return;
    }

    if (password.length < 8) {
        errorEl.textContent = 'La contraseña debe tener al menos 8 caracteres';
        return;
    }

    try {
        const response = await fetch(`${API_URLS.auth}/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password, displayName })
        });

        if (response.ok) {
            const data = await response.json();
            handleAuthSuccess(data);
        } else {
            const error = await response.json();
            errorEl.textContent = error.message || 'Error al registrarse';
        }
    } catch (error) {
        errorEl.textContent = 'Error de conexión con el servidor';
        console.error('Error:', error);
    }
}

async function login() {
    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;
    
    const errorEl = document.getElementById('login-error');
    errorEl.textContent = '';

    if (!username || !password) {
        errorEl.textContent = 'Por favor completa todos los campos';
        return;
    }

    try {
        const response = await fetch(`${API_URLS.auth}/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });

        if (response.ok) {
            const data = await response.json();
            handleAuthSuccess(data);
        } else {
            const error = await response.json();
            errorEl.textContent = error.message || 'Credenciales inválidas';
        }
    } catch (error) {
        errorEl.textContent = 'Error de conexión con el servidor';
        console.error('Error:', error);
    }
}

function handleAuthSuccess(data) {
    appState.user = data.user;
    appState.accessToken = data.accessToken;
    appState.refreshToken = data.refreshToken;
    
    saveState();
    showChatScreen();
    loadUsers();
    syncCurrentUser();
    connectToSignalR();
}

async function syncCurrentUser() {
    try {
        await fetch(`${API_URLS.messages}/sync-user`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${appState.accessToken}`
            },
            body: JSON.stringify(appState.user)
        });
    } catch (error) {
        console.error('Error sincronizando usuario:', error);
    }
}

function showChatScreen() {
    document.getElementById('auth-screen').classList.remove('active');
    document.getElementById('chat-screen').classList.add('active');
    document.getElementById('current-user-name').textContent = appState.user.displayName || appState.user.username;
}

async function logout() {
    if (appState.connection) {
        await appState.connection.stop();
    }
    
    try {
        await fetch(`${API_URLS.auth}/logout`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${appState.accessToken}`
            },
            body: JSON.stringify({ refreshToken: appState.refreshToken })
        });
    } catch (error) {
        console.error('Error al cerrar sesión:', error);
    }
    
    // Limpiar estado
    appState = {
        user: null,
        accessToken: null,
        refreshToken: null,
        currentChat: null,
        connection: null,
        users: [],
        conversations: new Map(),
        groups: [],
        typingTimeout: null
    };
    
    localStorage.removeItem('chatAppState');
    
    document.getElementById('chat-screen').classList.remove('active');
    document.getElementById('auth-screen').classList.add('active');
}

// ===== USUARIOS =====

async function loadUsers() {
    try {
        const response = await fetch(`${API_URLS.auth}/users`, {
            headers: { 'Authorization': `Bearer ${appState.accessToken}` }
        });
        
        if (response.ok) {
            appState.users = await response.json();
            
            // Sincronizar usuarios con el servicio de mensajes
            await syncUsers(appState.users);
            
            renderUserList();
        }
    } catch (error) {
        console.error('Error cargando usuarios:', error);
    }
}

async function syncUsers(users) {
    try {
        for (const user of users) {
            await fetch(`${API_URLS.messages}/sync-user`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${appState.accessToken}`
                },
                body: JSON.stringify(user)
            });
        }
    } catch (error) {
        console.error('Error sincronizando usuarios:', error);
    }
}

function renderUserList() {
    const userList = document.getElementById('user-list');
    userList.innerHTML = '';
    
    appState.users.forEach(user => {
        const li = document.createElement('li');
        li.className = 'user-item';
        if (appState.currentChat?.id === user.id) {
            li.classList.add('active');
        }
        
        const unreadCount = getUnreadCount(user.id);
        const lastMessage = getLastMessage(user.id);
        
        li.innerHTML = `
            <div class="user-avatar">${(user.displayName || user.username).charAt(0).toUpperCase()}</div>
            <div class="user-info">
                <div class="user-name">${user.displayName || user.username}</div>
                ${lastMessage ? `<div class="last-message">${lastMessage}</div>` : ''}
            </div>
            ${unreadCount > 0 ? `<span class="unread-badge">${unreadCount}</span>` : ''}
        `;
        
        li.onclick = () => selectUser(user);
        userList.appendChild(li);
    });
}

function getUnreadCount(userId) {
    const conv = appState.conversations.get(userId);
    return conv?.unreadCount || 0;
}

function getLastMessage(userId) {
    const conv = appState.conversations.get(userId);
    if (conv?.messages && conv.messages.length > 0) {
        const last = conv.messages[conv.messages.length - 1];
        if (!last.content) return null;
        
        const preview = last.content.length > 30 
            ? last.content.substring(0, 30) + '...' 
            : last.content;
        return last.senderId === appState.user.id 
            ? `Tú: ${preview}` 
            : preview;
    }
    return null;
}

async function selectUser(user) {
    appState.currentChat = user;
    
    // Mostrar área de chat y ocultar mensaje vacío
    document.getElementById('no-chat-selected').style.display = 'none';
    document.getElementById('chat-active').style.display = 'flex';
    
    // Ocultar botón de eliminar grupo (es chat 1:1)
    document.getElementById('delete-group-btn').style.display = 'none';
    
    // En móvil, ocultar sidebar
    if (window.innerWidth <= 768) {
        document.querySelector('.sidebar').classList.add('hidden');
    }
    
    // Actualizar UI
    document.getElementById('chat-header-name').textContent = user.displayName || user.username;
    document.querySelectorAll('.user-item').forEach(item => item.classList.remove('active'));
    event.currentTarget.classList.add('active');
    
    // Cargar mensajes de esta conversación
    await loadConversation(user.id);
    
    // Marcar como leídos
    markConversationAsRead(user.id);
}

async function loadConversation(userId) {
    // Primero mostrar mensajes guardados
    const conv = appState.conversations.get(userId);
    if (conv?.messages) {
        renderMessages(conv.messages);
    }
    
    // Luego cargar del servidor
    try {
        const response = await fetch(`${API_URLS.messages}/chat/${userId}?pageNumber=1&pageSize=50`, {
            headers: { 'Authorization': `Bearer ${appState.accessToken}` }
        });
        
        if (response.ok) {
            const data = await response.json();
            
            // Guardar en conversación
            if (!appState.conversations.has(userId)) {
                appState.conversations.set(userId, { messages: [], unreadCount: 0 });
            }
            
            // Ordenar mensajes por fecha
            const sortedMessages = data.messages.sort((a, b) => {
                return new Date(a.sentAt) - new Date(b.sentAt);
            });
            
            appState.conversations.get(userId).messages = sortedMessages;
            renderMessages(sortedMessages);
            saveState();
        }
    } catch (error) {
        console.error('Error cargando conversación:', error);
    }
}

function renderMessages(messages) {
    const container = document.getElementById('messages-container');
    container.innerHTML = '';
    
    // Ordenar mensajes por fecha (más antiguos primero)
    const sortedMessages = [...messages].sort((a, b) => {
        return new Date(a.sentAt) - new Date(b.sentAt);
    });
    
    sortedMessages.forEach(msg => {
        const div = document.createElement('div');
        div.className = `message ${msg.senderId === appState.user.id ? 'sent' : 'received'}`;
        div.setAttribute('data-message-id', msg.id);
        
        const time = new Date(msg.sentAt).toLocaleTimeString('es-AR', { 
            hour: '2-digit', 
            minute: '2-digit' 
        });
        
        // Mostrar nombre del remitente en mensajes de grupo
        const senderName = appState.currentChat?.isGroup && msg.senderId !== appState.user.id
            ? `<div class="message-sender">${msg.senderDisplayName || msg.senderUsername}</div>`
            : '';
        
        // Agregar check para mensajes enviados
        const checkIcon = msg.senderId === appState.user.id 
            ? `<span class="message-check ${msg.isRead ? 'read' : ''}">${msg.isRead ? '✓✓' : '✓'}</span>`
            : '';
        
        div.innerHTML = `
            ${senderName}
            <div class="message-content">${escapeHtml(msg.content)}</div>
            <div class="message-time">${time}${checkIcon}</div>
        `;
        
        container.appendChild(div);
    });
    
    // Scroll al final con un pequeño delay para asegurar que el DOM se actualizó
    setTimeout(() => {
        container.scrollTop = container.scrollHeight;
        
        // Marcar mensajes como leídos automáticamente
        markMessagesAsRead();
    }, 10);
}

function markMessagesAsRead() {
    if (!appState.currentChat || !appState.connection) return;
    
    const messages = appState.currentChat.isGroup 
        ? [] // Para grupos, marcar al ver
        : appState.conversations.get(appState.currentChat.id)?.messages || [];
    
    // Marcar solo mensajes recibidos (no enviados por el usuario actual)
    const unreadMessages = messages.filter(msg => 
        msg.senderId !== appState.user.id && !msg.isRead
    );
    
    unreadMessages.forEach(msg => {
        if (appState.connection) {
            appState.connection.invoke('MarkMessageAsRead', msg.id)
                .then(() => {
                    msg.isRead = true;
                })
                .catch(err => console.error('Error marcando mensaje como leído:', err));
        }
    });
}

function markConversationAsRead(userId) {
    const conv = appState.conversations.get(userId);
    if (conv) {
        conv.unreadCount = 0;
        saveState();
        renderUserList();
    }
}

// ===== SIGNALR =====

async function connectToSignalR() {
    try {
        appState.connection = new signalR.HubConnectionBuilder()
            .withUrl(API_URLS.chatHub, {
                accessTokenFactory: () => appState.accessToken
            })
            .withAutomaticReconnect()
            .build();

        // Eventos de SignalR
        appState.connection.on('ReceiveMessage', (message) => {
            handleNewMessage(message);
        });

        appState.connection.on('MessageSent', (message) => {
            handleNewMessage(message);
        });

        appState.connection.on('UserTyping', (notification) => {
            showTypingIndicator(notification);
        });

        appState.connection.on('MessageRead', (data) => {
            handleMessageRead(data);
        });

        await appState.connection.start();
        console.log('SignalR conectado');
    } catch (error) {
        console.error('Error conectando a SignalR:', error);
        setTimeout(connectToSignalR, 5000);
    }
}

function handleNewMessage(message) {
    const otherUserId = message.senderId === appState.user.id ? message.receiverId : message.senderId;
    
    // Manejar mensajes de grupo
    if (message.groupId && appState.currentChat?.isGroup && appState.currentChat.id === message.groupId) {
        // Eliminar mensaje temporal si existe (enviado por el usuario actual)
        if (message.senderId === appState.user.id) {
            const tempMessages = document.querySelectorAll('[data-message-id^="temp-"]');
            if (tempMessages.length > 0) {
                tempMessages[tempMessages.length - 1].remove();
            }
        }
        
        // Verificar si el mensaje real ya existe
        const existingMessage = document.querySelector(`[data-message-id="${message.id}"]`);
        if (existingMessage) return;
        
        const container = document.getElementById('messages-container');
        const div = document.createElement('div');
        div.className = `message ${message.senderId === appState.user.id ? 'sent' : 'received'}`;
        div.setAttribute('data-message-id', message.id);
        
        const time = new Date(message.sentAt).toLocaleTimeString('es-AR', { 
            hour: '2-digit', 
            minute: '2-digit' 
        });
        
        // Mostrar nombre del remitente en mensajes de grupo
        const senderName = message.senderId !== appState.user.id
            ? `<div class="message-sender">${message.senderDisplayName || message.senderUsername}</div>`
            : '';
        
        div.innerHTML = `
            ${senderName}
            <div class="message-content">${escapeHtml(message.content)}</div>
            <div class="message-time">${time}</div>
        `;
        
        container.appendChild(div);
        container.scrollTop = container.scrollHeight;
        return;
    }
    
    // Mensajes 1:1
    if (!appState.conversations.has(otherUserId)) {
        appState.conversations.set(otherUserId, { messages: [], unreadCount: 0 });
    }
    
    const conv = appState.conversations.get(otherUserId);
    
    // Evitar duplicados verificando si el mensaje ya existe
    const messageExists = conv.messages.some(m => m.id === message.id);
    if (messageExists) return;
    
    conv.messages.push(message);
    
    // Si no es el chat actual, incrementar no leídos
    if (appState.currentChat?.id !== otherUserId || appState.currentChat?.isGroup) {
        conv.unreadCount = (conv.unreadCount || 0) + 1;
    }
    
    saveState();
    renderUserList();
    
    // Si es el chat actual, mostrar mensaje reemplazando el temporal
    if (appState.currentChat?.id === otherUserId && !appState.currentChat?.isGroup) {
        // Eliminar mensaje temporal si existe (enviado por el usuario actual)
        if (message.senderId === appState.user.id) {
            const tempMessages = document.querySelectorAll('[data-message-id^="temp-"]');
            if (tempMessages.length > 0) {
                tempMessages[tempMessages.length - 1].remove();
            }
        }
        
        // Verificar si el mensaje real ya existe
        const existingMessage = document.querySelector(`[data-message-id="${message.id}"]`);
        if (!existingMessage) {
            const container = document.getElementById('messages-container');
            const div = document.createElement('div');
            div.className = `message ${message.senderId === appState.user.id ? 'sent' : 'received'}`;
            div.setAttribute('data-message-id', message.id);
            
            const time = new Date(message.sentAt).toLocaleTimeString('es-AR', { 
                hour: '2-digit', 
                minute: '2-digit' 
            });
            
            div.innerHTML = `
                <div class="message-content">${escapeHtml(message.content)}</div>
                <div class="message-time">${time}</div>
            `;
            
            container.appendChild(div);
            container.scrollTop = container.scrollHeight;
        }
    }
}

function showTypingIndicator(notification) {
    if (appState.currentChat?.id === notification.userId) {
        const indicator = document.getElementById('typing-indicator');
        indicator.style.display = 'block';
        
        clearTimeout(appState.typingTimeout);
        appState.typingTimeout = setTimeout(() => {
            indicator.style.display = 'none';
        }, 3000);
    }
}

function handleMessageRead(data) {
    // Actualizar el mensaje en la conversación
    if (appState.currentChat && !appState.currentChat.isGroup) {
        const conv = appState.conversations.get(appState.currentChat.id);
        if (conv) {
            const message = conv.messages.find(m => m.id === data.messageId);
            if (message) {
                message.isRead = true;
                message.readAt = data.readAt;
                
                // Actualizar visualmente el check en el DOM
                updateMessageReadStatus(data.messageId, true);
            }
        }
    }
}

function updateMessageReadStatus(messageId, isRead) {
    const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
    if (messageElement && messageElement.classList.contains('sent')) {
        let checkIcon = messageElement.querySelector('.message-check');
        if (!checkIcon) {
            checkIcon = document.createElement('span');
            checkIcon.className = 'message-check';
            const timeElement = messageElement.querySelector('.message-time');
            if (timeElement) {
                timeElement.appendChild(checkIcon);
            }
        }
        
        checkIcon.textContent = isRead ? '✓✓' : '✓';
        checkIcon.className = isRead ? 'message-check read' : 'message-check';
    }
}

// ===== ENVIAR MENSAJES =====

async function sendMessage() {
    const input = document.getElementById('message-input');
    const content = input.value.trim();
    
    if (!content || !appState.currentChat) {
        return;
    }
    
    try {
        // Crear mensaje optimista para mostrar inmediatamente
        const tempMessage = {
            id: `temp-${Date.now()}`,
            senderId: appState.user.id,
            senderUsername: appState.user.username,
            senderDisplayName: appState.user.displayName,
            content: content,
            sentAt: new Date().toISOString(),
            isGroup: appState.currentChat.isGroup
        };
        
        if (appState.currentChat.isGroup) {
            tempMessage.groupId = appState.currentChat.id;
        } else {
            tempMessage.receiverId = appState.currentChat.id;
        }
        
        // Renderizar el mensaje inmediatamente
        renderOptimisticMessage(tempMessage);
        
        // Determinar si es mensaje grupal o individual
        const messagePayload = appState.currentChat.isGroup 
            ? { groupId: appState.currentChat.id, content: content, type: 0 }
            : { receiverId: appState.currentChat.id, content: content, type: 0 };
        
        // Enviar mensaje por SignalR
        await appState.connection.invoke('SendMessage', messagePayload);
        
        input.value = '';
    } catch (error) {
        console.error('Error enviando mensaje:', error);
    }
}

function renderOptimisticMessage(message) {
    const container = document.getElementById('messages-container');
    const div = document.createElement('div');
    div.className = 'message sent';
    div.setAttribute('data-message-id', message.id);
    
    const time = new Date(message.sentAt).toLocaleTimeString('es-AR', { 
        hour: '2-digit', 
        minute: '2-digit' 
    });
    
    div.innerHTML = `
        <div class="message-content">${escapeHtml(message.content)}</div>
        <div class="message-time">${time}</div>
    `;
    
    container.appendChild(div);
    container.scrollTop = container.scrollHeight;
}

function onTyping() {
    if (appState.currentChat && appState.connection) {
        // NotifyTyping espera (chatId, isGroup, isTyping)
        appState.connection.invoke('NotifyTyping', appState.currentChat.id, false, true)
            .catch(err => console.error(err));
    }
}

// Enviar con Enter
document.addEventListener('DOMContentLoaded', () => {
    const input = document.getElementById('message-input');
    if (input) {
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });
    }
    
    // Botón volver en móvil
    const chatHeader = document.querySelector('.chat-header');
    if (chatHeader) {
        chatHeader.addEventListener('click', (e) => {
            if (window.innerWidth <= 768 && e.target.classList.contains('chat-header')) {
                document.querySelector('.sidebar').classList.remove('hidden');
            }
        });
    }
});

// ===== UTILIDADES =====

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ===== TABS Y MODALES =====

function showSidebarTab(tabName) {
    document.querySelectorAll('.tab-sidebar').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    
    event.target.classList.add('active');
    document.getElementById(`${tabName}-tab`).classList.add('active');
    
    if (tabName === 'groups') {
        loadGroups();
    }
}

function showNewChatModal() {
    const modal = document.getElementById('new-chat-modal');
    const usersList = document.getElementById('available-users-list');
    
    usersList.innerHTML = '';
    
    appState.users.forEach(user => {
        const li = document.createElement('li');
        li.className = 'available-user-item';
        li.innerHTML = `
            <div class="user-avatar">${(user.displayName || user.username).charAt(0).toUpperCase()}</div>
            <div class="user-info">
                <div class="user-name">${user.displayName || user.username}</div>
            </div>
        `;
        li.onclick = () => {
            closeModal('new-chat-modal');
            selectUserById(user.id);
        };
        usersList.appendChild(li);
    });
    
    modal.classList.add('active');
}

function showNewGroupModal() {
    const modal = document.getElementById('new-group-modal');
    const membersList = document.getElementById('group-members-list');
    
    membersList.innerHTML = '';
    
    appState.users.forEach(user => {
        const div = document.createElement('div');
        div.className = 'member-checkbox';
        div.innerHTML = `
            <input type="checkbox" id="member-${user.id}" value="${user.id}">
            <label for="member-${user.id}">${user.displayName || user.username}</label>
        `;
        membersList.appendChild(div);
    });
    
    modal.classList.add('active');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

function selectUserById(userId) {
    const user = appState.users.find(u => u.id === userId);
    if (user) {
        // Crear el evento para que selectUser funcione
        window.event = { currentTarget: null };
        selectUser(user);
        
        // Cambiar a la pestaña de chats
        document.querySelectorAll('.tab-sidebar').forEach(btn => btn.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
        document.querySelector('.tab-sidebar').classList.add('active');
        document.getElementById('chats-tab').classList.add('active');
    }
}

// ===== GRUPOS =====

async function loadGroups() {
    try {
        const response = await fetch(`${API_URLS.groups}`, {
            headers: { 'Authorization': `Bearer ${appState.accessToken}` }
        });
        
        if (response.ok) {
            appState.groups = await response.json();
            
            // Sincronizar todos los miembros de todos los grupos
            for (const group of appState.groups) {
                if (group.members && group.members.length > 0) {
                    await syncGroupMembers(group);
                }
            }
            
            renderGroupList();
        }
    } catch (error) {
        console.error('Error cargando grupos:', error);
    }
}

function renderGroupList() {
    const groupList = document.getElementById('group-list');
    groupList.innerHTML = '';
    
    if (appState.groups.length === 0) {
        groupList.innerHTML = '<li style="padding: 20px; text-align: center; color: #999;">No tienes grupos</li>';
        return;
    }
    
    appState.groups.forEach(group => {
        const li = document.createElement('li');
        li.className = 'user-item';
        
        li.innerHTML = `
            <div class="user-avatar">👥</div>
            <div class="user-info">
                <div class="user-name">${group.name}</div>
                <div class="last-message">${group.membersCount || group.members?.length || 0} miembros</div>
            </div>
        `;
        
        li.onclick = () => selectGroup(group);
        groupList.appendChild(li);
    });
}

async function createGroup() {
    const name = document.getElementById('group-name').value.trim();
    const description = document.getElementById('group-description').value.trim();
    
    if (!name) {
        alert('El nombre del grupo es requerido');
        return;
    }
    
    const selectedMembers = [];
    document.querySelectorAll('#group-members-list input[type="checkbox"]:checked').forEach(checkbox => {
        selectedMembers.push(parseInt(checkbox.value));
    });
    
    try {
        const response = await fetch(`${API_URLS.groups}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${appState.accessToken}`
            },
            body: JSON.stringify({
                name: name,
                description: description,
                memberIds: selectedMembers
            })
        });
        
        if (response.ok) {
            const group = await response.json();
            closeModal('new-group-modal');
            document.getElementById('group-name').value = '';
            document.getElementById('group-description').value = '';
            
            // Agregar el grupo a la lista local
            appState.groups.push(group);
            renderGroupList();
            
            // Sincronizar miembros del grupo recién creado
            await syncGroupMembers(group);
            
            // Seleccionar el grupo recién creado
            selectGroup(group);
        } else {
            const error = await response.text();
            alert('Error al crear el grupo: ' + error);
        }
    } catch (error) {
        console.error('Error creando grupo:', error);
        alert('Error al crear el grupo');
    }
}

function selectGroup(group) {
    appState.currentChat = {
        id: group.id,
        name: group.name,
        isGroup: true,
        members: group.members
    };
    
    // Mostrar área de chat
    document.getElementById('no-chat-selected').style.display = 'none';
    document.getElementById('chat-active').style.display = 'flex';
    
    // Mostrar botón de eliminar grupo
    document.getElementById('delete-group-btn').style.display = 'block';
    
    // En móvil, ocultar sidebar
    if (window.innerWidth <= 768) {
        document.querySelector('.sidebar').classList.add('hidden');
    }
    
    // Actualizar header
    document.getElementById('chat-header-name').textContent = group.name;
    
    // Marcar como activo
    document.querySelectorAll('.user-item').forEach(item => item.classList.remove('active'));
    event.currentTarget.classList.add('active');
    
    // Unirse al grupo en SignalR
    if (appState.connection) {
        appState.connection.invoke('JoinChat', group.id, true)
            .catch(err => console.error('Error uniéndose al grupo:', err));
    }
    
    // Limpiar mensajes y cargar los del grupo
    document.getElementById('messages-container').innerHTML = '';
    loadGroupMessages(group.id);
}

async function syncGroupMembers(group) {
    if (!group.members || group.members.length === 0) return;
    
    try {
        for (const member of group.members) {
            await fetch(`${API_URLS.messages}/sync-user`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${appState.accessToken}`
                },
                body: JSON.stringify({
                    id: member.userId,
                    username: member.username,
                    displayName: member.displayName,
                    avatarUrl: member.avatarUrl,
                    isOnline: member.isOnline || false,
                    lastSeen: member.lastSeen || new Date().toISOString()
                })
            });
        }
    } catch (error) {
        console.error('Error sincronizando miembros del grupo:', error);
    }
}

async function loadGroupMessages(groupId) {
    try {
        const response = await fetch(`${API_URLS.messages}/group/${groupId}?pageNumber=1&pageSize=50`, {
            headers: { 'Authorization': `Bearer ${appState.accessToken}` }
        });
        
        if (response.ok) {
            const data = await response.json();
            
            // Ordenar mensajes por fecha
            const sortedMessages = (data.messages || []).sort((a, b) => {
                return new Date(a.sentAt) - new Date(b.sentAt);
            });
            
            renderMessages(sortedMessages);
        }
    } catch (error) {
        console.error('Error cargando mensajes del grupo:', error);
    }
}

async function deleteCurrentGroup() {
    if (!appState.currentChat || !appState.currentChat.isGroup) {
        return;
    }
    
    const groupName = appState.currentChat.name;
    const confirmed = confirm(`¿Estás seguro de que deseas eliminar el grupo "${groupName}"?`);
    
    if (!confirmed) return;
    
    try {
        const response = await fetch(`${API_URLS.groups}/${appState.currentChat.id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${appState.accessToken}`
            }
        });
        
        if (response.ok) {
            // Eliminar del estado local
            appState.groups = appState.groups.filter(g => g.id !== appState.currentChat.id);
            
            // Actualizar UI
            renderGroupList();
            
            // Volver a vista sin chat seleccionado
            document.getElementById('chat-active').style.display = 'none';
            document.getElementById('no-chat-selected').style.display = 'flex';
            appState.currentChat = null;
            
            alert('Grupo eliminado exitosamente');
        } else {
            const error = await response.text();
            alert('Error al eliminar el grupo: ' + error);
        }
    } catch (error) {
        console.error('Error eliminando grupo:', error);
        alert('Error al eliminar el grupo');
    }
}
