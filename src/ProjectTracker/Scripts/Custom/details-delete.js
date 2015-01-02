$(document).ready(function () {
    if (window.location.search.indexOf("isDeleting=True") > -1)
        toggleDeleteDialog();
})

$(".btn-delete").click(toggleDeleteDialog);
$(".btn-delete-cancel").click(toggleDeleteDialog);

function toggleDeleteDialog() {
    $(".details-header").toggleClass("open");
    $(".details-delete").toggleClass("open");
}