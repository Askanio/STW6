var ScreTurn;
(function (ScreTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Common;
            (function (Common) {
                var MasterPasswordMessages = (function () {
                    function MasterPasswordMessages() {
                    }
                    return MasterPasswordMessages;
                })();
                Common.MasterPasswordMessages = MasterPasswordMessages;
                var MasterPassword = (function () {
                    function MasterPassword(messages) {
                        this._passForm = $("#newAdminPassForm");
                        this._passOk = $("#newAdminPassOk");
                        this._lblError = $("#lblError");
                        this._pass = $("#newPwd");
                        this._rePass = $("#reNewPwd");
                        this._messages = messages;
                        this._passForm.show();
                        this._passOk.hide();
                        this._lblError.hide();
                    }
                    MasterPassword.prototype.checkPassword = function () {
                        // Check Length
                        if (this._pass.val().length > 30 || this._pass.val().length < 6) {
                            this.updateTips(this._messages.checkLengthError);
                            return false;
                        }
                        // Check Regexp
                        if (!(/^([0-9a-zA-Z])+$/.test(this._pass.val()))) {
                            this.updateTips(this._messages.checkRegexpError);
                            return false;
                        }
                        // Check equal
                        if (this._pass.val() !== this._rePass.val()) {
                            this.updateTips(this._messages.checkEqualError);
                            return false;
                        }
                        return true;
                    };
                    MasterPassword.prototype.updateTips = function (t) {
                        this._lblError.text(t);
                    };
                    MasterPassword.prototype.bindEvents = function () {
                        var _this = this;
                        $("#btnSave").click(function (e) {
                            if (_this.checkPassword())
                                _this.OnSavePasswordClick(e);
                        });
                    };
                    MasterPassword.prototype.OnSavePasswordClick = function (e) {
                        var _this = this;
                        var url = STW_SET_MASTERPASSWORD_URL;
                        var jsonData = {
                            "password": this._pass.val()
                        };
                        this.makeAjaxRequest(url, jsonData, function (data) { _this.OnSavePasswordSuccess(data); });
                    };
                    MasterPassword.prototype.OnSavePasswordSuccess = function (data) {
                        if (data.Success) {
                            this._passForm.hide();
                            this._passOk.show();
                        }
                        else {
                            bootbox.alert("<h2>" + this._messages.titleError + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + data.ErrorMessage + "</pre>");
                        }
                    };
                    MasterPassword.prototype.makeAjaxRequest = function (url, data, successFunction) {
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
                    return MasterPassword;
                })();
                Common.MasterPassword = MasterPassword;
            })(Common = Web.Common || (Web.Common = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScreTurn.Wiki || (ScreTurn.Wiki = {}));
})(ScreTurn || (ScreTurn = {}));
//# sourceMappingURL=masterPassword.js.map