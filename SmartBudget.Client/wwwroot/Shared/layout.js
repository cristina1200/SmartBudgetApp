function getTheme() {
    return localStorage.getItem("theme") || "dark";
}

function isLightTheme() {
    return getTheme() === "light";
}

function applyTheme() {
    if (isLightTheme()) {
        document.body.classList.add("light-mode");
    } else {
        document.body.classList.remove("light-mode");
    }

    updateThemeAssets();
}

function toggleTheme() {
    const nextTheme = isLightTheme() ? "dark" : "light";
    localStorage.setItem("theme", nextTheme);

    applyTheme();

    const activePage = document.body.dataset.page;
    const pageTitle = document.body.dataset.title;

    if (document.getElementById("sidebar") && activePage) {
        renderSidebar(activePage);
    }

    if (document.getElementById("topbar") && pageTitle) {
        renderTopbar(pageTitle);
    }

    updateThemeAssets();

    document.dispatchEvent(new CustomEvent("themeChanged", {
        detail: {
            theme: nextTheme
        }
    }));
}

function currentLogoPath() {
    return isLightTheme()
        ? "../Assets/logos/logo-light.png"
        : "../Assets/logos/logo-dark.png";
}

function currentAvatarPath() {
    return isLightTheme()
        ? "../Assets/profile/default-avatar.png"
        : "../Assets/profile/default-avatar-dark.png";
}

function currentIllustration(name) {
    const suffix = isLightTheme() ? "" : "-dark";

    const paths = {
        login: `../Assets/illustrations/login${suffix}.png`,
        register: `../Assets/illustrations/register${suffix}.png`,
        transactions: `../Assets/illustrations/empty-transactions${suffix}.png`,
        savings: `../Assets/illustrations/empty-savings${suffix}.png`,
        noData: `../Assets/illustrations/no-data${suffix}.png`
    };

    return paths[name] || paths.noData;
}

function themeImageHtml(name, altText) {
    const lightPath = {
        login: "../Assets/illustrations/login.png",
        register: "../Assets/illustrations/register.png",
        transactions: "../Assets/illustrations/empty-transactions.png",
        savings: "../Assets/illustrations/empty-savings.png",
        noData: "../Assets/illustrations/no-data.png"
    };

    const darkPath = {
        login: "../Assets/illustrations/login-dark.png",
        register: "../Assets/illustrations/register-dark.png",
        transactions: "../Assets/illustrations/empty-transactions-dark.png",
        savings: "../Assets/illustrations/empty-savings-dark.png",
        noData: "../Assets/illustrations/no-data-dark.png"
    };

    const src = isLightTheme() ? lightPath[name] : darkPath[name];

    return `
        <img 
            class="empty-visual theme-image"
            src="${src}"
            data-light-src="${lightPath[name]}"
            data-dark-src="${darkPath[name]}"
            alt="${safeText(altText)}"
        >`;
}

function updateThemeAssets() {
    const themeIcon = document.getElementById("themeIcon");

    if (themeIcon) {
        themeIcon.className = isLightTheme()
            ? "fa-solid fa-sun"
            : "fa-solid fa-moon";
    }

    document.querySelectorAll(".brand-logo").forEach(logo => {
        logo.src = currentLogoPath();
    });

    document.querySelectorAll(".user-avatar").forEach(avatar => {
        avatar.src = currentAvatarPath();
    });

    document.querySelectorAll("[data-light-src][data-dark-src]").forEach(image => {
        image.src = isLightTheme()
            ? image.dataset.lightSrc
            : image.dataset.darkSrc;
    });
}

function renderSidebar(activePage) {
    applyTheme();

    const sidebar = document.getElementById("sidebar");

    if (!sidebar) return;

    const adminLinks = getRole() === "Admin"
        ? `
            <a class="${activePage === "admin-users" ? "active" : ""}" href="../Admin/users.html">
                <i class="fa-solid fa-users-gear"></i>
                Admin Users
            </a>

            <a class="${activePage === "admin-statistics" ? "active" : ""}" href="../Admin/statistics.html">
                <i class="fa-solid fa-chart-pie"></i>
                Statistics
            </a>`
        : "";

    sidebar.innerHTML = `
        <div class="brand">
            <img 
                class="brand-logo"
                src="${currentLogoPath()}"
                data-light-src="../Assets/logos/logo-light.png"
                data-dark-src="../Assets/logos/logo-dark.png"
                alt="SmartBudget logo"
            >

            <span>SmartBudget</span>
        </div>

        <nav class="nav-menu">
            <a class="${activePage === "dashboard" ? "active" : ""}" href="../Dashboard/dashboard.html">
                <i class="fa-solid fa-chart-line"></i>
                Dashboard
            </a>

            <a class="${activePage === "transactions" ? "active" : ""}" href="../Transactions/transactions.html">
                <i class="fa-solid fa-money-bill-transfer"></i>
                Transactions
            </a>

            <a class="${activePage === "budgets" ? "active" : ""}" href="../Budgets/budgets.html">
                <i class="fa-solid fa-wallet"></i>
                Budgets
            </a>

            <a class="${activePage === "saving-goals" ? "active" : ""}" href="../SavingGoals/saving-goals.html">
                <i class="fa-solid fa-piggy-bank"></i>
                Saving Goals
            </a>

            <a class="${activePage === "categories" ? "active" : ""}" href="../Categories/categories.html">
                <i class="fa-solid fa-tags"></i>
                Categories
            </a>

            <a class="${activePage === "profile" ? "active" : ""}" href="../Profile/profile.html">
                <i class="fa-solid fa-user"></i>
                Profile
            </a>

            ${adminLinks}
        </nav>

        <button class="logout-btn" onclick="logout()">
            <i class="fa-solid fa-right-from-bracket"></i>
            Logout
        </button>
    `;

    updateThemeAssets();
}

function renderTopbar(title) {
    applyTheme();

    const topbar = document.getElementById("topbar");

    if (!topbar) return;

    const themeIconClass = isLightTheme()
        ? "fa-solid fa-sun"
        : "fa-solid fa-moon";

    topbar.innerHTML = `
        <div>
            <h1>${safeText(title)}</h1>
            <p>Welcome back, ${safeText(getFullName())}</p>
        </div>

        <div class="user-pill">
            <button class="theme-toggle" onclick="toggleTheme()" title="Change theme">
                <i id="themeIcon" class="${themeIconClass}"></i>
            </button>

            <img 
                class="user-avatar"
                src="${currentAvatarPath()}"
                data-light-src="../Assets/profile/default-avatar.png"
                data-dark-src="../Assets/profile/default-avatar-dark.png"
                alt="User avatar"
            >

            <div>
                <strong>${safeText(getFullName())}</strong>
                <span>${safeText(getEmail())}</span>
            </div>
        </div>
    `;

    updateThemeAssets();
}

document.addEventListener("DOMContentLoaded", () => {
    applyTheme();
});