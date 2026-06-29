document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("dashboard");
    renderTopbar("Dashboard");

    await loadDashboard();
});

async function loadDashboard() {
    const userId = getUserId();

    try {
        const summary = await apiRequest(`/Dashboard/summary/${userId}`);

        document.getElementById("totalIncome").textContent = formatMoney(summary.totalIncome || 0);
        document.getElementById("totalExpenses").textContent = formatMoney(summary.totalExpenses || 0);
        document.getElementById("balance").textContent = formatMoney(summary.balance || 0);
        document.getElementById("transactionsCount").textContent = summary.transactionsCount || 0;

        await Promise.all([
            loadRecentTransactions(userId),
            loadGoalsPreview(userId)
        ]);
    } catch (error) {
        console.error(error);
        showToast("Dashboard could not be loaded.", "error");
    }
}

async function loadRecentTransactions(userId) {
    const container = document.getElementById("recentTransactions");

    try {
        const transactions = await apiRequest(`/Transactions/user/${userId}`);
        const recent = Array.isArray(transactions)
            ? transactions.slice(-5).reverse()
            : [];

        if (recent.length === 0) {
            container.innerHTML = `
                <div class="empty-state-box">
                    ${themeImageHtml("transactions", "No transactions")}
                    <h3>No transactions yet</h3>
                    <p>Add your first transaction to see it here.</p>
                </div>`;
            return;
        }

        container.innerHTML = recent.map(item => {
            const type = transactionTypeText(item.type);
            const isIncome = type === "Income";

            return `
                <div class="list-row">
                    <div>
                        <strong>${safeText(item.description || item.categoryName || "Transaction")}</strong>
                        <span>${safeText(item.categoryName || "No category")} • ${formatDate(item.date)}</span>
                    </div>

                    <b class="${isIncome ? "positive" : "negative"}">
                        ${isIncome ? "+" : "-"}${formatMoney(item.amount || 0)}
                    </b>
                </div>`;
        }).join("");
    } catch (error) {
        console.error(error);

        container.innerHTML = `
            <div class="empty-state-box">
                <img class="empty-visual" src="${currentIllustration("noData")}" alt="No data">
                <h3>Could not load transactions</h3>
                <p>Please try again later.</p>
            </div>`;
    }
}

async function loadGoalsPreview(userId) {
    const container = document.getElementById("goalsPreview");

    try {
        const goals = await apiRequest(`/SavingGoals/user/${userId}`);
        const preview = Array.isArray(goals)
            ? goals.slice(-4).reverse()
            : [];

        if (preview.length === 0) {
            container.innerHTML = `
                <div class="empty-state-box">
                    ${themeImageHtml("savings", "No saving goals")}
                    <h3>No saving goals yet</h3>
                    <p>Create a saving goal to track your progress.</p>
                </div>`;
            return;
        }

        container.innerHTML = preview.map(goal => {
            const progress = Number(goal.progressPercentage || 0);
            const safeProgress = Math.min(progress, 100);

            return `
                <div class="goal-preview">
                    <div class="list-row compact">
                        <div>
                            <strong>${safeText(goal.name || "Saving goal")}</strong>
                            <span>
                                ${formatMoney(goal.currentAmount || 0, currencyLabel(goal.currency))}
                                /
                                ${formatMoney(goal.targetAmount || 0, currencyLabel(goal.currency))}
                            </span>
                        </div>

                        <b>${safeProgress.toFixed(0)}%</b>
                    </div>

                    <div class="progress">
                        <div class="progress-fill" style="width:${safeProgress}%"></div>
                    </div>
                </div>`;
        }).join("");
    } catch (error) {
        console.error(error);

        container.innerHTML = `
            <div class="empty-state-box">
                 ${themeImageHtml("noData", "No data")}
                <h3>Could not load saving goals</h3>
                <p>Please try again later.</p>
            </div>`;
    }
}