class GroupChat {
    constructor() {
        const appData = document.getElementById('app-data');
        this.currentUserId = appData.dataset.currentUserId;
        this.groups = JSON.parse(appData.dataset.initialGroups || '[]');
        this.selectedGroup = null;
        this.allUsers = [];
        this.groupParticipants = [];
        this.selectedMembers = new Set();
        this.initElements();
        this.attachEventListeners();
        this.renderGroups();
    }

    initElements() {
        this.groupsList = document.getElementById('groupsList');
        this.addGroupBtn = document.getElementById('addGroupBtn');
        this.groupsSidebar = document.getElementById('groupsSidebar');
        this.backBtn = document.getElementById('backBtn');
        this.groupModal = document.getElementById('groupModal');
        this.closeModalBtn = document.getElementById('closeModalBtn');
        this.createGroupForm = document.getElementById('createGroupForm');
        this.cancelBtn = document.getElementById('cancelBtn');
        this.submitBtn = document.getElementById('submitBtn');
        this.groupTitle = document.getElementById('groupTitle');
        this.groupDescription = document.getElementById('groupDescription');
        this.formError = document.getElementById('formError');
        this.chatHeader = document.getElementById('chatHeader');
        this.noGroupSelected = document.getElementById('noGroupSelected');
        this.chatMessages = document.getElementById('chatMessages');
        this.selectedGroupName = document.getElementById('selectedGroupName');
        this.selectedGroupDesc = document.getElementById('selectedGroupDesc');
        this.dmForm = document.getElementById('dmForm');
        this.messageInput = document.getElementById('messageInput');
        this.sendBtn = document.getElementById('sendBtn');
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
    }


 
    // --- GROUP RENDERING ---
    renderGroups() {
        if (this.groups.length === 0) {
            this.groupsList.innerHTML = '<p class="text-sm text-slate-500 text-center py-8">No groups yet.</p>';
            return;
        }
        this.groupsList.innerHTML = this.groups
            .map(group => {
                // We can assume 'participants' is an array in your group object
                const count = group.participants ? group.participants.length : 0;

                return `
            <button class="w-full text-left px-4 py-3 hover:bg-slate-100 border-b border-slate-100 transition-colors group-item flex justify-between items-center" data-group-id="${group.id}">
                <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-slate-900 truncate">${this.escapeHtml(group.title)}</p>
                    <p class="text-xs text-slate-500 truncate">${this.escapeHtml(group.description)}</p>
                </div>
                <span class="ml-2 bg-slate-100 text-slate-600 text-[10px] font-bold px-2 py-1 rounded-full">
                    ${count}
                </span>
            </button>
        `;
            }).join('');

        document.querySelectorAll('.group-item').forEach(btn => {
            btn.addEventListener('click', (e) => this.selectGroup(e.currentTarget.dataset.groupId));
        });
    }

    selectGroup(groupId) {
        this.selectedGroup = this.groups.find(g => g.id === groupId);
        if (!this.selectedGroup) return;

        const conId = this.selectedGroup.conversationId;
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
        this.fetchMessages(conId);
   
        this.fetchGroupParticipants();
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
                            data { id content senderId createdAt sender { username email } }
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
        if (!messages || messages.length === 0) {
            this.chatMessages.innerHTML = '<p class="text-sm text-slate-500 text-center py-8">No messages yet.</p>';
            return;
        }
        this.chatMessages.innerHTML = messages.map(msg => {
            const isMe = msg.senderId === this.currentUserId;
            return `
                <div class="flex ${isMe ? 'justify-end' : 'justify-start'} mb-4">
                    <div class="flex flex-col ${isMe ? 'items-end' : 'items-start'}">
                        <span class="text-[10px] font-bold text-slate-500 mb-1">@${(msg.sender?.username || 'unknown').toLowerCase()}</span>
                        <div class="${isMe ? 'bg-sky-600 text-white rounded-tr-none' : 'bg-slate-100 text-slate-900 rounded-tl-none'} rounded-2xl px-4 py-2 max-w-xs text-sm shadow-sm">
                            ${this.escapeHtml(msg.content)}
                        </div>
                    </div>
                </div>`;
        }).join('');
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    }

