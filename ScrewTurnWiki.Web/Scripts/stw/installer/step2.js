/// <reference path="../typescript-ref/installerconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Installer;
            (function (Installer) {
                var Step2Messages = (function () {
                    function Step2Messages() {
                    }
                    return Step2Messages;
                }());
                Installer.Step2Messages = Step2Messages;
                var Step2 = (function () {
                    function Step2(wizard, messages) {
                        this._wizard = wizard;
                        this._messages = messages;
                        this._successTestDbConnection = false;
                        this._wizard.disableContinueButton();
                    }
                    Step2.prototype.bindEvents = function () {
                        var _this = this;
                        $("#testdbconnection").click(function (e) {
                            _this.OnTestDbConnectionClick(e);
                        });
                        $("#Step2-form").submit(function (e) {
                            return _this.OnFormSubmit(e);
                        });
                    };
                    Step2.prototype.OnFormSubmit = function (e) {
                        return (this._successTestDbConnection && this._checkedTestDbConnection == $("#ConnectionString").val());
                    };
                    Step2.prototype.OnTestDbConnectionClick = function (e) {
                        var _this = this;
                        var url = STW_INSTALLER_TESTDATABASE_URL;
                        var jsonData = {
                            "connectionString": $("#ConnectionString").val(),
                            "connectionSchemeName": $("#ConnectionSchemeName").val()
                        };
                        this._wizard.makeAjaxRequest(url, jsonData, function (data) { _this.OnTestDbConnectionSuccess(data); });
                    };
                    Step2.prototype.OnTestDbConnectionSuccess = function (data) {
                        var element = "#testdbconnection-info";
                        this._successTestDbConnection = data.Success;
                        if (data.Success) {
                            this._checkedTestDbConnection = $("#ConnectionString").val();
                            this._wizard.showSuccess(element, this._messages.successTitle);
                            this._wizard.enableContinueButton();
                        }
                        else {
                            this._wizard.showFailure(element, this._messages.failureTitle, this._messages.failureTestDbConnectionMessage + "\n" + data.ErrorMessage);
                            this._wizard.disableContinueButton();
                        }
                    };
                    return Step2;
                }());
                Installer.Step2 = Step2;
            })(Installer = Web.Installer || (Web.Installer = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
//# sourceMappingURL=step2.js.map