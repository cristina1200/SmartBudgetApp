document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("saving-goals");
    renderTopbar("Saving Goals");

    await loadGoals();

    document.getElementById("goalForm").addEventListener("submit", saveGoal);
});

async function loadGoals() {
    const container = document.getElementById("goalsGrid");

    try {
        const goals = await apiRequest(`/SavingGoals/user/${getUserId()}`);

        if (goals.length === 0) {
            container.innerHTML = `
                <div class="card empty-state-box">
                    <img class="empty-visual" src="${currentIllustration("savings")}" alt="No saving goals">
                    <h3>No saving goals yet</h3>
                    <p>Create a goal and start saving for your dreams.</p>
                </div>`;
            return;
        }

        container.innerHTML = goals.map(goal => goalCard(goal)).join("");
    } catch (error) {
        showToast("Saving goals could not be loaded.", "error");
    }
}

function goalCard(goal) {
    const currency = currencyLabel(goal.currency);
    const progress = Math.min(Number(goal.progressPercentage || 0), 100);

    return `
        <article class="card goal-card">
            <div class="goal-header">
                <h2>${safeText(goal.name)}</h2>

                <button class="icon-btn danger" onclick="deleteGoal(${goal.id})" title="Delete goal">
                    <i class="fa-solid fa-trash"></i>
                </button>
            </div>

            <p>
                ${formatMoney(goal.currentAmount, currency)}
                saved from
                ${formatMoney(goal.targetAmount, currency)}
            </p>

            <div class="progress">
                <div class="progress-fill" style="width:${progress}%"></div>
            </div>

            <strong>${progress.toFixed(0)}%</strong>

            <span>Deadline: ${formatDate(goal.deadline)}</span>

            <div class="add-money">
                <input type="number" step="0.01" id="amount-${goal.id}" placeholder="Add amount">

                <select id="currency-${goal.id}">
                    <option value="1">RON</option>
                    <option value="2">EUR</option>
                    <option value="3">USD</option>
                    <option value="4">GBP</option>
                </select>

                <button class="btn" onclick="addMoney(${goal.id})">
                    Add
                </button>
            </div>

            <div class="goal-actions">
                <button class="btn btn-secondary" onclick='editGoal(${JSON.stringify(goal)})'>
                    <i class="fa-solid fa-pen"></i>
                    Edit
                </button>

                <button class="btn" onclick="openGoalDetails(${goal.id})">
                    <i class="fa-solid fa-chart-line"></i>
                    View details
                </button>
            </div>
        </article>`;
}

async function saveGoal(event) {
    event.preventDefault();

    const id = document.getElementById("goalId").value;

    const dto = {
        name: document.getElementById("name").value.trim(),
        targetAmount: Number(document.getElementById("targetAmount").value),
        currentAmount: Number(document.getElementById("currentAmount").value),
        deadline: document.getElementById("deadline").value || null,
        currency: Number(document.getElementById("currency").value)
    };

    try {
        if (id) {
            await apiRequest(`/SavingGoals/${id}`, "PUT", dto);
            showMessage("goalMessage", "Goal updated successfully.", "success");
        } else {
            await apiRequest("/SavingGoals", "POST", {
                ...dto,
                userId: getUserId()
            });

            showMessage("goalMessage", "Goal created successfully.", "success");
        }

        event.target.reset();
        document.getElementById("goalId").value = "";
        document.getElementById("currentAmount").value = 0;

        await loadGoals();
    } catch (error) {
        showMessage(
            "goalMessage",
            error.message || "Goal could not be saved.",
            "error"
        );
    }
}

function editGoal(goal) {
    document.getElementById("goalId").value = goal.id;
    document.getElementById("name").value = goal.name;
    document.getElementById("targetAmount").value = goal.targetAmount;
    document.getElementById("currentAmount").value = goal.currentAmount;
    document.getElementById("currency").value = Number(goal.currency) || 1;
    document.getElementById("deadline").value =
        goal.deadline ? goal.deadline.split("T")[0] : "";

    window.scrollTo({
        top: 0,
        behavior: "smooth"
    });
}

async function addMoney(id) {
    const amount = Number(document.getElementById(`amount-${id}`).value);
    const currency = Number(document.getElementById(`currency-${id}`).value);

    if (amount <= 0) {
        showToast("Enter a valid amount.", "error");
        return;
    }

    try {
        await apiRequest(`/SavingGoals/${id}/add-money`, "POST", {
            amount,
            currency,
            note: "Quick contribution"
        });

        showToast("Money added to goal.");
        await loadGoals();
    } catch (error) {
        showToast(error.message || "Money could not be added.", "error");
    }
}

async function deleteGoal(id) {
    if (!confirm("Delete this saving goal?")) {
        return;
    }

    await apiRequest(`/SavingGoals/${id}`, "DELETE");

    showToast("Saving goal deleted.");

    await loadGoals();
}

function openGoalDetails(id) {
    window.location.href = `./saving-goal-details.html?id=${id}`;
}