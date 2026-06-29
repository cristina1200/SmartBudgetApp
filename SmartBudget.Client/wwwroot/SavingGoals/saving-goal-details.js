let currentGoalId = null;
let currentGoalCurrency = 1;

document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("saving-goals");
    renderTopbar("Saving Goal Details");

    currentGoalId = getGoalIdFromUrl();

    if (!currentGoalId) {
        document.getElementById("detailsContainer").innerHTML = `
            <div class="card empty-state-box">
                <h3>Saving goal not found</h3>
                <p>The selected saving goal could not be loaded.</p>
                <a class="btn" href="./saving-goals.html">Back to goals</a>
            </div>`;
        return;
    }

    await loadGoalDetails();
});

function getGoalIdFromUrl() {
    const params = new URLSearchParams(window.location.search);
    return params.get("id");
}

async function loadGoalDetails() {
    const container = document.getElementById("detailsContainer");

    try {
        const goal = await apiRequest(`/SavingGoals/details/${currentGoalId}`);

        currentGoalCurrency = Number(goal.currency) || 1;

        container.innerHTML = renderGoalDetails(goal);

        document
            .getElementById("addContributionForm")
            .addEventListener("submit", addContribution);
    } catch (error) {
        console.error(error);

        container.innerHTML = `
            <div class="card empty-state-box">
                <img class="empty-visual" src="${currentIllustration("noData")}" alt="No data">
                <h3>Could not load saving goal</h3>
                <p>Please try again later.</p>
                <a class="btn" href="./saving-goals.html">Back to goals</a>
            </div>`;
    }
}

function renderGoalDetails(goal) {
    const currency = currencyLabel(goal.currency);
    const progress = Math.min(Number(goal.progressPercentage || 0), 100);
    const estimatedText = formatEstimatedWeeks(goal.estimatedWeeksToComplete);
    const recommendedText = formatRecommended(goal.recommendedWeeklyContribution, currency);

    return `
        <a href="./saving-goals.html" class="back-link">
            <i class="fa-solid fa-arrow-left"></i>
            Back to saving goals
        </a>

        <section class="goal-details-hero">
            <article class="card goal-main-card">
                <div class="goal-title-row">
                    <div>
                        <h2>${safeText(goal.name)}</h2>
                        <p>${safeText(goal.smartMessage)}</p>
                    </div>

                    <div class="goal-icon-badge">
                        <i class="fa-solid fa-piggy-bank"></i>
                    </div>
                </div>

                <div class="goal-amounts-grid">
                    <div class="goal-mini-stat">
                        <span>Target amount</span>
                        <strong>${formatMoney(goal.targetAmount, currency)}</strong>
                    </div>

                    <div class="goal-mini-stat">
                        <span>Already saved</span>
                        <strong>${formatMoney(goal.currentAmount, currency)}</strong>
                    </div>

                    <div class="goal-mini-stat">
                        <span>Remaining</span>
                        <strong>${formatMoney(goal.remainingAmount, currency)}</strong>
                    </div>
                </div>

                <div class="goal-progress-large">
                    <div class="goal-progress-header">
                        <span>Progress</span>
                        <strong>${progress.toFixed(0)}%</strong>
                    </div>

                    <div class="progress">
                        <div class="progress-fill" style="width:${progress}%"></div>
                    </div>
                </div>

                <div class="goal-smart-message">
                    <i class="fa-solid fa-chart-line"></i>
                    ${safeText(goal.smartMessage)}
                </div>
            </article>

            <article class="card goal-insight-card">
                <div class="card-header">
                    <h2>Goal overview</h2>
                </div>

                <div class="goal-overview-illustration">
                    <img src="${currentIllustration("savings")}" alt="Saving goal overview">
                </div>

                <div class="goal-overview-list">
                    <div>
                        <span>Estimated time</span>
                        <strong>${estimatedText}</strong>
                    </div>

                    <div>
                        <span>Recommended weekly contribution</span>
                        <strong>${recommendedText}</strong>
                    </div>

                    <div>
                        <span>Deadline</span>
                        <strong>${formatDate(goal.deadline)}</strong>
                    </div>
                </div>
            </article>
        </section>

        <section class="goal-details-grid">
            <article class="card">
                <div class="card-header">
                    <h2>Smart statistics</h2>
                </div>

                <div class="analytics-grid">
                    <div class="analytics-card">
                        <span>Total contributed</span>
                        <strong>${formatMoney(goal.totalContributed, currency)}</strong>
                    </div>

                    <div class="analytics-card">
                        <span>Average weekly saving</span>
                        <strong>${formatMoney(goal.averageWeeklySaving, currency)}</strong>
                    </div>

                    <div class="analytics-card">
                        <span>Average monthly saving</span>
                        <strong>${formatMoney(goal.averageMonthlySaving, currency)}</strong>
                    </div>

                    <div class="analytics-card">
                        <span>Estimated time</span>
                        <strong>${estimatedText}</strong>
                    </div>

                    <div class="analytics-card">
                        <span>Estimated completion</span>
                        <strong>${formatDate(goal.estimatedCompletionDate)}</strong>
                    </div>

                    <div class="analytics-card">
                        <span>Recommended weekly amount</span>
                        <strong>${recommendedText}</strong>
                    </div>
                </div>

                <form id="addContributionForm" class="add-contribution-form">
                    <input type="number" step="0.01" id="contributionAmount" placeholder="Add amount" required>

                    <select id="contributionCurrency">
                        <option value="1">RON</option>
                        <option value="2">EUR</option>
                        <option value="3">USD</option>
                        <option value="4">GBP</option>
                    </select>

                    <textarea id="contributionNote" placeholder="Optional note, for example: salary bonus, gift, monthly saving"></textarea>

                    <button class="btn" type="submit">
                        <i class="fa-solid fa-plus"></i>
                        Add contribution
                    </button>
                </form>
            </article>

            <article class="card">
                <div class="card-header">
                    <h2>Contribution history</h2>
                </div>

                ${renderContributions(goal.contributions, currency)}
            </article>
        </section>
    `;
}

