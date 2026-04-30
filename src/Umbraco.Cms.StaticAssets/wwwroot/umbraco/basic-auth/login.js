document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("form").forEach(function (form) {
        form.addEventListener("submit", function () {
            var btn = form.querySelector(".btn");
            if (btn && btn.dataset.submittingText) {
                btn.disabled = true;
                btn.textContent = btn.dataset.submittingText;
            }
        });
    });
});
