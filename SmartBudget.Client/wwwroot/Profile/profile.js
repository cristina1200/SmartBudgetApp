document.addEventListener("DOMContentLoaded", () => {
    requireAuth();
    renderSidebar("profile");
    renderTopbar("Profile");
    document.getElementById("profileName").textContent = getFullName();
    document.getElementById("profileEmail").textContent = getEmail();
    document.getElementById("profileRole").textContent = getRole();
});
