/// <reference path="../typescript-ref/installerconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Installer;
            (function (Installer) {
                var Step4Messages = (function () {
                    function Step4Messages() {
                    }
                    return Step4Messages;
                }());
                Installer.Step4Messages = Step4Messages;
                var Step4 = (function () {
                    function Step4(wizard, messages, needMasterPassword) {
                        this.passForm = $("#setPassword");
                        this.ok = $("#completed");
                        this.result = $("#result");
                        this.pass = $("#newPwd");
                        this.rePass = $("#reNewPwd");
                        this.wizard = wizard;
                        this.messages = messages;
                        this.needMasterPassword = needMasterPassword;
                        if (this.needMasterPassword) {
                            $(this.passForm).show();
                            $(this.ok).hide();
                        }
                        else {
                            $(this.passForm).hide();
                            $(this.ok).show();
                        }
                    }
                    Step4.prototype.checkPassword = function () {
                        // Check Length
                        if (this.pass.val().length > 30 || this.pass.val().length < 6) {
                            this.updateTips(this.messages.checkLengthError, false);
                            return false;
                        }
                        // Check Regexp
                        if (!(/^([0-9a-zA-Z])+$/.test(this.pass.val()))) {
                            this.updateTips(this.messages.checkRegexpError, false);
                            return false;
                        }
                        // Check equal
                        if (this.pass.val() !== this.rePass.val()) {
                            this.updateTips(this.messages.checkEqualError, false);
                            return false;
                        }
                        $(this.result).removeClass("test-success").removeClass("test-error");
                        return true;
                    };
                    Step4.prototype.updateTips = function (t, success) {
                        $(this.result).html(t);
                        if (success)
                            $(this.result).addClass("test-success");
                        else
                            $(this.result).addClass("test-error");
                    };
                    Step4.prototype.bindEvents = function () {
                        var _this = this;
                        $("#btnSave").click(function (e) {
                            if (_this.checkPassword())
                                _this.OnSavePasswordClick(e);
                        });
                    };
                    Step4.prototype.OnSavePasswordClick = function (e) {
                        var _this = this;
                        var url = STW_SET_MASTERPASSWORD_URL;
                        var jsonData = {
                            "password": this.pass.val()
                        };
                        this.wizard.makeAjaxRequest(url, jsonData, function (data) { _this.OnSavePasswordSuccess(data); });
                    };
                    Step4.prototype.OnSavePasswordSuccess = function (data) {
                        if (data.Success) {
                            this.updateTips(this.messages.passwordSaveSuccess, true);
                            $(this.passForm).hide();
                            $(this.ok).show();
                        }
                        else {
                            bootbox.alert("<h2>" + this.messages.titleError + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + data.ErrorMessage + "</pre>");
                        }
                    };
                    return Step4;
                }());
                Installer.Step4 = Step4;
            })(Installer = Web.Installer || (Web.Installer = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
//# sourceMappingURL=step4.js.map