    // --- MEMBER MANAGEMENT LOGIC ---
    async openAddMembersModal() {
        this.addMembersModal.classList.remove('hidden');
        this.usersList.innerHTML = '<p class="text-center py-4">Loading users...</p>';
        this.selectedMembers.clear();

        // Fetch current members first, then all users
        await this.fetchGroupParticipants();
        await this.fetchAllUsers();
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

            // 1. Update our local data
            this.groupParticipants = participants;
            this.selectedGroup.participants = participants;

            // 2. Update the sidebar count immediately
            this.updateSidebarCount(this.selectedGroup.id, participants.length);

        } catch (e) {
            console.error("Error fetching participants:", e);
        }
    }

    async fetchAllUsers() {
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: `query GetUsers { allUsers { id username email } }`
                })
            });
            const result = await response.json();
            this.allUsers = result.data?.allUsers || [];
            this.renderUsersList(this.allUsers);
        } catch (e) {
            this.usersList.innerHTML = '<p class="text-red-500 text-center py-4">Error loading users</p>';
        }
    }

    renderUsersList(users) {
        // LOGIC: Filter out users who are already in groupParticipants
        const participantIds = new Set(this.groupParticipants.map(p => p.id));

        const availableUsers = users.filter(user =>
            user.id !== this.currentUserId && !participantIds.has(user.id)
        );

        if (availableUsers.length === 0) {
            this.usersList.innerHTML = '<p class="text-sm text-slate-500 text-center py-4">No new users to add.</p>';
            return;
        }

        this.usersList.innerHTML = availableUsers.map(user => `
            <label class="flex items-center gap-3 px-4 py-3 hover:bg-slate-50 cursor-pointer border-b border-slate-100">
                <input type="checkbox" class="user-checkbox" data-user-id="${user.id}">
                <div class="flex-1">
                    <div class="text-sm font-medium text-slate-900">${this.escapeHtml(user.username || user.email)}</div>
                    <div class="text-xs text-slate-500">${this.escapeHtml(user.email)}</div>
                </div>
            </label>
        `).join('');

        // Attach listeners to checkboxes
        this.usersList.querySelectorAll('.user-checkbox').forEach(cb => {
            cb.addEventListener('change', (e) => {
                if (e.target.checked) this.selectedMembers.add(e.target.dataset.userId);
                else this.selectedMembers.delete(e.target.dataset.userId);
            });
        });
    }

    handleSendMessage(e) {
        e.preventDefault();
        const message = this.messageInput.value.trim();
        if (!message || !this.selectedGroup) return;

        const msgEl = document.createElement('div');
        msgEl.className = 'flex justify-end';
        msgEl.innerHTML = `
            <div class="bg-sky-600 text-white rounded-lg px-4 py-2 max-w-xs text-sm">
                ${this.escapeHtml(message)}
            </div>
        `;
        this.chatMessages.appendChild(msgEl);
        this.messageInput.value = '';
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    }

    showFormError(message) {
        this.formError.textContent = message;
        this.formError.classList.remove('hidden');
    }

    getAntiforgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    escapeHtml(text) {
        if (!text) return "";
        const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
        return text.replace(/[&<>"']/g, m => map[m]);
    }

    async handleCreateGroup(e) {
        e.preventDefault();
        this.submitBtn.disabled = true;
        this.submitBtn.textContent = 'Creating...';

        try {
            const formData = new FormData(this.createGroupForm);
            const response = await fetch('?handler=CreateGroup', {
                method: 'POST',
                body: formData,
                headers: { 'RequestVerificationToken': this.getAntiforgeryToken() }
            });

            const result = await response.json();
            if (response.ok && result.statusCode >= 200 && result.statusCode < 300) {
                window.location.reload();
            } else {
                this.showFormError(result?.message || 'Failed to create group');
                this.submitBtn.disabled = false;
                this.submitBtn.textContent = 'Create Group';
            }
        } catch (error) {
            this.showFormError('An error occurred while creating the group');
            this.submitBtn.disabled = false;
        }
    }
}




    filterUsers(query) {
        const filtered = this.allUsers.filter(u =>
            (u.username?.toLowerCase().includes(query.toLowerCase())) ||
            (u.email?.toLowerCase().includes(query.toLowerCase()))
        );
        this.renderUsersList(filtered);
    }

    // --- HELPERS ---
    closeAddMembersModal() {
        this.addMembersModal.classList.add('hidden');
    }


    openModal() {
        this.groupModal.classList.remove('hidden')
    }
    closeModal() {
        this.groupModal.classList.add('hidden');

        // optional cleanup
        this.createGroupForm.reset();
        this.formError.classList.add('hidden');
}

    resetForm() {
    this.createGroupForm.reset();
    this.formError.classList.add('hidden');
    document.querySelectorAll('[id$="Error"]').forEach(el => el.classList.add('hidden'));
}

    updateSidebarCount(groupId, count) {
        const groupButton = document.querySelector(`button[data-group-id="${groupId}"]`);
        if (groupButton) {
            const countBadge = groupButton.querySelector('span.rounded-full');
            if (countBadge) {
                countBadge.textContent = count;
            }
        }
    }

    deselectGroup() {
        this.selectedGroup = null;
        this.noGroupSelected.classList.remove('hidden');
        this.chatHeader.classList.add('hidden');
        this.chatMessages.classList.add('hidden');
        this.dmForm.classList.add('hidden');
        this.addMembersBtn.classList.add('hidden');
        this.groupsSidebar.classList.remove('max-md:-left-80');
    }

    escapeHtml(t) {
        if (!t) return "";
        const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
        return t.replace(/[&<>"']/g, m => map[m]);
    }

    getAntiforgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }
}

document.addEventListener('DOMContentLoaded', () => new GroupChat());