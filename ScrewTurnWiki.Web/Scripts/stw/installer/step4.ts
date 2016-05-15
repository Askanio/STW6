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
        private wizard: InstallWizard;
        private messages: Step4Messages;
        private needMasterPassword: boolean;
        private passForm = $("#setPassword");
        private ok = $("#completed");
        private result = $("#result");
        private pass = $("#newPwd");
        private rePass = $("#reNewPwd");

        constructor(wizard: InstallWizard, messages: Step4Messages, needMasterPassword: boolean) {
            this.wizard = wizard;
            this.messages = messages;
            this.needMasterPassword = needMasterPassword;

            if (this.needMasterPassword) {
                $(this.passForm).show();
                $(this.ok).hide();
                //this._result.hide();
            } else {
                $(this.passForm).hide();
                $(this.ok).show();
                //this._result.hide(); 
            }
        }

        private checkPassword() {
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
        }

        private updateTips(t: string, success: boolean) {
            $(this.result).html(t);
            if (success)
                $(this.result).addClass("test-success");
            else
                $(this.result).addClass("test-error");
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
                    "password": this.pass.val()
                };

            this.wizard.makeAjaxRequest(url, jsonData, (data: any) => { this.OnSavePasswordSuccess(data); });
        }

        private OnSavePasswordSuccess(data: any) {
            if (data.Success) {
                this.updateTips(this.messages.passwordSaveSuccess, true); 
                $(this.passForm).hide();
                $(this.ok).show();
                //this._result.show();
            }
            else {
                bootbox.alert("<h2>" + this.messages.titleError + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + data.ErrorMessage + "</pre>");
            }
        }
    }
}