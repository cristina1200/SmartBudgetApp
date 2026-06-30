document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("admin-users");
    renderTopbar("Admin Users");

    if (getRole() !== "Admin") {
        showToast("Access denied. Admin only.", "error");
        window.location.href = "../Dashboard/dashboard.html";
        return;
    }

    await loadUsers();
});

async function loadUsers() {
    const table = document.getElementById("usersTable");

    try {
        const users = await apiRequest("/Users");

        if (!users || users.length === 0) {
            table.innerHTML = `
                <tr>
                    <td colspan="5" class="empty-table">
                        No users found.
                    </td>
                </tr>`;
            return;
        }

        table.innerHTML = users.map(user => `
            <tr>
                <td>${user.id}</td>
                <td>${safeText(`${user.firstName || ""} ${user.lastName || ""}`.trim())}</td>
                <td>${safeText(user.email)}</td>
                <td>${formatDate(user.createdAt)}</td>
                <td>
                    <button class="icon-btn danger" onclick="deleteUser(${user.id})">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </td>
            </tr>
        `).join("");
    } catch (error) {
        console.error(error);

        table.innerHTML = `
            <tr>
                <td colspan="5" class="empty-table">
                    Users could not be loaded.
                </td>
            </tr>`;

        showToast("Users could not be loaded.", "error");
    }
}

async function deleteUser(id) {
    if (!confirm("Delete this user?")) {
        return;
    }

    try {
        await apiRequest(`/Users/${id}`, "DELETE");

        showToast("User deleted successfully.");

        await loadUsers();
    } catch (error) {
        showToast(error.message || "User could not be deleted.", "error");
    }
}