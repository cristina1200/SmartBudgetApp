function formatMoney(amount, currency = "RON") {
    return `${Number(amount || 0).toFixed(2)} ${currency}`;
}

function currencyLabel(currency) {
    if (typeof currency === "string") return currency;

    switch (Number(currency)) {
        case 1: return "RON";
        case 2: return "EUR";
        case 3: return "USD";
        case 4: return "GBP";
        default: return "RON";
    }
}

function currencySymbol(currency) {
    switch (currencyLabel(currency)) {
        case "RON": return "lei";
        case "EUR": return "€";
        case "USD": return "$";
        case "GBP": return "£";
        default: return "lei";
    }
}

function transactionTypeValue(type) {
    if (type === "Income" || Number(type) === 1) return 1;
    if (type === "Expense" || Number(type) === 2) return 2;
    return Number(type);
}

function transactionTypeText(type) {
    return transactionTypeValue(type) === 1 ? "Income" : "Expense";
}

function formatDate(date) {
    if (!date) return "-";
    return new Date(date).toLocaleDateString("ro-RO");
}

function todayIso() {
    return new Date().toISOString().split("T")[0];
}

function showMessage(elementId, message, type = "success") {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.textContent = message;
    element.className = type === "success" ? "message success" : "message error";

    setTimeout(() => {
        element.textContent = "";
        element.className = "message";
    }, 3500);
}

function safeText(value) {
    return String(value ?? "").replace(/[&<>'"]/g, char => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        "'": "&#039;",
        '"': "&quot;"
    }[char]));
}
