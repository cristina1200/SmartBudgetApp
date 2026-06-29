document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();
    renderSidebar("budgets");
    renderTopbar("Budgets");

    const now = new Date();
    document.getElementById("month").value = now.getMonth() + 1;
    document.getElementById("year").value = now.getFullYear();

    await loadBudgets();
    document.getElementById("budgetForm").addEventListener("submit", saveBudget);
});

async function loadBudgets() {
    const table = document.getElementById("budgetsTable");
    const budgets = await apiRequest(`/Budgets/user/${getUserId()}`);

    if (budgets.length === 0) {
        table.innerHTML = `
    <tr>
        <td colspan="4" class="empty-table">
            <div class="empty-state-box">
                <img class="empty-visual" src="${currentIllustration("noData")}" alt="No budgets">
                <h3>No budgets yet</h3>
                <p>Create your first monthly budget.</p>
            </div>
        </td>
    </tr>`;        return;
    }

    table.innerHTML = budgets.slice().reverse().map(b => `<tr><td>${b.month}</td><td>${b.year}</td><td>${formatMoney(b.monthlyLimit)}</td><td><button class="icon-btn" onclick='editBudget(${JSON.stringify(b)})'><i class="fa-solid fa-pen"></i></button> <button class="icon-btn danger" onclick="deleteBudget(${b.id})"><i class="fa-solid fa-trash"></i></button></td></tr>`).join("");
}

async function saveBudget(event) {
    event.preventDefault();
    const id = document.getElementById("budgetId").value;
    const dto = { monthlyLimit: Number(document.getElementById("monthlyLimit").value), month: Number(document.getElementById("month").value), year: Number(document.getElementById("year").value) };

    try {
        if (id) {
            await apiRequest(`/Budgets/${id}`, "PUT", dto);
            showMessage("budgetMessage", "Budget updated successfully.", "success");
        } else {
            await apiRequest("/Budgets", "POST", { ...dto, userId: getUserId() });
            showMessage("budgetMessage", "Budget added successfully.", "success");
        }
        event.target.reset();
        document.getElementById("budgetId").value = "";
        await loadBudgets();
    } catch (error) {
        showMessage("budgetMessage", error.message || "Budget could not be saved.", "error");
    }
}

function editBudget(budget) {
    document.getElementById("budgetId").value = budget.id;
    document.getElementById("monthlyLimit").value = budget.monthlyLimit;
    document.getElementById("month").value = budget.month;
    document.getElementById("year").value = budget.year;
    window.scrollTo({ top: 0, behavior: "smooth" });
}

async function deleteBudget(id) {
    if (!confirm("Delete this budget?")) return;
    await apiRequest(`/Budgets/${id}`, "DELETE");
    showToast("Budget deleted.");
    await loadBudgets();
}