function renderContributions(contributions, currency) {
    if (!contributions || contributions.length === 0) {
        return `
            <div class="empty-state-box">
                <img class="empty-visual" src="${currentIllustration("savings")}" alt="No contributions">
                <h3>No contributions yet</h3>
                <p>Add money to this goal and the history will appear here.</p>
            </div>`;
    }

    return `
        <div class="contributions-list">
            ${contributions.map(item => `
                <div class="contribution-row">
                    <div>
                        <strong>+${formatMoney(item.amount, currency)}</strong>
                        <span>${formatDate(item.createdAt)}</span>
                        ${item.note ? `<span>${safeText(item.note)}</span>` : ""}
                    </div>

                    <div>
                        <span>Original</span>
                        <strong>${formatMoney(item.originalAmount, currencyLabel(item.currency))}</strong>
                    </div>
                </div>
            `).join("")}
        </div>
    `;
}

async function addContribution(event) {
    event.preventDefault();

    const amount = Number(document.getElementById("contributionAmount").value);
    const currency = Number(document.getElementById("contributionCurrency").value);
    const note = document.getElementById("contributionNote").value.trim();

    if (amount <= 0) {
        showToast("Enter a valid amount.", "error");
        return;
    }

    try {
        await apiRequest(`/SavingGoals/${currentGoalId}/add-money`, "POST", {
            amount,
            currency,
            note
        });

        showToast("Contribution added successfully.");
        await loadGoalDetails();
    } catch (error) {
        showToast(error.message || "Contribution could not be added.", "error");
    }
}

function formatEstimatedWeeks(value) {
    if (value === null || value === undefined) {
        return "Not enough data";
    }

    if (Number(value) === 0) {
        return "Completed";
    }

    return `${value} week(s)`;
}

function formatRecommended(value, currency) {
    if (value === null || value === undefined) {
        return "Set a deadline";
    }

    return formatMoney(value, currency);
}