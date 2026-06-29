document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();
    renderSidebar("categories");
    renderTopbar("Categories");
    await loadCategories();
    document.getElementById("categoryForm").addEventListener("submit", createCategory);
});

function categoryIconPath(name) {
    const value = String(name || "").toLowerCase();

    if (value.includes("food") || value.includes("mancare") || value.includes("restaurant")) {
        return "../Assets/categories/food.png";
    }

    if (value.includes("shopping") || value.includes("cumparaturi")) {
        return "../Assets/categories/shopping.png";
    }

    if (value.includes("transport") || value.includes("car") || value.includes("benzina")) {
        return "../Assets/categories/transport.png";
    }

    if (value.includes("entertainment") || value.includes("movie") || value.includes("film")) {
        return "../Assets/categories/entertainment.png";
    }

    if (value.includes("health") || value.includes("medical") || value.includes("sanatate")) {
        return "../Assets/categories/health.png";
    }

    if (value.includes("education") || value.includes("scoala") || value.includes("facultate")) {
        return "../Assets/categories/education.png";
    }

    if (value.includes("salary") || value.includes("salariu") || value.includes("income")) {
        return "../Assets/categories/salary.png";
    }

    if (value.includes("gift") || value.includes("cadou")) {
        return "../Assets/categories/gifts.png";
    }

    if (value.includes("bill") || value.includes("factura") || value.includes("utilitati")) {
        return "../Assets/categories/bills.png";
    }

    if (value.includes("travel") || value.includes("vacanta") || value.includes("trip")) {
        return "../Assets/categories/travel.png";
    }

    return "../Assets/money/wallet.png";
}

async function loadCategories() {
    const container = document.getElementById("categoriesList");
    const categories = await apiRequest(`/Categories/user/${getUserId()}`);

    if (categories.length === 0) {
        container.innerHTML = `
    <div class="empty-state-box">
        <img class="empty-visual" src="${currentIllustration("noData")}" alt="No categories">
        <h3>No categories yet</h3>
        <p>Create categories for your income and expenses.</p>
    </div>`;
        return;
    }

    container.innerHTML = categories.map(c => {
        const type = transactionTypeText(c.type);
        return `
    <div class="category-item">
        <div style="display:flex; align-items:center;">
            <img class="category-icon" src="${categoryIconPath(c.name)}" alt="${safeText(c.name)}">
            <div>
                <strong>${safeText(c.name)}</strong>
                <span>${type}</span>
            </div>
        </div>

        <button class="icon-btn danger" onclick="deleteCategory(${c.id})">
            <i class="fa-solid fa-trash"></i>
        </button>
    </div>`;
    }).join("");
}

async function createCategory(event) {
    event.preventDefault();
    const dto = { name: document.getElementById("name").value.trim(), type: Number(document.getElementById("type").value), userId: getUserId() };
    try {
        await apiRequest("/Categories", "POST", dto);
        showMessage("categoryMessage", "Category added successfully.", "success");
        event.target.reset();
        await loadCategories();
    } catch (error) {
        showMessage("categoryMessage", error.message || "Category could not be saved.", "error");
    }
}

async function deleteCategory(id) {
    if (!confirm("Delete this category?")) return;
    await apiRequest(`/Categories/${id}`, "DELETE");
    showToast("Category deleted.");
    await loadCategories();
}
