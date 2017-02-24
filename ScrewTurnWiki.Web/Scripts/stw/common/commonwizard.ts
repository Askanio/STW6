/// <reference path="../typescript-ref/commonconstants.ts" />
module ScrewTurn.Wiki.Web.Common {
    export class CommonWizard {
        constructor() {
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
                bootbox.alert("<pre style='max-height:500px;overflow-y:scroll;'>" + errorThrown + "</pre>");

            });
        }

        public requestConfirm() {
            return confirm(CONFIRM_MESSAGE);
        }

        public showMessage(messageType, message) {
            var type = "success";
            if (messageType === 1)
                type = "alert";
            if (messageType === 2)
                type = "error";
            if (messageType === 3)
                type = "warning";
            if (messageType === 4)
                type = "information";
            if (messageType === 5)
                type = "notification";
            noty({
                text: message,
                layout: 'center',
                closeWith: ['click', 'hover'],
                type: type // alert, success, error, warning, information, notification
            });
        }

        public createCookie(name, value, days) {
            var expires = "";
            if (days) {
                var date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                expires = "; expires=" + date.toUTCString();
            } else expires = "";
            document.cookie = name + "=" + value + expires + ";";
        }

        public readCookie(name) {
            var nameEQ = name + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') c = c.substring(1, c.length);
                if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
            }
            return null;
        }

        public eraseCookie(name) {
            this.createCookie(name, "", -1);
        }
    }
}