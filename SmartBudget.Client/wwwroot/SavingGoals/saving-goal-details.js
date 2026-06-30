document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("saving-goals");
    renderTopbar("Saving Goal Details");

    const goalId = new URLSearchParams(window.location.search).get("id");

    if (!goalId) {
        showToast("Saving goal id is missing.", "error");
        return;
    }

    await loadSavingGoalDetails(goalId);
});

async function loadSavingGoalDetails(goalId) {
    try {
        const details = await getSavingGoalDetails(goalId);

        if (!details) {
            showToast("Saving goal details could not be loaded.", "error");
            return;
        }

        console.log("Saving goal details:", details);

        renderGoalMain(details);
        renderGoalOverview(details);
        renderGoalStatistics(details);
        renderContributionHistory(details.contributions || [], details.currency);
    } catch (error) {
        console.error(error);
        showToast("Saving goal details could not be loaded.", "error");
    }
}

async function getSavingGoalDetails(goalId) {
    /*
     * Incercam mai multe variante, ca sa mearga indiferent cum ai endpoint-ul in controller.
     */
    const endpoints = [
        `/SavingGoals/${goalId}/details`,
        `/SavingGoals/details/${goalId}`,
        `/SavingGoals/${goalId}`
    ];

    for (const endpoint of endpoints) {
        try {
            const result = await apiRequest(endpoint);

            if (result) {
                return result;
            }
        } catch (error) {
            console.warn(`Endpoint failed: ${endpoint}`, error);
        }
    }

    return null;
}

function renderGoalMain(details) {
    const name = details.name || details.Name || "Saving goal";

    const targetAmount = Number(details.targetAmount ?? details.TargetAmount ?? 0);
    const currentAmount = Number(details.currentAmount ?? details.CurrentAmount ?? 0);
    const remainingAmount = Number(
        details.remainingAmount ??
        details.RemainingAmount ??
        Math.max(targetAmount - currentAmount, 0)
    );

    const progress = Number(details.progressPercentage ?? details.ProgressPercentage ?? 0);
    const currency = getCurrencyText(details.currency ?? details.Currency);

    const estimatedWeeks = details.estimatedWeeksToComplete ??
        details.EstimatedWeeksToComplete ??
        details.estimatedWeeks ??
        details.EstimatedWeeks ??
        null;

    const smartMessage = details.smartMessage ??
        details.SmartMessage ??
        buildPredictionText(estimatedWeeks);

    setText("goalName", name);
    setText("goalPredictionText", smartMessage);
    setText("goalPredictionBoxText", smartMessage);

    setText("goalTargetAmount", formatGoalMoney(targetAmount, currency));
    setText("goalCurrentAmount", formatGoalMoney(currentAmount, currency));
    setText("goalRemainingAmount", formatGoalMoney(remainingAmount, currency));

    setText("goalProgressPercent", `${progress.toFixed(0)}%`);

    const progressBar = document.getElementById("goalProgressBar");

    if (progressBar) {
        progressBar.style.width = `${Math.min(progress, 100)}%`;
    }
}

function renderGoalOverview(details) {
    const progress = Number(details.progressPercentage ?? details.ProgressPercentage ?? 0);

    const estimatedWeeks = details.estimatedWeeksToComplete ??
        details.EstimatedWeeksToComplete ??
        details.estimatedWeeks ??
        details.EstimatedWeeks ??
        null;

    const recommendedWeekly = Number(
        details.recommendedWeeklyContribution ??
        details.RecommendedWeeklyContribution ??
        0
    );

    const deadline = details.deadline ?? details.Deadline ?? null;
    const currency = getCurrencyText(details.currency ?? details.Currency);

    renderProgressCircle(progress);

    setText("overviewEstimatedTime", buildEstimatedWeeksText(estimatedWeeks));
    setText("overviewRecommendedWeekly", formatGoalMoney(recommendedWeekly, currency));
    setText("overviewDeadline", deadline ? formatDate(deadline) : "-");
}

