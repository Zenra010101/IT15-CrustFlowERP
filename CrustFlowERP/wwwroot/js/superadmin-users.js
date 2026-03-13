// Super Admin Users Management JavaScript

// Search functionality
document.addEventListener('DOMContentLoaded', function () {
    const userSearch = document.getElementById('userSearch');
    const roleFilter = document.getElementById('roleFilter');
    const statusFilter = document.getElementById('statusFilter');

    if (userSearch) {
        userSearch.addEventListener('input', function (e) {
            const searchTerm = e.target.value.toLowerCase();
            const rows = document.querySelectorAll('tbody tr');

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(searchTerm) ? '' : 'none';
            });
        });
    }

    if (roleFilter) {
        roleFilter.addEventListener('change', function (e) {
            const roleFilterValue = e.target.value;
            const rows = document.querySelectorAll('tbody tr');

            rows.forEach(row => {
                if (roleFilterValue === '') {
                    row.style.display = '';
                } else {
                    const roleCell = row.querySelector('td:nth-child(2)');
                    const hasRole = roleCell && roleCell.textContent.includes(roleFilterValue);
                    row.style.display = hasRole ? '' : 'none';
                }
            });
        });
    }

    if (statusFilter) {
        statusFilter.addEventListener('change', function (e) {
            const statusFilterValue = e.target.value;
            const rows = document.querySelectorAll('tbody tr');

            rows.forEach(row => {
                if (statusFilterValue === '') {
                    row.style.display = '';
                } else {
                    const statusCell = row.querySelector('td:nth-child(3)');
                    const hasStatus = statusCell && statusCell.textContent.includes(statusFilterValue);
                    row.style.display = hasStatus ? '' : 'none';
                }
            });
        });
    }

    // Create user modal functionality
    const createUserModalButton = document.querySelector('[data-bs-target="#createUserModal"]');
    if (createUserModalButton) {
        createUserModalButton.addEventListener('click', function () {
            const form = document.querySelector('#createUserModal form');
            if (form) {
                form.reset();
            }
        });
    }

    // Edit user modal role change listener
    const editUserRoleSelect = document.getElementById('editUserRole');
    if (editUserRoleSelect) {
        editUserRoleSelect.addEventListener('change', function () {
            showRolePermissions(this.value);
        });
    }
});

// User management functions
function editUser(userId) {
    // Fetch user data and populate modal
    fetch('/SuperAdmin/GetUserById', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ userId: userId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Populate modal fields
                document.getElementById('editUserId').value = data.user.id;
                document.getElementById('editUserName').value = data.user.userName;
                document.getElementById('editUserEmail').value = data.user.email;
                document.getElementById('editUserRole').value = data.user.role;

                // Update status display
                const statusElement = document.getElementById('editUserStatus');
                const lockoutInfoElement = document.getElementById('editUserLockoutInfo');

                if (data.user.status === 'Deactivated') {
                    statusElement.className = 'badge bg-warning text-dark';
                    statusElement.innerHTML = '<i class="fas fa-ban me-1"></i>Deactivated';
                    lockoutInfoElement.textContent = '';
                } else if (data.user.status === 'Revoked') {
                    statusElement.className = 'badge bg-danger text-white';
                    statusElement.innerHTML = '<i class="fas fa-user-slash me-1"></i>Revoked';
                    lockoutInfoElement.textContent = '';
                } else if (data.user.isLockedOut) {
                    statusElement.className = 'badge bg-warning text-dark';
                    statusElement.innerHTML = '<i class="fas fa-lock me-1"></i>Locked';
                    if (data.user.lockoutEnd) {
                        lockoutInfoElement.textContent = `Locked until ${new Date(data.user.lockoutEnd).toLocaleString()}`;
                    }
                } else {
                    statusElement.className = 'badge bg-success text-white';
                    statusElement.innerHTML = '<i class="fas fa-check me-1"></i>Active';
                    lockoutInfoElement.textContent = '';
                }

                // Show role permissions
                showRolePermissions(data.user.role);

                // Open modal
                const modal = new bootstrap.Modal(document.getElementById('editUserModal'));
                modal.show();
            } else {
                alert('Error loading user data: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error loading user data');
        });
}

// Show role permissions in modal
function showRolePermissions(role) {
    // Hide all permission sections
    document.getElementById('adminPermissions').style.display = 'none';
    document.getElementById('managerPermissions').style.display = 'none';
    document.getElementById('cashierPermissions').style.display = 'none';

    // Show relevant permissions
    if (role === '1') {
        document.getElementById('adminPermissions').style.display = 'block';
    } else if (role === '11' || role === '2') {
        document.getElementById('managerPermissions').style.display = 'block';
    } else if (role === '3' || role === '4') {
        document.getElementById('productionPermissions').style.display = 'block';
    } else {
        document.getElementById('cashierPermissions').style.display = 'block';
    }
}

