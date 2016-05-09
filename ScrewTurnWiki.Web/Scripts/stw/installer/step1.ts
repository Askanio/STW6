/// <reference path="../typescript-ref/installerconstants.ts" />
module ScrewTurn.Wiki.Web.Installer {
    export class Step1Messages {
        public successTitle: string;
        public failureTitle: string;
        public failureWebConfigTestMessage: string;
        public failurePublicDirectoryTestMessage: string;
    }

    export class Step1 {
        private _wizard: InstallWizard;
        private _messages: Step1Messages;
        private _successTestWebConfig: boolean;
        private _successTestPublicDirectory: boolean;
        private _checkedPublicDirectory: string;

        constructor(wizard: InstallWizard, messages: Step1Messages) {
            this._wizard = wizard;
            this._messages = messages;

            this._successTestWebConfig = false;
            this._successTestPublicDirectory = false;

            this._wizard.disableContinueButton();
        }

        public bindEvents() {
            $("#testwebconfig").click((e) => {
                this.OnTestWebConfigClick(e);
            });
            $("#testpublicdirectory").click((e) => {
                this.OnTestPublicDirectoryClick(e);
            });
            $("#PublicDirectory").change((e) => {
                this.OnPublicDirectoryChange(e);
            });
            $("#step1-form").submit((e) => {
                return this.OnFormSubmit(e);
            });
        }

        private OnFormSubmit(e: any) {
            return (this._successTestWebConfig && this._successTestPublicDirectory && this._checkedPublicDirectory == $("#PublicDirectory").val());
        }

        private OnPublicDirectoryChange(e: any) {
            this._wizard.disableContinueButton();
            $("#testpublicdirectory-info").removeClass("test-error").removeClass("test-success").html("");
        }


        private OnTestWebConfigClick(e: any) {
            var url: string = STW_INSTALLER_TESTWEBCONFIG_URL;
            this._wizard.makeAjaxRequest(url, {}, (data: any) => { this.OnTestWebConfigSuccess(data); });
        }

        private OnTestWebConfigSuccess(data: any) {
            var element: string = "#testwebconfig-info";
            this._successTestWebConfig = data.Success;
            if (data.Success) {
                this._wizard.showSuccess(element, this._messages.successTitle);
                this.EnableContinueButton();
            }
            else {
                this._wizard.showFailure(element, this._messages.failureTitle, this._messages.failureWebConfigTestMessage + "\n" + data.ErrorMessage);
                this._wizard.disableContinueButton();
            }
        }

        private OnTestPublicDirectoryClick(e: any) {
            var url: string = STW_INSTALLER_TESTPUBLICDIRECTORY_URL;

            var jsonData: any =
                {
                    "publicDirectory": $("#PublicDirectory").val()
                };

            this._wizard.makeAjaxRequest(url, jsonData, (data: any) => { this.OnTestPublicDirectorySuccess(data); });
        }

        private OnTestPublicDirectorySuccess(data: any) {
            var element: string = "#testpublicdirectory-info";
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
        }

        private EnableContinueButton() {
            if (this._successTestWebConfig && this._successTestPublicDirectory) {
                this._wizard.enableContinueButton();
            }
        }
    }
}