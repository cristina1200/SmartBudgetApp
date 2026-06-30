function isAuthenticated() {
    const token = localStorage.getItem("token");

    return token !== null && token !== undefined && token.trim() !== "";
}

function saveAuthData(data) {
    if (!data) {
        return;
    }

    const token = data.token || data.jwtToken || data.accessToken;

    if (token) {
        localStorage.setItem("token", token);
    }

    if (data.userId !== undefined && data.userId !== null) {
        localStorage.setItem("userId", data.userId);
    }

    if (data.id !== undefined && data.id !== null && !data.userId) {
        localStorage.setItem("userId", data.id);
    }

    const firstName = data.firstName || "";
    const lastName = data.lastName || "";
    const fullNameFromData = data.fullName || `${firstName} ${lastName}`.trim();

    if (fullNameFromData) {
        localStorage.setItem("fullName", fullNameFromData);
    }

    if (data.email) {
        localStorage.setItem("email", data.email);
    }

    if (data.role) {
        localStorage.setItem("role", data.role);
    } else {
        localStorage.setItem("role", "User");
    }
}

function clearAuthData() {
    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    localStorage.removeItem("fullName");
    localStorage.removeItem("email");
    localStorage.removeItem("role");
}

function requireAuth() {
    if (!isAuthenticated()) {
        window.location.href = "../Auth/login.html";
        return false;
    }

    return true;
}

function redirectIfAuthenticated() {
    if (isAuthenticated()) {
        window.location.href = "../Dashboard/dashboard.html";
    }
}

function logout() {
    clearAuthData();
    window.location.href = "../Auth/login.html";
}