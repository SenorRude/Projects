$(".btn-vote").click(function () {
    $btn = $(this);
    $.ajax({
        url: $(this).attr("href"),
        type: "POST",
        cache: false,
        async: true,
        error: function (data) {
            alert("Error: " + data.statusText);
        },
        success: function (result) {
            $btn.toggleClass("btn-success");
            $btn.toggleClass("btn-default");
            $btn.prop("title", 
                ($btn.hasClass("btn-default")
                 ? "Vote for this project"
                 : "Remove vote"));
            $btn.find(".vote-count").html(result);
        }
    });
    return false;
});