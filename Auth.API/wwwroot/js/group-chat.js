class GroupChat {
    constructor() {
        const appData = document.getElementById('app-data');
        this.currentUserId = appData.dataset.currentUserId;
        this.groups = JSON.parse(appData.dataset.initialGroups || '[]');
        this.selectedGroup = null;
        this.allUsers = [];
        this.groupParticipants = [];
        this.selectedMembers = new Set();
        this.socket = null;
        this.typingTimeout = null;

        this.initElements();
        this.attachEventListeners();
        this.renderGroups();
    }

    initElements() {
        // Sidebar & List
        this.groupsList = document.getElementById('groupsList');
        this.addGroupBtn = document.getElementById('addGroupBtn');
        this.groupsSidebar = document.getElementById('groupsSidebar');
        this.backBtn = document.getElementById('backBtn');

        // Modals
        this.groupModal = document.getElementById('groupModal');
        this.closeModalBtn = document.getElementById('closeModalBtn');
        this.createGroupForm = document.getElementById('createGroupForm');
        this.cancelBtn = document.getElementById('cancelBtn');
        this.submitBtn = document.getElementById('submitBtn');
        this.formError = document.getElementById('formError');

        // Chat Area
        this.chatHeader = document.getElementById('chatHeader');
        this.noGroupSelected = document.getElementById('noGroupSelected');
        this.chatMessages = document.getElementById('chatMessages');
        this.selectedGroupName = document.getElementById('selectedGroupName');
        this.selectedGroupDesc = document.getElementById('selectedGroupDesc');
        this.dmForm = document.getElementById('dmForm');
        this.messageInput = document.getElementById('messageInput');
        this.sendBtn = document.getElementById('sendBtn');

        // Members Modal
        this.addMembersBtn = document.getElementById('addMembersBtn');
        this.addMembersModal = document.getElementById('addMembersModal');
        this.closeAddMembersBtn = document.getElementById('closeAddMembersBtn');
        this.userSearchInput = document.getElementById('userSearchInput');
        this.usersList = document.getElementById('usersList');
        this.addSelectedBtn = document.getElementById('addSelectedBtn');
        this.cancelAddMembersBtn = document.getElementById('cancelAddMembersBtn');
    }

    attachEventListeners() {
        this.addGroupBtn.addEventListener('click', () => this.openModal());
        this.closeModalBtn.addEventListener('click', () => this.closeModal());
        this.cancelBtn.addEventListener('click', () => this.closeModal());
        this.backBtn.addEventListener('click', () => this.deselectGroup());

        this.createGroupForm.addEventListener('submit', (e) => this.handleCreateGroup(e));
        this.dmForm.addEventListener('submit', (e) => this.handleSendMessage(e));
        this.sendBtn.addEventListener('click', (e) => this.handleSendMessage(e));

        this.addMembersBtn.addEventListener('click', () => this.openAddMembersModal());
        this.closeAddMembersBtn.addEventListener('click', () => this.closeAddMembersModal());
        this.cancelAddMembersBtn.addEventListener('click', () => this.closeAddMembersModal());
        this.addSelectedBtn.addEventListener('click', () => this.handleAddMembers());
        this.userSearchInput.addEventListener('input', (e) => this.filterUsers(e.target.value));

        // Typing Indicator Listener
        this.messageInput.addEventListener('input', () => {
            this.sendTypingStatus(true);
            clearTimeout(this.typingTimeout);
            this.typingTimeout = setTimeout(() => this.sendTypingStatus(false), 3000);
        });
    }

    // --- GROUP RENDERING ---
    renderGroups() {
        if (this.groups.length === 0) {
            this.groupsList.innerHTML = '<p class="text-sm text-slate-500 text-center py-8">No groups yet.</p>';
            return;
        }
        this.groupsList.innerHTML = this.groups
            .map(group => {
                const count = group.participants ? group.participants.length :0;
                return `
            <button class="w-full text-left px-4 py-3 hover:bg-slate-100 border-b border-slate-100 transition-colors group-item flex justify-between items-center" data-group-id="${group.id}">
                <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-slate-900 truncate">${this.escapeHtml(group.title)}</p>
                    <p class="text-xs text-slate-500 truncate">${this.escapeHtml(group.description)}</p>
                </div>
                <span class="ml-2 bg-slate-100 text-slate-600 text-[10px] font-bold px-2 py-1 rounded-full">
                   ${count}
                </span>
            </button>`;
            }).join('');

        document.querySelectorAll('.group-item').forEach(btn => {
            btn.addEventListener('click', (e) => this.selectGroup(e.currentTarget.dataset.groupId));
        });
    }

    selectGroup(groupId) {
        this.selectedGroup = this.groups.find(g => g.id === groupId);
        if (!this.selectedGroup) return;

        this.selectedGroupName.textContent = this.selectedGroup.title;
        this.selectedGroupDesc.textContent = this.selectedGroup.description;

        this.noGroupSelected.classList.add('hidden');
        this.chatHeader.classList.remove('hidden');
        this.chatMessages.classList.remove('hidden');
        this.dmForm.classList.remove('hidden');
        this.addMembersBtn.classList.remove('hidden');
        this.groupsSidebar.classList.add('max-md:-left-80');

        document.querySelectorAll('.group-item').forEach(btn => {
            btn.classList.toggle('bg-sky-50', btn.dataset.groupId === groupId);
        });

        this.chatMessages.innerHTML = '<p class="text-sm text-slate-500 text-center py-8">Loading messages...</p>';

        this.fetchMessages(this.selectedGroup.conversationId);
        this.fetchGroupParticipants();
        this.setupSubscription(this.selectedGroup.conversationId);
    }

    // --- MESSAGING LOGIC ---
    async fetchMessages(conversationId) {
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: `query GetGroupMessages($conversationId: String!) {
                        groupMessages(conversationId: $conversationId) {
                            data { id content senderId createdAt sender { username } }
                        }
                    }`,
                    variables: { conversationId }
                })
            });
            const result = await response.json();
            this.renderMessages(result.data?.groupMessages?.data || []);
        } catch (error) {
            this.chatMessages.innerHTML = '<p class="text-red-500 text-center py-8">Error loading messages.</p>';
        }
    }

    renderMessages(messages) {
        this.chatMessages.innerHTML = '';
        if (messages.length === 0) {
            this.chatMessages.innerHTML = '<p class="text-sm text-slate-500 text-center py-8">No messages yet.</p>';
            return;
        }
        messages.forEach(msg => this.appendMessageToUI(msg));
    }

    async handleSendMessage(e) {
        if (e) e.preventDefault();
        const content = this.messageInput.value.trim();
        if (!content || !this.selectedGroup) return;

        try {
            const res = await fetch('/graphql', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiforgeryToken()
                },
                body: JSON.stringify({
                    query: `mutation SendGroupMessage($conversationId: UUID!, $message: String!) {
                        sendGroupMessage(conversationId: $conversationId, message: $message) {
                            message statusCode
                        }
                    }`,
                    variables: { conversationId: this.selectedGroup.conversationId, message: content }
                })
            });

            const result = await res.json();
            if (!result.errors && result.data?.sendGroupMessage?.statusCode < 400) {
                this.messageInput.value = '';
                this.sendTypingStatus(false);
                // Optionally append immediately for better UX
                this.appendMessageToUI({
                    senderId: this.currentUserId,
                    content: content,
                    sender: { username: "Me" }
                });
            }
        } catch (err) { console.error("Send error:", err); }
    }

    // --- REAL-TIME (SUBSCRIPTIONS) ---
    setupSubscription(conversationId) {
        if (this.socket) this.socket.close();

        const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
        this.socket = new WebSocket(`${protocol}//${window.location.host}/graphql`, 'graphql-transport-ws');

        this.socket.onopen = () => {
            this.socket.send(JSON.stringify({ type: 'connection_init' }));

            // Sub 1: Typing
            this.socket.send(JSON.stringify({
                id: 'typing_sub', type: 'subscribe',
                payload: {
                    query: `subscription typing($cId: UUID!) {
                        onUserTyping(conversationId: $cId) { isTyping userId username }
                    }`,
                    variables: { cId: conversationId }
                }
            }));

            // Sub 2: Messages
            this.socket.send(JSON.stringify({
                id: 'message_sub', type: 'subscribe',
                payload: {
                    query: `subscription onNewMsg($cId: UUID!) {
                        onNewGroupMessage(conversationId: $cId) {
                            id content senderId sender { username }
                        }
                    }`,
                    variables: { cId: conversationId }
                }
            }));
        };

        this.socket.onmessage = (event) => {
            const msg = JSON.parse(event.data);
            if (msg.type !== 'next') return;

            if (msg.id === 'typing_sub') {
                const data = msg.payload.data.onUserTyping;
                if (data.userId !== this.currentUserId) this.toggleTypingUI(data.isTyping, data.username);
            }

            if (msg.id === 'message_sub') {
                const newMsg = msg.payload.data.onNewGroupMessage;
                if (newMsg.senderId !== this.currentUserId) this.appendMessageToUI(newMsg);
            }
        };
    }

    sendTypingStatus(isTyping) {
        if (!this.selectedGroup) return;
        fetch('/graphql', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': this.getAntiforgeryToken() },
            body: JSON.stringify({
                query: `mutation OnTyping($cId: UUID!, $isTyping: Boolean!) {
                    setTyping(conversationId: $cId, isTyping: $isTyping)
                }`,
                variables: { cId: this.selectedGroup.conversationId, isTyping }
            })
        }).catch(() => { });
    }

    // --- MEMBER MANAGEMENT ---
    async handleAddMembers() {
        if (!this.selectedGroup || this.selectedMembers.size === 0) return;
        try {

            console.log({
                conversationId: this.selectedGroup.conversationId,
                userIds: Array.from(this.selectedMembers)
            }, "user ids")
            const response = await fetch('?handler=AddMembers', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': this.getAntiforgeryToken() },
                body: JSON.stringify({
                    conversationId: this.selectedGroup.conversationId,
                    userIds: Array.from(this.selectedMembers)
                })
            });
            const result = await response.json();
            if (result.statusCode === 201 || result.statusCode === 200) {
                alert(result.message);
                this.closeAddMembersModal();
                await this.fetchGroupParticipants(); // Refresh the UI
            } else {
                alert("Error: " + result.message);
            }
        } catch (e) { console.error(e); }
    }

    async fetchGroupParticipants() {
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: `query GetParticipants($conId: UUID!) {
                        groupParticipants(conversationId: $conId) { data { id } }
                    }`,
                    variables: { conId: this.selectedGroup.conversationId }
                })
            });
            const result = await response.json();
            const participants = result.data?.groupParticipants?.data || [];
            this.groupParticipants = participants;
            this.updateSidebarCount(this.selectedGroup.id, participants.length);
        } catch (e) { console.error(e); }
    }

    async fetchAllUsers() {
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ query: `query GetUsers { allUsers { id username email } }` })
            });
            const result = await response.json();
            this.allUsers = result.data?.allUsers || [];
            this.renderUsersList(this.allUsers);
        } catch (e) { this.usersList.innerHTML = '<p class="text-red-500 text-center py-4">Error loading users</p>'; }
    }

    renderUsersList(users) {
        const pIds = new Set(this.groupParticipants.map(p => p.id));
        const available = users.filter(u => u.id !== this.currentUserId && !pIds.has(u.id));

        if (available.length === 0) {
            this.usersList.innerHTML = '<p class="text-sm text-slate-500 text-center py-4">No new users.</p>';
            return;
        }

        this.usersList.innerHTML = available.map(u => `
            <label class="flex items-center gap-3 px-4 py-3 hover:bg-slate-50 cursor-pointer border-b border-slate-100">
                <input type="checkbox" class="user-checkbox" data-user-id="${u.id}">
                <div class="flex-1">
                    <div class="text-sm font-medium text-slate-900">${this.escapeHtml(u.username || u.email)}</div>
                </div>
            </label>`).join('');

        this.usersList.querySelectorAll('.user-checkbox').forEach(cb => {
            cb.addEventListener('change', (e) => {
                if (e.target.checked) this.selectedMembers.add(e.target.dataset.userId);
                else this.selectedMembers.delete(e.target.dataset.userId);
            });
        });
    }

    // --- UI HELPERS ---
    appendMessageToUI(msg) {
        const isMe = msg.senderId === this.currentUserId;
        const html = `
        <div class="flex ${isMe ? 'justify-end' : 'justify-start'} mb-4">
            <div class="flex flex-col ${isMe ? 'items-end' : 'items-start'}">
                <span class="text-[10px] font-bold text-slate-500 mb-1">@${(msg.sender?.username || 'unknown').toLowerCase()}</span>
                <div class="${isMe ? 'bg-sky-600 text-white rounded-tr-none' : 'bg-slate-100 text-slate-900 rounded-tl-none'} rounded-2xl px-4 py-2 max-w-xs text-sm shadow-sm">
                    ${this.escapeHtml(msg.content)}
                </div>
            </div>
        </div>`;
        this.chatMessages.insertAdjacentHTML('beforeend', html);
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    }

    toggleTypingUI(isTyping, username = 'Someone') {
        const indicator = document.getElementById('typingIndicator');
        const text = document.getElementById('typingText');
        if (isTyping) {
            text.textContent = `${username} is typing...`;
            indicator.classList.remove('hidden');
        } else {
            indicator.classList.add('hidden');
        }
    }

    async handleCreateGroup(e) {
        e.preventDefault();
        this.submitBtn.disabled = true;
        try {
            const formData = new FormData(this.createGroupForm);
            const response = await fetch('?handler=CreateGroup', {
                method: 'POST',
                body: formData,
                headers: { 'RequestVerificationToken': this.getAntiforgeryToken() }
            });
            const result = await response.json();
            if (response.ok && result.statusCode < 300) window.location.reload();
            else this.showFormError(result?.message || 'Failed');
        } catch (error) { this.showFormError('Error occurred'); }
        finally { this.submitBtn.disabled = false; }
    }

    // --- UTILS ---
    escapeHtml(t) {
        if (!t) return "";
        return t.replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[m]));
    }

    getAntiforgeryToken() { return document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''; }

    showFormError(m) { this.formError.textContent = m; this.formError.classList.remove('hidden'); }

    openModal() { this.groupModal.classList.remove('hidden'); }

    closeModal() { this.groupModal.classList.add('hidden'); this.createGroupForm.reset(); }

    openAddMembersModal() { this.addMembersModal.classList.remove('hidden'); this.fetchAllUsers(); }

    closeAddMembersModal() { this.addMembersModal.classList.add('hidden'); }

    filterUsers(q) {
        const filtered = this.allUsers.filter(u => u.username?.toLowerCase().includes(q.toLowerCase()) || u.email?.toLowerCase().includes(q.toLowerCase()));
        this.renderUsersList(filtered);
    }

    updateSidebarCount(id, count) {
        const badge = document.querySelector(`button[data-group-id="${id}"] span.rounded-full`);
        if (badge) badge.textContent = count;
    }

    deselectGroup() {
        this.selectedGroup = null;
        this.noGroupSelected.classList.remove('hidden');
        this.chatHeader.classList.add('hidden');
        this.chatMessages.classList.add('hidden');
        this.dmForm.classList.add('hidden');
        this.addMembersBtn.classList.add('hidden');
        this.groupsSidebar.classList.remove('max-md:-left-80');
        if (this.socket) this.socket.close();
    }
}

document.addEventListener('DOMContentLoaded', () => new GroupChat());