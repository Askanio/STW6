/// <reference path="../typescript-ref/installerconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Installer;
            (function (Installer) {
                var InstallWizard = (function () {
                    function InstallWizard() {
                        // Set the bottom submit button to submit the form above it
                        $("#bottom-buttons button[type=submit]").click(function () {
                            $("form").submit();
                        });
                    }
                    InstallWizard.prototype.showSuccess = function (element, message) {
                        $(element).removeClass("test-error").addClass("test-success").html(message);
                    };
                    InstallWizard.prototype.showFailure = function (element, title, errorMessage) {
                        bootbox.alert("<h2>" + title + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + errorMessage + "</pre>");
                        $(element).removeClass("test-success").addClass("test-error").html(title);
                    };
                    InstallWizard.prototype.enableContinueButton = function () {
                        $(".continue").removeClass("disabled");
                        $(".continue").show();
                    };
                    InstallWizard.prototype.disableContinueButton = function () {
                        $(".continue").addClass("disabled");
                        $(".continue").hide();
                    };
                    InstallWizard.prototype.makeAjaxRequest = function (url, data, successFunction) {
                        var request = $.ajax({
                            type: "GET",
                            url: url,
                            data: data,
                            dataType: "json"
                        });
                        request.done(successFunction);
                        request.fail(function (jqXHR, textStatus, errorThrown) {
                            bootbox.alert("<pre style='max-height:500px;overflow-y:scroll;'>" + STW_INSTALLER_WOOPS + errorThrown + "</pre>");
                        });
                    };
                    return InstallWizard;
                }());
                Installer.InstallWizard = InstallWizard;
            })(Installer = Web.Installer || (Web.Installer = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
