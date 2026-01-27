// =====================================================
// GLOBAL SITE JS
// =====================================================

// SIDEBAR TOGGLE
document.addEventListener("click", function (e) {

    const parentLink = e.target.closest(".sidebar-parent-link");
    if (!parentLink) return;

    e.preventDefault();

    const item = parentLink.closest(".sidebar-item");
    if (!item) return;

    item.classList.toggle("open");
});
