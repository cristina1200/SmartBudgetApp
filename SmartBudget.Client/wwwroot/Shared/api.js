function getToken() {
    return localStorage.getItem("token");
}

function getUserId() {
    return Number(localStorage.getItem("userId"));
}

function getFullName() {
    return localStorage.getItem("fullName") || "User";
}

function getEmail() {
    return localStorage.getItem("email") || "";
}

function getRole() {
    return localStorage.getItem("role") || "User";
}

async function apiRequest(endpoint, method = "GET", body = null) {
    const headers = {
        "Content-Type": "application/json"
    };

    const token = getToken();

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    const options = {
        method,
        headers
    };

    if (body !== null) {
        options.body = JSON.stringify(body);
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, options);

    if (response.status === 401 || response.status === 403) {
        if (endpoint !== "/Auth/login") {
            logout();
        }

        throw new Error("Unauthorized request.");
    }

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Request failed.");
    }

    if (response.status === 204) {
        return null;
    }

    return await response.json();
}

async function apiUpload(endpoint, formData, method = "POST") {
    const headers = {};

    const token = getToken();

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method,
        headers,
        body: formData
    });

    if (response.status === 401 || response.status === 403) {
        logout();
        throw new Error("Unauthorized request.");
    }

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Upload failed.");
    }

    if (response.status === 204) {
        return null;
    }

    return await response.json();
}