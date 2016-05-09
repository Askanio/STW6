/// <reference path="../typescript-ref/installerconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Installer;
            (function (Installer) {
                var Step3Messages = (function () {
                    function Step3Messages() {
                    }
                    return Step3Messages;
                }());
                Installer.Step3Messages = Step3Messages;
                var Step3 = (function () {
                    function Step3(wizard, messages) {
                        this._tips = $(".validateTips");
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
                            this.AddUser();
                        });
                        Step3.instance = this;
                    }
                    Step3.prototype.bindEvents = function () {
                        $("#create-wiki").click(function (e) {
                            Step3.instance._dialog.dialog("open");
                        });
                    };
                    Step3.prototype.addWiki = function () {
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
                    };
                    Step3.prototype.updateTips = function (t) {
                        var that = Step3.instance;
                        that._tips
                            .text(t)
                            .addClass("bg-danger");
                        setTimeout(function () {
                            that._tips.removeClass("bg-danger", 1500);
                        }, 500);
                    };
                    Step3.prototype.checkLength = function (o, n, min, max) {
                        var that = Step3.instance;
                        if (o.val().length > max || o.val().length < min) {
                            o.addClass("has-error");
                            that.updateTips(that._messages.checkLengthError1 + " " + n + " " + that._messages.checkLengthError2 + " " +
                                min + " " + that._messages.checkLengthError3 + " " + max + ".");
                            return false;
                        }
                        else {
                            return true;
                        }
                    };
                    Step3.prototype.checkRegexp = function (o, regexp, n) {
                        if (!(regexp.test(o.val()))) {
                            o.addClass("has-error");
                            Step3.instance.updateTips(n);
                            return false;
                        }
                        else {
                            return true;
                        }
                    };
                    return Step3;
                }());
                Installer.Step3 = Step3;
            })(Installer = Web.Installer || (Web.Installer = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
//# sourceMappingURL=step3.js.map