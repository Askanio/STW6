/// <reference path="../typescript-ref/installerconstants.ts" />
module ScrewTurn.Wiki.Web.Installer {
    export class InstallWizard {
        constructor() {
            // Set the bottom submit button to submit the form above it
            $("#bottom-buttons button[type=submit]").click(function () {
                $("form").submit();
            });

        }

        public showSuccess(element: string, message: string) {
            $(element).removeClass("test-error").addClass("test-success").html(message);
        }

        public showFailure(element: string, title: string, errorMessage: string) {
            bootbox.alert("<h2>" + title + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + errorMessage + "</pre>");
            $(element).removeClass("test-success").addClass("test-error").html(title);
        }

        public enableContinueButton() {
            $(".continue").removeClass("disabled");
            $(".continue").show();
        }

        public disableContinueButton() {
            $(".continue").addClass("disabled");
            $(".continue").hide();
        }

        public makeAjaxRequest(url: string, data: any, successFunction: (data: any) => void) {
            var request = $.ajax({
                type: "GET",
                url: url,
                data: data,
                dataType: "json"
            });

            request.done(successFunction);

            request.fail(function (jqXHR, textStatus, errorThrown: SyntaxError) {
                bootbox.alert("<pre style='max-height:500px;overflow-y:scroll;'>" + STW_INSTALLER_WOOPS + errorThrown + "</pre>");

            });
        }
    }
}