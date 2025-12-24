class GroupChat {
    constructor() {
        this.currentUserId = document.getElementById('app-data').dataset.currentUserId;
        this.groups = JSON.parse(document.getElementById('app-data').dataset.initialGroups || '[]');
        this.selectedGroup = null;
        this.allUsers = [];
        this.groupParticipants = [];
        this.selectedMembers = new Set();
        this.initElements();
        this.attachEventListeners();
        this.renderGroups();
    }

    initElements() {
        // Sidebar
        this.groupsList = document.getElementById('groupsList');
        this.addGroupBtn = document.getElementById('addGroupBtn');
        this.groupsSidebar = document.getElementById('groupsSidebar');
        this.backBtn = document.getElementById('backBtn');

        // Modal
        this.groupModal = document.getElementById('groupModal');
        this.closeModalBtn = document.getElementById('closeModalBtn');
        this.createGroupForm = document.getElementById('createGroupForm');
        this.cancelBtn = document.getElementById('cancelBtn');
        this.submitBtn = document.getElementById('submitBtn');

        // Form inputs
        this.groupTitle = document.getElementById('groupTitle');
        this.groupDescription = document.getElementById('groupDescription');
        this.groupStatus = document.getElementById('groupStatus');
        this.formError = document.getElementById('formError');

        // Chat area
        this.chatHeader = document.getElementById('chatHeader');
        this.noGroupSelected = document.getElementById('noGroupSelected');
        this.chatMessages = document.getElementById('chatMessages');
        this.selectedGroupName = document.getElementById('selectedGroupName');
        this.selectedGroupDesc = document.getElementById('selectedGroupDesc');
        this.dmForm = document.getElementById('dmForm');
        this.messageInput = document.getElementById('messageInput');
        this.sendBtn = document.getElementById('sendBtn');

        // Add Members elements
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
        this.sendBtn.addEventListener('click', (e) => this.handleSendMessage(e));

        // Add Members event listeners
        this.addMembersBtn.addEventListener('click', () => this.openAddMembersModal());
        this.closeAddMembersBtn.addEventListener('click', () => this.closeAddMembersModal());
        this.cancelAddMembersBtn.addEventListener('click', () => this.closeAddMembersModal());
        this.addSelectedBtn.addEventListener('click', () => this.handleAddMembers());
        this.userSearchInput.addEventListener('input', (e) => this.filterUsers(e.target.value));
    }

    renderGroups() {
        if (this.groups.length === 0) {
            this.groupsList.innerHTML = '<p class="text-sm text-slate-500 text-center py-8">No groups yet. Create one!</p>';
            return;
        }

        this.groupsList.innerHTML = this.groups
            .map(group => `
                <button class="w-full text-left px-4 py-3 hover:bg-slate-100 border-b border-slate-100 transition-colors group-item" data-group-id="${group.id}">
                    <p class="text-sm font-medium text-slate-900">${this.escapeHtml(group.title)}</p>
                    <p class="text-xs text-slate-500 truncate">${this.escapeHtml(group.description)}</p>
                </button>
            `)
            .join('');

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
            const isSelected = btn.dataset.groupId === groupId;
            btn.classList.toggle('bg-sky-50', isSelected);
            btn.classList.toggle('border-l-4', isSelected);
            btn.classList.toggle('border-l-sky-600', isSelected);
        });

        this.chatMessages.innerHTML = '<p class="text-sm text-slate-500 text-center py-8">No messages yet. Start a conversation!</p>';
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

    openModal() {
        this.groupModal.classList.remove('hidden');
        this.groupTitle.focus();
        this.resetForm();
    }

    closeModal() {
        this.groupModal.classList.add('hidden');
        this.resetForm();
    }

    resetForm() {
        this.createGroupForm.reset();
        this.formError.classList.add('hidden');
        document.querySelectorAll('[id$="Error"]').forEach(el => el.classList.add('hidden'));
    }

    async openAddMembersModal() {
        this.addMembersModal.classList.remove('hidden');
        this.selectedMembers.clear();
        this.userSearchInput.value = '';
        this.usersList.innerHTML = '<p class="text-sm text-slate-500 text-center py-4">Loading...</p>';

        // Ensure participants are loaded BEFORE users are rendered to allow filtering
        await this.fetchGroupParticipants();
        await this.fetchAllUsers();
    }

    closeAddMembersModal() {
        this.addMembersModal.classList.add('hidden');
        this.selectedMembers.clear();
        this.userSearchInput.value = '';
    }

    async fetchAllUsers() {
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: `
                        query GetUsers {
                            allUsers {
                                id
                                username
                                email
                            }
                        }
                    `
                })
            });

            const result = await response.json();
            if (result.data && result.data.allUsers) {
                this.allUsers = result.data.allUsers;
                this.renderUsersList(this.allUsers);
            } else {
                this.usersList.innerHTML = '<p class="text-sm text-red-500 text-center py-4">Error loading users</p>';
            }
        } catch (error) {
            console.error('Error fetching users:', error);
            this.usersList.innerHTML = '<p class="text-sm text-red-500 text-center py-4">Error loading users</p>';
        }
    }

    renderUsersList(users) {
        if (!users || users.length === 0) {
            this.usersList.innerHTML = '<p class="text-sm text-slate-500 text-center py-4">No users found</p>';
            return;
        }

        // Create a set of IDs for existing participants for O(1) lookup
        const participantIds = new Set(this.groupParticipants.map(p => p.id));

        // Filter: Exclude current user AND users already in the group
        const availableUsers = users.filter(user =>
            user.id !== this.currentUserId && !participantIds.has(user.id)
        );

        if (availableUsers.length === 0) {
            this.usersList.innerHTML = '<p class="text-sm text-slate-500 text-center py-4">All users are already members</p>';
            return;
        }

        this.usersList.innerHTML = availableUsers
            .map(user => `
                <label class="flex items-center gap-3 px-4 py-3 hover:bg-slate-50 cursor-pointer transition-colors border-b border-slate-100">
                    <input 
                        type="checkbox" 
                        class="user-checkbox" 
                        data-user-id="${user.id}"
                        data-user-name="${this.escapeHtml(user.username || user.email)}"
                    />
                    <div class="flex-1 min-w-0">
                        <div class="font-medium text-slate-900 text-sm">${this.escapeHtml(user.username || user.email)}</div>
                        <div class="text-xs text-slate-500 truncate">${this.escapeHtml(user.email)}</div>
                    </div>
                </label>
            `)
            .join('');

        this.attachCheckboxListeners();
    }

    attachCheckboxListeners() {
        this.usersList.querySelectorAll('.user-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const userId = e.target.dataset.userId;
                const userName = e.target.dataset.userName;

                if (e.target.checked) {
                    this.selectedMembers.add({ id: userId, name: userName });
                } else {
                    // Manual delete since object references differ
                    for (let item of this.selectedMembers) {
                        if (item.id === userId) {
                            this.selectedMembers.delete(item);
                            break;
                        }
                    }
                }
            });
        });
    }

    async fetchGroupParticipants() {
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: `
                        query GetParticipants($conId: UUID!) {
                            groupParticipants(conversationId: $conId) {
                                data {
                                    id
                                    username
                                    email
                                }
                            }
                        }
                    `,
                    variables: { conId: this.selectedGroup.id }
                })
            });

            const result = await response.json();
            this.groupParticipants = result.data?.groupParticipants?.data || [];
        } catch (error) {
            console.error('Error fetching group participants:', error);
            this.groupParticipants = [];
        }
    }

    filterUsers(searchTerm) {
        const term = searchTerm.toLowerCase();
        const filtered = this.allUsers.filter(user =>
            (user.username && user.username.toLowerCase().includes(term)) ||
            (user.email && user.email.toLowerCase().includes(term))
        );
        this.renderUsersList(filtered);
    }

    async handleAddMembers() {
        if (this.selectedMembers.size === 0) {
            alert('Please select at least one user');
            return;
        }

        this.addSelectedBtn.disabled = true;
        this.addSelectedBtn.textContent = 'Adding...';

        try {
            const memberIds = Array.from(this.selectedMembers).map(m => m.id);
            let lastMessage = "";

            for (const userId of memberIds) {
                const response = await fetch('/graphql', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        query: `
                            mutation AddUserToGroupChat($conversationId: UUID!, $newUserId: UUID!) {
                                addUserToGroupChat(conversationId: $conversationId, newUserId: $newUserId) {
                                    message
                                    statusCode
                                }
                            }
                        `,
                        variables: {
                            conversationId: this.selectedGroup.id,
                            newUserId: userId
                        }
                    })
                });

                const result = await response.json();
                lastMessage = result.data?.addUserToGroupChat?.message;
            }

            alert(lastMessage || 'Members added successfully');
            this.closeAddMembersModal();
        } catch (error) {
            console.error('Error:', error);
            alert('Error adding members');
        } finally {
            this.addSelectedBtn.disabled = false;
            this.addSelectedBtn.textContent = 'Add Selected';
        }
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
}

document.addEventListener('DOMContentLoaded', () => {
    new GroupChat();
});