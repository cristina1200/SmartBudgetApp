let categories = [];

document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();
    renderSidebar("transactions");
    renderTopbar("Transactions");
    document.getElementById("date").value = todayIso();

    await loadCategories();
    await loadTransactions();

    document.getElementById("type").addEventListener("change", renderCategoryOptions);
    document.getElementById("transactionForm").addEventListener("submit", createTransaction);
});

async function loadCategories() {
    categories = await apiRequest(`/Categories/user/${getUserId()}`);
    renderCategoryOptions();
}

function renderCategoryOptions() {
    const select = document.getElementById("categoryId");
    const type = Number(document.getElementById("type").value);
    const filtered = categories.filter(c => transactionTypeValue(c.type) === type);

    select.innerHTML = filtered.length
        ? filtered.map(c => `<option value="${c.id}">${safeText(c.name)}</option>`).join("")
        : `<option value="">Create a category first</option>`;
}

async function loadTransactions() {
    const table = document.getElementById("transactionsTable");
    const transactions = await apiRequest(`/Transactions/user/${getUserId()}`);

    if (transactions.length === 0) {
        table.innerHTML = `
    <tr>
        <td colspan="6" class="empty-table">
            <div class="empty-state-box">
                <img class="empty-visual" src="${currentIllustration("transactions")}" alt="No transactions">
                <h3>No transactions yet</h3>
                <p>Add your first transaction to get started.</p>
            </div>
        </td>
    </tr>`;        return;
    }

    table.innerHTML = transactions.slice().reverse().map(item => {
        const type = transactionTypeText(item.type);
        return `<tr>
            <td>${formatDate(item.date)}</td>
            <td>${safeText(item.description)}</td>
            <td>${safeText(item.categoryName)}</td>
            <td><span class="badge ${type === "Income" ? "badge-income" : "badge-expense"}">${type}</span></td>
            <td>${formatMoney(item.amount)}</td>
            <td><button class="icon-btn danger" onclick="deleteTransaction(${item.id})"><i class="fa-solid fa-trash"></i></button></td>
        </tr>`;
    }).join("");
}

async function createTransaction(event) {
    event.preventDefault();

    const dto = {
        amount: Number(document.getElementById("amount").value),
        description: document.getElementById("description").value.trim(),
        date: document.getElementById("date").value,
        type: Number(document.getElementById("type").value),
        userId: getUserId(),
        categoryId: Number(document.getElementById("categoryId").value)
    };

    try {
        await apiRequest("/Transactions", "POST", dto);
        showMessage("transactionMessage", "Transaction added successfully.", "success");
        event.target.reset();
        document.getElementById("date").value = todayIso();
        await loadTransactions();
    } catch (error) {
        showMessage("transactionMessage", error.message || "Transaction could not be saved.", "error");
    }
}

async function deleteTransaction(id) {
    if (!confirm("Delete this transaction?")) return;
    await apiRequest(`/Transactions/${id}`, "DELETE");
    showToast("Transaction deleted.");
    await loadTransactions();
}
