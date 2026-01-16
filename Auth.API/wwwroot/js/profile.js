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
        // Inside bindUpdateUser()
        saveBtn?.addEventListener('click', () => {
            const uiMessage = document.getElementById('uiMessage');
            const uiText = document.getElementById('uiMessageText');
            const uiIcon = document.getElementById('uiMessageIcon');

            const formData = {
                firstName: document.getElementById('firstName').value,
                lastName: document.getElementById('lastName').value,
                bio: document.getElementById('bio').value,
                preferences: {
                    emailNotification: document.getElementById('emailNotification').checked,
                    profileVisibility: document.getElementById('profileVisibility').value
                }
            };

            saveBtn.disabled = true;
            saveBtn.textContent = 'Saving...';

            fetch('?handler=UpdateProfile', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(formData)
            })
                .then(async res => {
                    let result;
                    try {
                        result = await res.json();
                        console.log("Full Result from Server:", result);
                    } catch (e) {
                    
                        result = { errors: [{ message: "Server returned an invalid response." }] };
                    }

                    uiMessage.classList.remove('hidden', 'bg-green-50', 'text-green-700', 'border-green-200', 'bg-red-50', 'text-red-700', 'border-red-200');

                    // Check specifically for GraphQL structure: data.updateUserDetails.statusCode
                    const gqlData = result?.data?.updateUserDetails;

                    if (res.ok && !result.errors && (gqlData?.statusCode === 200 || gqlData?.statusCode === 201)) {
                        uiMessage.classList.add('bg-green-50', 'text-green-700', 'border-green-200');
                        uiMessage.classList.remove('hidden');
                        uiText.textContent = gqlData?.message || "Profile updated successfully!";
                        uiIcon.innerHTML = '✅';
                        setTimeout(() => window.location.reload(), 1500);
                    } else {
                        uiMessage.classList.add('bg-red-50', 'text-red-700', 'border-red-200');
                        uiMessage.classList.remove('hidden');
                        uiText.textContent = result.errors ? result.errors[0].message : (gqlData?.message || "Failed to update.");
                        uiIcon.innerHTML = '❌';
                        saveBtn.disabled = false;
                        saveBtn.textContent = 'Save Changes';
                    }
                })
                .catch(err => {
                    console.error(err);
                    saveBtn.disabled = false;
                    saveBtn.textContent = 'Save Changes';
                });
        });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new Profile();
});
