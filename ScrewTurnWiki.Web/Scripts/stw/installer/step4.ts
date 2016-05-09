/// <reference path="../typescript-ref/installerconstants.ts" />
module ScrewTurn.Wiki.Web.Installer {

    export class Step4Messages {
        public checkLengthError: string;
        public checkRegexpError: string;
        public checkEqualError: string;
        public titleError: string;
        public passwordSaveSuccess: string;
    }

    export class Step4 {
        private _wizard: InstallWizard;
        private _messages: Step4Messages;
        private _needMasterPassword: boolean;
        private _passForm = $("#setPassword");
        private _ok = $("#completed");
        private _result = $("#result");
        private _pass = $("#newPwd");
        private _rePass = $("#reNewPwd");

        constructor(wizard: InstallWizard, messages: Step4Messages, needMasterPassword: boolean) {
            this._wizard = wizard;
            this._messages = messages;
            this._needMasterPassword = needMasterPassword;

            if (this._needMasterPassword) {
                $(this._passForm).show();
                $(this._ok).hide();
                //this._result.hide();
            } else {
                $(this._passForm).hide();
                $(this._ok).show();
                //this._result.hide(); 
            }
        }

        private checkPassword() {
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
        }

        private updateTips(t: string, success: boolean) {
            $(this._result).html(t);
            if (success)
                $(this._result).addClass("test-success");
            else
                $(this._result).addClass("test-error");
        }

        public bindEvents() {
            $("#btnSave").click((e) => {
                if (this.checkPassword())
                    this.OnSavePasswordClick(e);
            });
        }

        private OnSavePasswordClick(e: any) {
            var url: string = STW_SET_MASTERPASSWORD_URL;

            var jsonData: any =
                {
                    "password": this._pass.val()
                };

            this._wizard.makeAjaxRequest(url, jsonData, (data: any) => { this.OnSavePasswordSuccess(data); });
        }

        private OnSavePasswordSuccess(data: any) {
            if (data.Success) {
                this.updateTips(this._messages.passwordSaveSuccess, true); 
                $(this._passForm).hide();
                $(this._ok).show();
                //this._result.show();
            }
            else {
                bootbox.alert("<h2>" + this._messages.titleError + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + data.ErrorMessage + "</pre>");
            }
        }
    }
}