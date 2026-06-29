document.addEventListener("DOMContentLoaded", async () => {
    requireAdmin();
    renderSidebar("admin-statistics");
    renderTopbar("Statistics");
    await loadStatistics();
});

async function loadStatistics() {
    try {
        const [users, transactions, budgets, goals] = await Promise.all([
            apiRequest("/Users"),
            apiRequest("/Transactions"),
            apiRequest("/Budgets"),
            apiRequest("/SavingGoals")
        ]);

        document.getElementById("usersCount").textContent = users.length;
        document.getElementById("transactionsCount").textContent = transactions.length;
        document.getElementById("budgetsCount").textContent = budgets.length;
        document.getElementById("goalsCount").textContent = goals.length;

        const income = transactions.filter(t => transactionTypeText(t.type) === "Income").reduce((sum, t) => sum + Number(t.amount || 0), 0);
        const expenses = transactions.filter(t => transactionTypeText(t.type) === "Expense").reduce((sum, t) => sum + Number(t.amount || 0), 0);
        document.getElementById("systemTotals").textContent = `Total income: ${formatMoney(income)} | Total expenses: ${formatMoney(expenses)} | Balance: ${formatMoney(income - expenses)}`;
    } catch (error) {
        showToast("Statistics could not be loaded. Check admin permissions.", "error");
    }
}