function renderGoalStatistics(details) {
    const currency = getCurrencyText(details.currency ?? details.Currency);

    const totalContributed = Number(
        details.totalContributed ??
        details.TotalContributed ??
        details.currentAmount ??
        details.CurrentAmount ??
        0
    );

    const averageWeekly = Number(
        details.averageWeeklySaving ??
        details.AverageWeeklySaving ??
        0
    );

    const averageMonthly = Number(
        details.averageMonthlySaving ??
        details.AverageMonthlySaving ??
        0
    );

    const estimatedWeeks = details.estimatedWeeksToComplete ??
        details.EstimatedWeeksToComplete ??
        details.estimatedWeeks ??
        details.EstimatedWeeks ??
        null;

    const estimatedCompletionDate = details.estimatedCompletionDate ??
        details.EstimatedCompletionDate ??
        null;

    const recommendedWeekly = Number(
        details.recommendedWeeklyContribution ??
        details.RecommendedWeeklyContribution ??
        0
    );

    setText("statTotalContributed", formatGoalMoney(totalContributed, currency));
    setText("statAverageWeekly", formatGoalMoney(averageWeekly, currency));
    setText("statAverageMonthly", formatGoalMoney(averageMonthly, currency));
    setText("statEstimatedTime", buildEstimatedWeeksText(estimatedWeeks));
    setText("statEstimatedCompletion", estimatedCompletionDate ? formatDate(estimatedCompletionDate) : "-");
    setText("statRecommendedWeekly", formatGoalMoney(recommendedWeekly, currency));
}

function renderContributionHistory(contributions, currencyValue) {
    const container = document.getElementById("contributionHistory");

    if (!container) {
        return;
    }

    const currency = getCurrencyText(currencyValue);

    if (!contributions || contributions.length === 0) {
        container.innerHTML = `
            <div class="empty-box">
                No contributions yet.
            </div>
        `;
        return;
    }

    container.innerHTML = contributions
        .slice()
        .reverse()
        .map(item => {
            const amount = Number(item.amount ?? item.Amount ?? 0);
            const createdAt = item.createdAt ?? item.CreatedAt ?? item.date ?? item.Date ?? null;
            const note = item.note ?? item.Note ?? item.source ?? item.Source ?? "Contribution";

            return `
                <div class="contribution-row">
                    <div>
                        <strong>+${formatGoalMoney(amount, currency)}</strong>
                        <span>${createdAt ? formatDate(createdAt) : "-"}</span>
                    </div>

                    <div>
                        <span>${safeText(note)}</span>
                    </div>
                </div>
            `;
        })
        .join("");
}

function renderProgressCircle(progress) {
    const safeProgress = Math.max(0, Math.min(100, Number(progress || 0)));

    const circle = document.getElementById("goalProgressCircle");
    const value = document.getElementById("goalProgressCircleValue");

    if (circle) {
        circle.style.setProperty("--progress", safeProgress);
    }

    if (value) {
        value.textContent = `${safeProgress.toFixed(0)}%`;
    }
}

function buildPredictionText(estimatedWeeks) {
    if (!estimatedWeeks || Number(estimatedWeeks) <= 0) {
        return "Add more contributions to receive a better prediction.";
    }

    return `At your current pace, you can complete this goal in about ${Math.ceil(Number(estimatedWeeks))} week(s).`;
}

function buildEstimatedWeeksText(estimatedWeeks) {
    if (!estimatedWeeks || Number(estimatedWeeks) <= 0) {
        return "-";
    }

    return `${Math.ceil(Number(estimatedWeeks))} week(s)`;
}

function formatGoalMoney(value, currency) {
    const amount = Number(value || 0);

    return `${amount.toFixed(2)} ${currency}`;
}

function getCurrencyText(currency) {
    if (currency === null || currency === undefined) {
        return "RON";
    }

    const value = String(currency);

    if (value === "0") {
        return "RON";
    }

    if (value === "1") {
        return "EUR";
    }

    if (value === "2") {
        return "USD";
    }

    return value;
}

function setText(id, value) {
    const element = document.getElementById(id);

    if (element) {
        element.textContent = value;
    }
}