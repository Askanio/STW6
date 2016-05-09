/// <reference path="../typescript-ref/installerconstants.ts" />
module ScrewTurn.Wiki.Web.Installer {
    export class Step2Messages {
        public successTitle: string;
        public failureTitle: string;
        public failureTestDbConnectionMessage: string;
    }

    export class Step2 {
        private _wizard: InstallWizard;
        private _messages: Step2Messages;
        private _successTestDbConnection: boolean;
        private _checkedTestDbConnection: string;

        constructor(wizard: InstallWizard, messages: Step2Messages) {
            this._wizard = wizard;
            this._messages = messages;

            this._successTestDbConnection = false;

            this._wizard.disableContinueButton();
        }

        public bindEvents() {
            $("#testdbconnection").click((e) => {
                this.OnTestDbConnectionClick(e);
            });
            $("#Step2-form").submit((e) => {
                return this.OnFormSubmit(e);
            });
        }

        private OnFormSubmit(e: any) {
            return (this._successTestDbConnection && this._checkedTestDbConnection == $("#ConnectionString").val());
        }

        private OnTestDbConnectionClick(e: any) {
            var url: string = STW_INSTALLER_TESTDATABASE_URL;

            var jsonData: any =
                {
                    "connectionString": $("#ConnectionString").val(),
                    "connectionSchemeName": $("#ConnectionSchemeName").val()
                };

            this._wizard.makeAjaxRequest(url, jsonData, (data: any) => { this.OnTestDbConnectionSuccess(data); });
        }

        private OnTestDbConnectionSuccess(data: any) {
            var element: string = "#testdbconnection-info";
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
        }
    }
}