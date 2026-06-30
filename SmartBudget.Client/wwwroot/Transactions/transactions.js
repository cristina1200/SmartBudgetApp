let categories = [];
let lastReceiptScan = null;

document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("transactions");
    renderTopbar("Transactions");

    document.getElementById("date").value = todayIso();

    await loadCategories();
    await loadTransactions();

    document.getElementById("type").addEventListener("change", renderCategoryOptions);
    document.getElementById("transactionForm").addEventListener("submit", createTransaction);
    document.getElementById("receiptScanForm").addEventListener("submit", scanReceipt);
    document.getElementById("saveReceiptExpenseBtn").addEventListener("click", saveReceiptExpense);
});

async function loadCategories() {
    categories = await apiRequest(`/Categories/user/${getUserId()}`);

    renderCategoryOptions();
    renderReceiptCategoryOptions();
}

function renderCategoryOptions() {
    const select = document.getElementById("categoryId");
    const type = Number(document.getElementById("type").value);

    const filtered = categories.filter(c => transactionTypeValue(c.type) === type);

    select.innerHTML = filtered.length
        ? filtered.map(c => `<option value="${c.id}">${safeText(c.name)}</option>`).join("")
        : `<option value="">Create a category first</option>`;
}

function renderReceiptCategoryOptions() {
    const select = document.getElementById("receiptCategoryId");

    const expenseCategories = categories.filter(c => transactionTypeValue(c.type) === 2);

    select.innerHTML = expenseCategories.length
        ? expenseCategories.map(c => `<option value="${c.id}">${safeText(c.name)}</option>`).join("")
        : `<option value="">Create an expense category first</option>`;
}

async function loadTransactions() {
    const table = document.getElementById("transactionsTable");

    try {
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
                </tr>`;
            return;
        }

        table.innerHTML = transactions.slice().reverse().map(item => {
            const type = transactionTypeText(item.type);

            return `
                <tr>
                    <td>${formatDate(item.date)}</td>
                    <td>${safeText(item.description)}</td>
                    <td>${safeText(item.categoryName)}</td>
                    <td>
                        <span class="badge ${type === "Income" ? "badge-income" : "badge-expense"}">
                            ${type}
                        </span>
                    </td>
                    <td>${formatMoney(item.amount)}</td>
                    <td>
                        <button class="icon-btn danger" onclick="deleteTransaction(${item.id})">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </td>
                </tr>`;
        }).join("");
    } catch (error) {
        showToast("Transactions could not be loaded.", "error");
    }
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
        showMessage(
            "transactionMessage",
            error.message || "Transaction could not be saved.",
            "error"
        );
    }
}

async function scanReceipt(event) {
    event.preventDefault();

    const fileInput = document.getElementById("receiptFile");
    const file = fileInput.files[0];

    if (!file) {
        showMessage("receiptScanMessage", "Choose a receipt image first.", "error");
        return;
    }

    const formData = new FormData();
    formData.append("file", file);

    try {
        showMessage("receiptScanMessage", "Scanning receipt...", "success");

        const result = await apiUpload("/Receipts/scan", formData);

        lastReceiptScan = result;

        document.getElementById("receiptPreview").classList.remove("hidden");

        document.getElementById("receiptAmount").value =
            result.suggestedAmount ?? "";

        document.getElementById("receiptDate").value =
            result.suggestedDate
                ? result.suggestedDate.split("T")[0]
                : todayIso();

        document.getElementById("receiptDescription").value =
            result.suggestedDescription || "Receipt expense";

        document.getElementById("receiptRawText").value =
            result.rawText || "";

        showMessage(
            "receiptScanMessage",
            result.message || "Receipt scanned successfully.",
            "success"
        );
    } catch (error) {
        showMessage(
            "receiptScanMessage",
            error.message || "Receipt could not be scanned.",
            "error"
        );
    }
}

async function saveReceiptExpense() {
    const amount = Number(document.getElementById("receiptAmount").value);
    const date = document.getElementById("receiptDate").value;
    const categoryId = Number(document.getElementById("receiptCategoryId").value);
    const description = document.getElementById("receiptDescription").value.trim();

    if (amount <= 0) {
        showToast("Enter a valid amount.", "error");
        return;
    }

    if (!categoryId) {
        showToast("Choose an expense category.", "error");
        return;
    }

    try {
        await apiRequest("/Receipts/confirm", "POST", {
            amount,
            date,
            description,
            userId: getUserId(),
            categoryId
        });

        showToast("Receipt saved as expense.");

        document.getElementById("receiptScanForm").reset();
        document.getElementById("receiptPreview").classList.add("hidden");

        lastReceiptScan = null;

        await loadTransactions();
    } catch (error) {
        showToast(error.message || "Expense could not be saved.", "error");
    }
}

async function deleteTransaction(id) {
    if (!confirm("Delete this transaction?")) {
        return;
    }

    await apiRequest(`/Transactions/${id}`, "DELETE");

    showToast("Transaction deleted.");

    await loadTransactions();
}