/// <reference path="../typescript-ref/commonconstants.ts" />
var ScrewTurn;
(function (ScrewTurn) {
    var Wiki;
    (function (Wiki) {
        var Web;
        (function (Web) {
            var Common;
            (function (Common) {
                var Search = (function () {
                    function Search(wizard) {
                        this.wizard = wizard;
                        this.toggleCategoriesList();
                    }
                    Search.prototype.bindEvents = function () {
                        var _this = this;
                        $('#IsAllNamespaces').click(function (e) {
                            _this.toggleCategoriesList();
                        });
                        $('#SelectAll').click(function (e) {
                            _this.selectAll();
                        });
                        $('#SelectNone').click(function (e) {
                            _this.selectNone();
                        });
                        $('#SelectInverse').click(function (e) {
                            _this.selectInverse();
                        });
                    };
                    Search.prototype.toggleCategoriesList = function () {
                        var chk = $('#IsAllNamespaces');
                        var filter = $('#CategoryFilterDiv');
                        if (chk.prop("checked"))
                            filter.attr("style", "display: none;");
                        else
                            filter.removeAttr("style");
                        ;
                    };
                    Search.prototype.selectAll = function () {
                        $('#CategoryFilterInternalDiv input').each(function () { $(this).get(0).checked = true; });
                        return false;
                    };
                    Search.prototype.selectNone = function () {
                        $('#CategoryFilterInternalDiv input').each(function () { $(this).get(0).checked = false; });
                        return false;
                    };
                    Search.prototype.selectInverse = function () {
                        $('#CategoryFilterInternalDiv input').each(function () { $(this).get(0).checked = !$(this).get(0).checked; });
                        return false;
                    };
                    return Search;
                }());
                Common.Search = Search;
            })(Common = Web.Common || (Web.Common = {}));
        })(Web = Wiki.Web || (Wiki.Web = {}));
    })(Wiki = ScrewTurn.Wiki || (ScrewTurn.Wiki = {}));
})(ScrewTurn || (ScrewTurn = {}));
