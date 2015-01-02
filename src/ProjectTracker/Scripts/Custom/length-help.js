$(".length-help").focus(function () {
    $(".help-block.active").removeClass("active");  // Hide any help-block already showing.
    $(this).next(".help-block").addClass("active"); // Show the one below the focused field.
    setLengthHelpText($(this));
});

$(".length-help").on("input", function () {
    setLengthHelpText($(this));
});

function setLengthHelpText(field) {
    var usedChars = field.val().length;
    var maxChars = field.attr("data-val-length-max")
    var helpBlock = field.next(".help-block");
    helpBlock.toggleClass("has-error", (usedChars > maxChars));
    return helpBlock.text(usedChars + " of " + maxChars + " characters");
}