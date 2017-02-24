/// <reference path="../typescript-ref/commonconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Common;
            (function (Common) {
                var HistoryPage = (function () {
                    function HistoryPage(wizard, page) {
                        this.wizard = wizard;
                        this.page = page;
                    }
                    HistoryPage.prototype.bindEvents = function () {
                        var _this = this;
                        $('#LinkToCompare').click(function (e) {
                            _this.hrefToCompare();
                        });
                        $(".rollback").click(function (e) {
                            _this.wizard.requestConfirm();
                        });
                    };
                    HistoryPage.prototype.hrefToCompare = function () {
                        var rev1 = $('#lstRev1').val();
                        var rev2 = $('#lstRev2').val();
                        var path = "/" + this.page + "/Diff?rev1=" + rev1 + "&rev2=" + rev2;
                        //$(this).attr("href", path);
                        window.location.href = path;
                    };
                    return HistoryPage;
                }());
                Common.HistoryPage = HistoryPage;
            })(Common = Web.Common || (Web.Common = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
