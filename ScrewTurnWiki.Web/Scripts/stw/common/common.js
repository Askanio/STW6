function showLoader() {
    $('#loadingDiv').show();
}

function hideLoader(page) {
    $('#loadingDiv').hide();

    $("div[id^='PageSelectorDiv'").children('a').each(function(i) {
        if ($(this).hasClass("selected"))
            $(this).removeClass("selected");
        var id = "lnkp" + page;
        if (this.id === id)
            $(this).addClass("selected");
    });
}