/// <reference path="../typescript-ref/installerconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Installer;
            (function (Installer) {
                var Step1Messages = (function () {
                    function Step1Messages() {
                    }
                    return Step1Messages;
                }());
                Installer.Step1Messages = Step1Messages;
                var Step1 = (function () {
                    function Step1(wizard, messages) {
                        this._wizard = wizard;
                        this._messages = messages;
                        this._successTestWebConfig = false;
                        this._successTestPublicDirectory = false;
                        this._wizard.disableContinueButton();
                    }
                    Step1.prototype.bindEvents = function () {
                        var _this = this;
                        $("#testwebconfig").click(function (e) {
                            _this.OnTestWebConfigClick(e);
                        });
                        $("#testpublicdirectory").click(function (e) {
                            _this.OnTestPublicDirectoryClick(e);
                        });
                        $("#PublicDirectory").change(function (e) {
                            _this.OnPublicDirectoryChange(e);
                        });
                        $("#step1-form").submit(function (e) {
                            return _this.OnFormSubmit(e);
                        });
                    };
                    Step1.prototype.OnFormSubmit = function (e) {
                        return (this._successTestWebConfig && this._successTestPublicDirectory && this._checkedPublicDirectory == $("#PublicDirectory").val());
                    };
                    Step1.prototype.OnPublicDirectoryChange = function (e) {
                        this._wizard.disableContinueButton();
                        $("#testpublicdirectory-info").removeClass("test-error").removeClass("test-success").html("");
                    };
                    Step1.prototype.OnTestWebConfigClick = function (e) {
                        var _this = this;
                        var url = STW_INSTALLER_TESTWEBCONFIG_URL;
                        this._wizard.makeAjaxRequest(url, {}, function (data) { _this.OnTestWebConfigSuccess(data); });
                    };
                    Step1.prototype.OnTestWebConfigSuccess = function (data) {
                        var element = "#testwebconfig-info";
                        this._successTestWebConfig = data.Success;
                        if (data.Success) {
                            this._wizard.showSuccess(element, this._messages.successTitle);
                            this.EnableContinueButton();
                        }
                        else {
                            this._wizard.showFailure(element, this._messages.failureTitle, this._messages.failureWebConfigTestMessage + "\n" + data.ErrorMessage);
                            this._wizard.disableContinueButton();
                        }
                    };
                    Step1.prototype.OnTestPublicDirectoryClick = function (e) {
                        var _this = this;
                        var url = STW_INSTALLER_TESTPUBLICDIRECTORY_URL;
                        var jsonData = {
                            "publicDirectory": $("#PublicDirectory").val()
                        };
                        this._wizard.makeAjaxRequest(url, jsonData, function (data) { _this.OnTestPublicDirectorySuccess(data); });
                    };
                    Step1.prototype.OnTestPublicDirectorySuccess = function (data) {
                        var element = "#testpublicdirectory-info";
                        this._successTestPublicDirectory = data.Success;
                        if (data.Success) {
                            this._checkedPublicDirectory = $("#PublicDirectory").val();
                            this._wizard.showSuccess(element, this._messages.successTitle);
                            this.EnableContinueButton();
                        }
                        else {
                            this._wizard.showFailure(element, this._messages.failureTitle, this._messages.failurePublicDirectoryTestMessage + "\n" + data.ErrorMessage);
                            this._wizard.disableContinueButton();
                        }
                    };
                    Step1.prototype.EnableContinueButton = function () {
                        if (this._successTestWebConfig && this._successTestPublicDirectory) {
                            this._wizard.enableContinueButton();
                        }
                    };
                    return Step1;
                }());
                Installer.Step1 = Step1;
            })(Installer = Web.Installer || (Web.Installer = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
