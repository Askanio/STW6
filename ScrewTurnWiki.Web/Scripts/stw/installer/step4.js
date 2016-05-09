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
                        this._passForm = $("#setPassword");
                        this._ok = $("#completed");
                        this._result = $("#result");
                        this._pass = $("#newPwd");
                        this._rePass = $("#reNewPwd");
                        this._wizard = wizard;
                        this._messages = messages;
                        this._needMasterPassword = needMasterPassword;
                        if (this._needMasterPassword) {
                            $(this._passForm).show();
                            $(this._ok).hide();
                        }
                        else {
                            $(this._passForm).hide();
                            $(this._ok).show();
                        }
                    }
                    Step4.prototype.checkPassword = function () {
                        // Check Length
                        if (this._pass.val().length > 30 || this._pass.val().length < 6) {
                            this.updateTips(this._messages.checkLengthError, false);
                            return false;
                        }
                        // Check Regexp
                        if (!(/^([0-9a-zA-Z])+$/.test(this._pass.val()))) {
                            this.updateTips(this._messages.checkRegexpError, false);
                            return false;
                        }
                        // Check equal
                        if (this._pass.val() !== this._rePass.val()) {
                            this.updateTips(this._messages.checkEqualError, false);
                            return false;
                        }
                        $(this._result).removeClass("test-success").removeClass("test-error");
                        return true;
                    };
                    Step4.prototype.updateTips = function (t, success) {
                        $(this._result).html(t);
                        if (success)
                            $(this._result).addClass("test-success");
                        else
                            $(this._result).addClass("test-error");
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
                            "password": this._pass.val()
                        };
                        this._wizard.makeAjaxRequest(url, jsonData, function (data) { _this.OnSavePasswordSuccess(data); });
                    };
                    Step4.prototype.OnSavePasswordSuccess = function (data) {
                        if (data.Success) {
                            this.updateTips(this._messages.passwordSaveSuccess, true);
                            $(this._passForm).hide();
                            $(this._ok).show();
                        }
                        else {
                            bootbox.alert("<h2>" + this._messages.titleError + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + data.ErrorMessage + "</pre>");
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