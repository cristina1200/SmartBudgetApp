document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("admin-statistics");
    renderTopbar("Statistics");

    if (getRole() !== "Admin") {
        showToast("Access denied. Admin only.", "error");
        window.location.href = "../Dashboard/dashboard.html";
        return;
    }

    await loadStatistics();
});

async function loadStatistics() {
    try {
        const users = await apiRequest("/Users");
        const transactions = await apiRequest("/Transactions");
        const budgets = await apiRequest("/Budgets");
        const savingGoals = await apiRequest("/SavingGoals");

        const totalIncome = transactions
            .filter(t => transactionTypeText(t.type) === "Income")
            .reduce((sum, item) => sum + Number(item.amount || 0), 0);

        const totalExpenses = transactions
            .filter(t => transactionTypeText(t.type) === "Expense")
            .reduce((sum, item) => sum + Number(item.amount || 0), 0);

        const totalBalance = totalIncome - totalExpenses;

        document.getElementById("usersCount").textContent = users.length || 0;
        document.getElementById("transactionsCount").textContent = transactions.length || 0;
        document.getElementById("budgetsCount").textContent = budgets.length || 0;
        document.getElementById("savingGoalsCount").textContent = savingGoals.length || 0;

        document.getElementById("totalIncome").textContent = formatMoney(totalIncome);
        document.getElementById("totalExpenses").textContent = formatMoney(totalExpenses);
        document.getElementById("totalBalance").textContent = formatMoney(totalBalance);
    } catch (error) {
        console.error(error);
        showToast("Statistics could not be loaded.", "error");
    }
}