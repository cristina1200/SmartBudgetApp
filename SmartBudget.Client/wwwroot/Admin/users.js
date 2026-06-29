document.addEventListener("DOMContentLoaded", async () => {
    requireAdmin();
    renderSidebar("admin-users");
    renderTopbar("Admin Users");
    await loadUsers();
});

async function loadUsers() {
    const table = document.getElementById("usersTable");
    const users = await apiRequest("/Users");

    if (users.length === 0) {
        table.innerHTML = `<tr><td colspan="5" class="empty-table">No users found.</td></tr>`;
        return;
    }

    table.innerHTML = users.map(user => `<tr><td>${user.id}</td><td>${safeText(user.firstName)} ${safeText(user.lastName)}</td><td>${safeText(user.email)}</td><td>${formatDate(user.createdAt)}</td><td><button class="icon-btn danger" onclick="deleteUser(${user.id})"><i class="fa-solid fa-trash"></i></button></td></tr>`).join("");
}

async function deleteUser(id) {
    if (id === getUserId()) return showToast("You cannot delete your own account while logged in.", "error");
    if (!confirm("Delete this user?")) return;
    await apiRequest(`/Users/${id}`, "DELETE");
    showToast("User deleted.");
    await loadUsers();
}
