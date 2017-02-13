/// <reference path="../typescript-ref/commonconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Common;
            (function (Common) {
                var CommonWizard = (function () {
                    function CommonWizard() {
                    }
                    CommonWizard.prototype.makeAjaxRequest = function (url, data, successFunction) {
                        var request = $.ajax({
                            type: "GET",
                            url: url,
                            data: data,
                            dataType: "json"
                        });
                        request.done(successFunction);
                        request.fail(function (jqXHR, textStatus, errorThrown) {
                            bootbox.alert("<pre style='max-height:500px;overflow-y:scroll;'>" + errorThrown + "</pre>");
                        });
                    };
                    CommonWizard.prototype.requestConfirm = function () {
                        return confirm(CONFIRM_MESSAGE);
                    };
                    CommonWizard.prototype.createCookie = function (name, value, days) {
                        var expires = "";
                        if (days) {
                            var date = new Date();
                            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                            expires = "; expires=" + date.toUTCString();
                        }
                        else
                            expires = "";
                        document.cookie = name + "=" + value + expires + ";";
                    };
                    CommonWizard.prototype.readCookie = function (name) {
                        var nameEQ = name + "=";
                        var ca = document.cookie.split(';');
                        for (var i = 0; i < ca.length; i++) {
                            var c = ca[i];
                            while (c.charAt(0) == ' ')
                                c = c.substring(1, c.length);
                            if (c.indexOf(nameEQ) == 0)
                                return c.substring(nameEQ.length, c.length);
                        }
                        return null;
                    };
                    CommonWizard.prototype.eraseCookie = function (name) {
                        this.createCookie(name, "", -1);
                    };
                    return CommonWizard;
                }());
                Common.CommonWizard = CommonWizard;
            })(Common = Web.Common || (Web.Common = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
//# sourceMappingURL=commonwizard.js.map