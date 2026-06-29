function saveAuthData(data) {
    localStorage.setItem("token", data.token);
    localStorage.setItem("userId", data.userId);
    localStorage.setItem("fullName", data.fullName);
    localStorage.setItem("email", data.email);

    const tokenRole = getRoleFromToken(data.token);
    localStorage.setItem("role", data.role || tokenRole || "User");
}

function isAuthenticated() {
    return !!localStorage.getItem("token");
}

function getRoleFromToken(token) {
    if (!token) return null;

    try {
        const payload = JSON.parse(atob(token.split(".")[1]));
        return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
            || payload.role
            || payload.Role
            || null;
    } catch {
        return null;
    }
}

function requireAdmin() {
    requireAuth();

    if (getRole() !== "Admin") {
        window.location.href = "../Dashboard/dashboard.html";
    }
}