// Update user function
function updateUser() {
    const userId = document.getElementById('editUserId').value;
    const userName = document.getElementById('editUserName').value;
    const email = document.getElementById('editUserEmail').value;
    const role = document.getElementById('editUserRole').value;

    // Validation
    if (!userName.trim() || !email.trim() || !role) {
        alert('Please fill in all required fields.');
        return;
    }

    // Get anti-forgery token
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenElement) {
        console.error('Anti-forgery token not found');
        alert('Security token not found. Please refresh the page and try again.');
        return;
    }

    const userData = {
        Id: userId,
        UserName: userName,
        Email: email,
        Role: parseInt(role)
    };

    fetch('/SuperAdmin/EditUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': tokenElement.value
        },
        body: JSON.stringify(userData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
                modal.hide();

                // Show success message
                alert('User updated successfully!');

                // Reload page to show updated user data
                location.reload();
            } else {
                alert('Error updating user: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error updating user. Please try again.');
        });
}

function lockUser(userId) {
    if (confirm('Are you sure you want to lock this user?')) {
        fetch('/SuperAdmin/LockUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ userId: userId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    location.reload();
                } else {
                    alert('Error locking user: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error locking user');
            });
    }
}

function unlockUser(userId) {
    if (confirm('Are you sure you want to unlock this user?')) {
        fetch('/SuperAdmin/UnlockUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ userId: userId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    location.reload();
                } else {
                    alert('Error unlocking user: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error unlocking user');
            });
    }
}

function deactivateUser(userId) {
    if (confirm('Are you sure you want to DEACTIVATE this user? They will be blocked from logging in.')) {
        fetch('/SuperAdmin/DeactivateUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ userId: userId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    location.reload();
                } else {
                    alert('Error: ' + data.message);
                }
            });
    }
}

function revokeUser(userId) {
    if (confirm('Are you sure you want to REVOKE access for this user? This is a serious action.')) {
        fetch('/SuperAdmin/RevokeUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ userId: userId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    location.reload();
                } else {
                    alert('Error: ' + data.message);
                }
            });
    }
}

function activateUser(userId) {
    fetch('/SuperAdmin/ActivateUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ userId: userId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                location.reload();
            } else {
                alert('Error: ' + data.message);
            }
        });
}

function deleteUser(userId) {
    if (confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
        fetch('/SuperAdmin/DeleteUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ userId: userId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    location.reload();
                } else {
                    alert('Error deleting user: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error deleting user');
            });
    }
}

function resetPassword(userId) {
    if (confirm('Are you sure you want to reset this user\'s password?')) {
        fetch('/SuperAdmin/ResetPassword', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ userId: userId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Password reset email sent successfully!');
                } else {
                    alert('Error resetting password: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error resetting password');
            });
    }
}

function viewDetails(userId) {
    window.location.href = '/SuperAdmin/UserDetails/' + userId;
}

// Create Admin User function
function createAdminUser() {
    const email = document.getElementById('adminEmail').value;
    const password = document.getElementById('adminPassword').value;
    const confirmPassword = document.getElementById('adminConfirmPassword').value;
    const role = document.getElementById('adminRole').value;
    const sendEmail = document.getElementById('sendEmail').checked;

    // Validation
    if (!email || !password || !confirmPassword || !role) {
        alert('Please fill in all required fields.');
        return;
    }

    if (password !== confirmPassword) {
        alert('Passwords do not match.');
        return;
    }

    if (password.length < 6) {
        alert('Password must be at least 6 characters long.');
        return;
    }

    const adminData = {
        email: email,
        password: password,
        confirmPassword: confirmPassword,
        username: email, // Use email as username
        role: role,
        sendEmail: sendEmail
    };

    fetch('/SuperAdmin/CreateAdmin', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(adminData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('createUserModal'));
                modal.hide();

                // Reset form
                document.getElementById('createAdminForm').reset();

                // Show success message
                alert('Admin user created successfully!');

                // Reload page to show new user
                location.reload();
            } else {
                alert('Error creating admin user: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error creating admin user. Please try again.');
        });
}

// Logout function
function logoutUser() {
    console.log('Attempting logout...');

    // Method 1: Try to submit the hidden form first
    const logoutForm = document.getElementById('logoutForm');
    if (logoutForm) {
        try {
            logoutForm.submit();
            console.log('Logout form submitted successfully');
            return;
        } catch (error) {
            console.error('Error submitting logout form:', error);
        }
    }

    // Method 2: Direct fetch to AccountController Logout
    fetch('/Account/Logout', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        }
    })
        .then(response => {
            console.log('Logout response:', response);
            // Always redirect to login page, regardless of response
            window.location.href = '/Account/Login';
        })
        .catch(error => {
            console.error('Error during logout:', error);
            // Even if there's an error, redirect to login page
            window.location.href = '/Account/Login';
        });
}
