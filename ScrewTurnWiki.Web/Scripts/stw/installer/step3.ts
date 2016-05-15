/// <reference path="../typescript-ref/installerconstants.ts" />
module ScrewTurn.Wiki.Web.Installer {
    export class Step3Messages {
        public buttonDeleteTitle: string;
        public checkLengthError1: string;
        public checkLengthError2: string;
        public checkLengthError3: string;
        public wikinameExistsError: string;
        public wikinameInValidCharacters: string;
        public hostsInValidCharacters: string;
        public buttonCreateWikiTitle: string;
        public buttonCancelTitle: string;
        public wikiNameTitle: string;
        public hostsTitle: string;
    }

    export class Step3 {
        private _wizard: InstallWizard;
        private _messages: Step3Messages;
        private _dialog: any;
        private _wikiname: any;
        private _host: any;
        private _allFields: any;
        private _form: any;
        private _tips = $(".validateTips");
        public static instance: any;

        constructor(wizard: InstallWizard, messages: Step3Messages) {
            this._wizard = wizard;
            this._messages = messages;

            this._wikiname = $("#wikiname-edit");
            this._host = $("#host-edit");
            this._allFields = $([]).add(this._wikiname).add(this._host);

            this._dialog = $("#dialog-form").dialog({
                autoOpen: false,
                height: 300,
                width: 350,
                modal: true,
                buttons: [
                    {
                        text: this._messages.buttonCreateWikiTitle,
                        click: this.addWiki
                    },
                    {
                        text: this._messages.buttonCancelTitle,
                        click: function () {
                            $(this).dialog("close");
                        }
                    }
                ],
                close: function () {
                    Step3.instance._form[0].reset();
                    Step3.instance._allFields.removeClass("has-error");
                }
            });

            this._form = this._dialog.find("form").on("submit", function (event) {
                event.preventDefault();
                this.AddWiki();
            });

            Step3.instance = this;
        }

        public bindEvents() {
            $("#create-wiki").click((e) => {
                Step3.instance._dialog.dialog("open");
            });
        }

        private addWiki() {
            var that = Step3.instance;
            var valid = true;
            that._allFields.removeClass("has-error");

            valid = valid && that.checkLength(that._wikiname, that._messages.wikiNameTitle, 1, 50);
            valid = valid && that.checkLength(that._host, that._messages.hostsTitle, 3, 100);

            valid = valid && that.checkRegexp(that._wikiname, /^([^{}=<>~!@#"%;:&*/'`,\(\)\\[\]\\\.\^\$\|\?\+])+$/i, that._messages.wikinameInValidCharacters);
            valid = valid && that.checkRegexp(that._host, /^([^{}=<>~!@#"%:&*/'`,\(\)\\[\]\\\^\$\|\?\+])+$/i, that._messages.hostsInValidCharacters);

            $('#wikis tr').each(function () {
                var td = $(this).find('td:eq(0)').html();
                if (!!td && td === that._wikiname.val()) {
                    that._tips
                        .text(that._messages.wikinameExistsError)
                        .addClass("has-warning");
                    valid = false;
                }
            });

            if (valid) {
                $("#wikis tbody").append("<tr>" +
                    "<td>" + that._wikiname.val() + "</td>" +
                    "<td>" + that._host.val() + "<input type=\"hidden\" name=\"Wikis[" + that._wikiname.val() + "]\" value=\"" + that._host.val() + "\"/></td>" +
                    "<td><a href='#' onclick=\"$(this).parents('tr').remove(); return false;\" class=\"btn btn-xs btn-primary btn-danger\">" + that._messages.buttonDeleteTitle + "</a></td>" +
                    "</tr>");
                that._dialog.dialog("close");
            }
            return valid;
        }

        private updateTips(t: string) {
            var that = Step3.instance;
            that._tips
                .text(t)
                .addClass("bg-danger");
            setTimeout(function () {
                that._tips.removeClass("bg-danger", 1500);
            }, 500);
        }

        private checkLength(o: any, n: any, min: number, max: number): boolean {
            var that = Step3.instance;
            if (o.val().length > max || o.val().length < min) {
                o.addClass("has-error");
                that.updateTips(that._messages.checkLengthError1 + " " + n + " " + that._messages.checkLengthError2 + " " +
                    min + " " + that._messages.checkLengthError3 + " " + max + ".");
                return false;
            } else {
                return true;
            }
        }

        private checkRegexp(o, regexp, n) {
            if (!(regexp.test(o.val()))) {
                o.addClass("has-error");
                Step3.instance.updateTips(n);
                return false;
            } else {
                return true;
            }
        }

    }
}