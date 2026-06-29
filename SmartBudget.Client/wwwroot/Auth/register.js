function getTheme() {
    return localStorage.getItem("theme") || "dark";
}

function applyAuthTheme() {
    const theme = getTheme();
    const isLight = theme === "light";

    if (isLight) {
        document.body.classList.add("light-mode");
    } else {
        document.body.classList.remove("light-mode");
    }

    const themeIcon = document.getElementById("themeIcon");

    if (themeIcon) {
        themeIcon.className = isLight
            ? "fa-solid fa-sun"
            : "fa-solid fa-moon";
    }

    const logo = document.querySelector(".auth-logo-text img");

    if (logo) {
        logo.src = isLight
            ? "../Assets/logos/logo-light.png"
            : "../Assets/logos/logo-dark.png";
    }

    const heroImg = document.querySelector(".hero-illustration img");

    if (heroImg) {
        heroImg.src = isLight
            ? "../Assets/illustrations/register.png"
            : "../Assets/illustrations/register-dark.png";
    }
}

function toggleAuthTheme() {
    const nextTheme = getTheme() === "dark" ? "light" : "dark";
    localStorage.setItem("theme", nextTheme);
    applyAuthTheme();
}

function setupPasswordToggle() {
    const togglePassword = document.getElementById("togglePassword");
    const passwordInput = document.getElementById("password");

    if (!togglePassword || !passwordInput) return;

    togglePassword.addEventListener("click", () => {
        const isPassword = passwordInput.type === "password";

        passwordInput.type = isPassword ? "text" : "password";

        togglePassword.innerHTML = isPassword
            ? `<i class="fa-solid fa-eye-slash"></i>`
            : `<i class="fa-solid fa-eye"></i>`;
    });
}

document.addEventListener("DOMContentLoaded", () => {
    applyAuthTheme();

    const themeToggle = document.getElementById("themeToggle");

    if (themeToggle) {
        themeToggle.addEventListener("click", toggleAuthTheme);
    }

    setupPasswordToggle();

    const form = document.getElementById("registerForm");

    if (!form) return;

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const dto = {
            firstName: document.getElementById("firstName").value.trim(),
            lastName: document.getElementById("lastName").value.trim(),
            email: document.getElementById("email").value.trim(),
            password: document.getElementById("password").value
        };

        try {
            await apiRequest("/Auth/register", "POST", dto);

            showMessage(
                "registerMessage",
                "Account created successfully. Redirecting to login...",
                "success"
            );

            setTimeout(() => {
                window.location.href = "./login.html";
            }, 1200);
        } catch (error) {
            showMessage(
                "registerMessage",
                error.message || "Registration failed.",
                "error"
            );
        }
    });
});