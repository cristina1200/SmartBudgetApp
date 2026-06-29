function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.classList.add("show");
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.classList.remove("show");
}

document.addEventListener("click", event => {
    if (event.target.classList.contains("modal")) {
        event.target.classList.remove("show");
    }
});
