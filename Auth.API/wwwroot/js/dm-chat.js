class DirectMessageChat {
    constructor() {
        const dataEl = document.getElementById('app-data');
        this.currentUserId = dataEl.dataset.currentUserId;
        this.initialUsers = JSON.parse(dataEl.dataset.initialUsers);

        this.selectedUserId = null;
        this.selectedUserName = null;

        this.socket = null;
        this.typingTimeout = null;
        this.conversationId = null;

        this.initializeElements();
        this.attachEventListeners();

        if (this.initialUsers && this.initialUsers.length > 0) {
            this.renderUsersList(this.initialUsers);
        } else {
            this.elements.usersLoading.textContent = 'No users found.';
        }
    }

    initializeElements() {
        this.elements = {
            usersSidebar: document.getElementById('usersSidebar'),
            usersList: document.getElementById('usersList'),
            usersLoading: document.getElementById('usersLoading'),
            chatHeader: document.getElementById('chatHeader'),
            noUserSelected: document.getElementById('noUserSelected'),
            chat: document.getElementById('chat'),
            dmForm: document.getElementById('dmForm'),
            messageInput: document.getElementById('messageInput'),
            sendBtn: document.getElementById('sendBtn'),
            backBtn: document.getElementById('backBtn'),
            selectedUserName: document.getElementById('selectedUserName'),
            typingIndicator: document.getElementById('typingIndicator'),
            typingText: document.getElementById('typingText')
        };
    }

    attachEventListeners() {
        this.elements.sendBtn.addEventListener('click', (e) => this.handleSendMessage(e));
        this.elements.backBtn.addEventListener('click', () => this.handleBackButton());

        this.elements.messageInput.addEventListener('input', () => {
            if (!this.conversationId) return;

            this.emitTyping(true);
            clearTimeout(this.typingTimeout);
            this.typingTimeout = setTimeout(() => {
                this.emitTyping(false);
            }, 3000); // Reduced to 3s for better responsiveness
        });

        this.elements.messageInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.handleSendMessage();
            }
        });
    }

    renderUsersList(users) {
        this.elements.usersList.innerHTML = '';
        this.elements.usersLoading.classList.add('hidden');

        users.forEach(user => {
            const userItem = document.createElement('div');
            userItem.className = 'flex items-center gap-3 px-4 py-3 hover:bg-slate-50 cursor-pointer transition-colors border-b border-slate-100 user-item';
            userItem.dataset.userId = user.id;

            userItem.innerHTML = `
                <div class="flex-1 min-w-0">
                    <div class="font-medium text-slate-900 text-sm truncate">${this.escapeHtml(user.username || user.email)}</div>
                    <div class="text-xs text-slate-500 truncate">${this.escapeHtml(user.email)}</div>
                </div>
            `;

            userItem.addEventListener('click', () => this.selectUser(user.id, user.username || user.email));
            this.elements.usersList.appendChild(userItem);
        });
    }

    async selectUser(userId, userName) {
        this.selectedUserId = userId;
        this.selectedUserName = userName;
        this.conversationId = null; // Reset for new selection

        if (window.innerWidth < 768) this.elements.usersSidebar.classList.add('hidden');

        this.elements.chatHeader.classList.remove('hidden');
        this.elements.noUserSelected.classList.add('hidden');
        this.elements.chat.classList.remove('hidden');
        this.elements.dmForm.classList.remove('hidden');
        this.elements.typingIndicator.classList.add('hidden'); // Hide indicator on switch

        this.elements.selectedUserName.textContent = userName;
        this.elements.chat.innerHTML = '<div class="text-center p-4 text-slate-400">Loading messages...</div>';

        this.highlightSelectedUser(userId);
        await this.fetchMessages(userId);
    }

    async fetchMessages(otherUserId) {
        try {
            const res = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: `
                    query GetHistory($otherId: String!) {
                        messages(otherUserId: $otherId) {
                            conversationId
                            content
                            senderId
                            createdAt
                        }
                    }`,
                    variables: { otherId: otherUserId }
                })
            });

            const result = await res.json();
            const messages = result.data?.messages || [];

            this.elements.chat.innerHTML = '';

            if (messages.length === 0) {
                this.elements.chat.innerHTML = '<div class="text-center p-4 text-slate-400">No messages yet.</div>';
            } else {
                this.conversationId = messages[0].conversationId;
                messages.forEach(msg => {
                    const isMine = String(msg.senderId) === String(this.currentUserId);
                    this.appendMessageToChat(msg.content, isMine);
                });
                this.setupSubscription(this.conversationId);
            }
        } catch (err) {
            console.error(err);
            this.elements.chat.innerHTML = '<div class="text-center p-4 text-red-500">Error loading messages.</div>';
        }
    }

    setupSubscription(conversationId) {
        if (this.socket && this.socket.readyState === WebSocket.OPEN) {
            this.socket.close();
        }

        const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
        this.socket = new WebSocket(`${protocol}//${window.location.host}/graphql`, 'graphql-transport-ws');

        this.socket.onopen = () => {
            this.socket.send(JSON.stringify({ type: 'connection_init' }));
            this.socket.send(JSON.stringify({
                id: 'typing_sub',
                type: 'subscribe',
                payload: {
                    query: `subscription typing($cId: UUID!) {
                        onUserTyping(conversationId: $cId) {
                            isTyping
                            userId
                        }
                    }`,
                    variables: { cId: conversationId }
                }
            }));
        };

        this.socket.onmessage = (event) => {
            const msg = JSON.parse(event.data);
            if (msg.type === 'next' && msg.id === 'typing_sub') {
                const data = msg.payload.data.onUserTyping;
                // Only show if it's the OTHER user typing in THIS chat
                if (data.userId !== this.currentUserId) {
                    this.toggleTypingUI(data.isTyping);
                }
            }
        };
    }

    toggleTypingUI(isTyping) {
        if (isTyping) {
            this.elements.typingText.textContent = `${this.selectedUserName} is typing...`;
            this.elements.typingIndicator.classList.remove('hidden');
            this.elements.chat.scrollTop = this.elements.chat.scrollHeight;
        } else {
            this.elements.typingIndicator.classList.add('hidden');
        }
    }

    async handleSendMessage() {
        const content = this.elements.messageInput.value.trim();
        if (!content || !this.selectedUserId) return;

        try {
            const res = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: `
                    mutation($r: String!, $m: String!) {
                        sendDirectMessage(receiverId: $r, message: $m) { 
                            conversationId 
                            content 
                            senderId 
                        }
                    }`,
                    variables: { r: this.selectedUserId, m: content }
                })
            });

            const result = await res.json();
            if (!result.errors) {
                const sentMsg = result.data.sendDirectMessage;

                // If this was the first message, we now have a conversationId
                if (!this.conversationId) {
                    this.conversationId = sentMsg.conversationId;
                    this.setupSubscription(this.conversationId);
                }

                // Remove "No messages yet" text if it's there
                if (this.elements.chat.querySelector('.text-slate-400')) {
                    this.elements.chat.innerHTML = '';
                }

                this.appendMessageToChat(content, true);
                this.elements.messageInput.value = '';
                this.emitTyping(false);
            }
        } catch (err) {
            console.error(err);
        }
    }

    emitTyping(isTyping) {
        if (!this.conversationId) return;
        fetch('/graphql', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                query: `mutation OnTyping($cId: UUID!, $isTyping: Boolean!) {
                    setTyping(conversationId: $cId, isTyping: $isTyping)
                }`,
                variables: { cId: this.conversationId, isTyping }
            })
        }).catch(() => { });
    }

    appendMessageToChat(content, isMine) {
        const msgEl = document.createElement('div');
        msgEl.className = `flex ${isMine ? 'justify-end' : 'justify-start'} mb-2`;
        msgEl.innerHTML = `
            <div class="${isMine ? 'bg-sky-600 text-white' : 'bg-slate-200 text-slate-900'} rounded-lg px-3 py-2 max-w-[80%] text-sm shadow-sm">
                ${this.escapeHtml(content)}
            </div>`;
        this.elements.chat.appendChild(msgEl);
        this.elements.chat.scrollTop = this.elements.chat.scrollHeight;
    }

    highlightSelectedUser(userId) {
        document.querySelectorAll('.user-item').forEach(item => {
            item.classList.toggle('bg-sky-50', item.dataset.userId === userId);
        });
    }

    handleBackButton() {
        this.elements.usersSidebar.classList.remove('hidden');
        this.elements.chatHeader.classList.add('hidden');
        this.elements.chat.classList.add('hidden');
        this.elements.dmForm.classList.add('hidden');
        this.elements.noUserSelected.classList.remove('hidden');
        this.selectedUserId = null;
    }

    escapeHtml(str) {
        const d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }
}

document.addEventListener('DOMContentLoaded', () => new DirectMessageChat());