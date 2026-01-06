class Profile {
    constructor() {
        this.bindUpdateUser();
        this.bindDeleteUser();
    }

    bindDeleteUser() {
        const deleteBtn = document.getElementById('deleteAccountBtn');
        if (!deleteBtn) return;

        deleteBtn.addEventListener('click', () => {
            if (!confirm('Are you sure you want to delete your account?')) return;

            fetch('?handler=DeleteUser', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken':
                        document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        alert('Account deleted');
                        window.location.href = '/Auth/Login';
                    } else {
                        alert(data.message || 'Failed to delete account');
                    }
                })
                .catch(() => alert('Something went wrong'));
        });
    }

    bindUpdateUser() {
        const editProfileBtn = document.getElementById('editProfileBtn');
        const displayView = document.getElementById('displayView');
        const editView = document.getElementById('editView');
        const cancelBtn = document.getElementById('cancelBtn');
        const saveBtn = document.getElementById('saveBtn');
        const bioTextarea = document.getElementById('bio');
        const charCount = document.getElementById('charCount');

        // Character counter
        if (bioTextarea && charCount) {
            charCount.textContent = bioTextarea.value.length;
            bioTextarea.addEventListener('input', () => {
                charCount.textContent = bioTextarea.value.length;
            });
        }

        // Show edit form
        editProfileBtn?.addEventListener('click', () => {
            displayView.style.display = 'none';
            editView.style.display = 'block';
        });

        // Cancel editing
        cancelBtn?.addEventListener('click', () => {
            displayView.style.display = 'block';
            editView.style.display = 'none';
            location.reload();
        });

        // Save changes
        saveBtn?.addEventListener('click', () => {

            const formData = {
                firstName: document.getElementById('firstName').value,
                lastName: document.getElementById('lastName').value,
                bio: document.getElementById('bio').value,
                preferences: {
                    emailNotification: document.getElementById('emailNotification').checked,
                    profileVisibility: document.getElementById('profileVisibility').value
                }
            };
            fetch('?handler=UpdateProfile', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken':
                        document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(formData)
            })
                .then(async res => {
                    const data = await res.json().catch(() => null);
               
                    window.location.reload();
                })
                .catch(err => console.error("Fetch error:", err));
        });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new Profile();
});
