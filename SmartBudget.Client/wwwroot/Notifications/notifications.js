document.addEventListener("DOMContentLoaded", async () => {
    requireAuth();

    renderSidebar("notifications");
    renderTopbar("Notifications");

    await loadNotificationsPage();

    const markAllReadBtn = document.getElementById("markAllReadBtn");

    if (markAllReadBtn) {
        markAllReadBtn.addEventListener("click", async () => {
            await markAllNotificationsAsRead();
            await loadNotificationsPage();
        });
    }
});

async function loadNotificationsPage() {
    const container = document.getElementById("notificationsList");

    if (!container) {
        return;
    }

    const notifications = await getNotificationsForPage();

    if (notifications.length === 0) {
        container.innerHTML = `
            <div class="notifications-empty">
                <i class="fa-regular fa-bell"></i>
                No notifications yet.
            </div>
        `;
        return;
    }

    container.innerHTML = notifications.map(notification => {
        const unreadClass = notification.isRead ? "" : "unread";
        const typeClass = notificationTypeClass(notification.type);

        return `
            <div class="notification-page-item ${unreadClass} ${typeClass}">
                <div class="notification-page-item-header">
                    <h3>${safeText(notification.title)}</h3>
                    <span class="notification-page-item-time">
                        ${formatNotificationDate(notification.createdAt)}
                    </span>
                </div>

                <p>${safeText(notification.message)}</p>

                ${notification.isRead
                ? ""
                : `
                        <div class="notification-page-item-actions">
                            <button 
                                class="mark-read-btn" 
                                type="button" 
                                onclick="markSingleNotificationRead(${notification.id})"
                            >
                                Mark read
                            </button>
                        </div>
                    `}
            </div>
        `;
    }).join("");
}

async function getNotificationsForPage() {
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
        console.error("Notifications page could not be loaded.", error);
        return [];
    }
}

async function markSingleNotificationRead(id) {
    await markNotificationAsRead(id);
    await loadNotificationsPage();
}