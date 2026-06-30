let notificationDocumentClickAttached = false;

document.addEventListener("DOMContentLoaded", () => {
    setTimeout(() => {
        initializeNotificationCenter();
    }, 100);
});

async function initializeNotificationCenter() {
    const bell = document.getElementById("notificationBell");
    const dropdown = document.getElementById("notificationDropdown");
    const list = document.getElementById("notificationDropdownList");
    const badge = document.getElementById("notificationBadge");

    if (!bell || !dropdown || !list || !badge) {
        return;
    }

    bell.onclick = async event => {
        event.preventDefault();
        event.stopPropagation();

        dropdown.classList.toggle("hidden");

        if (!dropdown.classList.contains("hidden")) {
            await loadNotificationDropdown();
        }
    };

    if (!notificationDocumentClickAttached) {
        document.addEventListener("click", event => {
            if (!event.target.closest(".notification-wrapper")) {
                const currentDropdown = document.getElementById("notificationDropdown");

                if (currentDropdown) {
                    currentDropdown.classList.add("hidden");
                }
            }
        });

        notificationDocumentClickAttached = true;
    }

    await refreshNotificationBadge();
    await showImportantNotificationToast();
}

async function fetchNotifications() {
    try {
        const userId = getUserId();

        if (!userId) {
            return [];
        }

        const notifications = await apiRequest(`/Notifications/user/${userId}`);

        if (!Array.isArray(notifications)) {
            return [];
        }

        return notifications.sort((a, b) => {
            return new Date(b.createdAt) - new Date(a.createdAt);
        });
    } catch (error) {
        console.error("Notifications could not be loaded.", error);
        return [];
    }
}

async function refreshNotificationBadge() {
    const badge = document.getElementById("notificationBadge");

    if (!badge) {
        return;
    }

    const notifications = await fetchNotifications();
    const unreadCount = notifications.filter(notification => !notification.isRead).length;

    if (unreadCount <= 0) {
        badge.textContent = "0";
        badge.classList.add("hidden");
        return;
    }

    badge.textContent = unreadCount > 9 ? "9+" : unreadCount;
    badge.classList.remove("hidden");
}

async function loadNotificationDropdown() {
    const list = document.getElementById("notificationDropdownList");

    if (!list) {
        return;
    }

    const notifications = await fetchNotifications();
    const latestNotifications = notifications.slice(0, 5);

    if (latestNotifications.length === 0) {
        list.innerHTML = `
            <div class="notification-empty">
                No notifications yet.
            </div>
        `;
        return;
    }

    list.innerHTML = latestNotifications.map(notification => {
        const unreadClass = notification.isRead ? "" : "unread";
        const typeClass = notificationTypeClass(notification.type);

        return `
            <a 
                href="../Notifications/notifications.html" 
                class="notification-preview-item ${unreadClass} ${typeClass}"
            >
                <strong>${safeText(notification.title)}</strong>
                <p>${safeText(notification.message)}</p>
                <span>${formatNotificationDate(notification.createdAt)}</span>
            </a>
        `;
    }).join("");
}

async function showImportantNotificationToast() {
    const notifications = await fetchNotifications();

    const importantNotification = notifications.find(notification => {
        const type = String(notification.type).toLowerCase();

        return !notification.isRead &&
            (
                type === "2" ||
                type === "3" ||
                type === "warning" ||
                type === "danger"
            );
    });

    if (!importantNotification) {
        return;
    }

    const toastKey = `notification-toast-${importantNotification.id}`;

    if (sessionStorage.getItem(toastKey) === "shown") {
        return;
    }

    sessionStorage.setItem(toastKey, "shown");

    renderNotificationToast(importantNotification);
}

function renderNotificationToast(notification) {
    const existingToast = document.getElementById("notificationToast");

    if (existingToast) {
        existingToast.remove();
    }

    const toast = document.createElement("div");
    toast.id = "notificationToast";
    toast.className = `notification-toast ${notificationTypeClass(notification.type)}`;

    toast.innerHTML = `
        <button class="notification-toast-close" type="button" title="Close">
            <i class="fa-solid fa-xmark"></i>
        </button>

        <div class="notification-toast-icon">
            <i class="fa-regular fa-bell"></i>
        </div>

        <div class="notification-toast-content">
            <strong>${safeText(notification.title)}</strong>
            <p>${safeText(notification.message)}</p>

            <a href="../Notifications/notifications.html">
                View all notifications
            </a>
        </div>
    `;

    document.body.appendChild(toast);

    const closeButton = toast.querySelector(".notification-toast-close");

    if (closeButton) {
        closeButton.addEventListener("click", () => {
            toast.remove();
        });
    }

    setTimeout(() => {
        if (document.body.contains(toast)) {
            toast.remove();
        }
    }, 9000);
}

function notificationTypeClass(type) {
    const value = String(type).toLowerCase();

    if (value === "3" || value === "danger") {
        return "danger";
    }

    if (value === "2" || value === "warning") {
        return "warning";
    }

    if (value === "4" || value === "success") {
        return "success";
    }

    return "info";
}

async function markAllNotificationsAsRead() {
    const userId = getUserId();

    if (!userId) {
        return;
    }

    try {
        await apiRequest(`/Notifications/user/${userId}/read-all`, "PATCH");

        await refreshNotificationBadge();
        await loadNotificationDropdown();
    } catch (error) {
        showToast("Notifications could not be marked as read.", "error");
    }
}

async function markNotificationAsRead(id) {
    try {
        await apiRequest(`/Notifications/${id}/read`, "PATCH");

        await refreshNotificationBadge();
        await loadNotificationDropdown();
    } catch (error) {
        showToast("Notification could not be marked as read.", "error");
    }
}

function formatNotificationDate(value) {
    if (!value) {
        return "";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return "";
    }

    return date.toLocaleString("ro-RO", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
    });
}