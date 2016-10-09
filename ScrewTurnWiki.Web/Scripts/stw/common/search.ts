/// <reference path="../typescript-ref/commonconstants.ts" />
module ScrewTurn.Wiki.Web.Common {

    export class Search {
        private wizard: CommonWizard;

        constructor(wizard: CommonWizard) {
            this.wizard = wizard;

            this.toggleCategoriesList();
        }

        public bindEvents() {
            $('#IsAllNamespaces').click((e) => {
                this.toggleCategoriesList();
            });

            $('#SelectAll').click((e) => {
                this.selectAll();
            });

            $('#SelectNone').click((e) => {
                this.selectNone();
            });

            $('#SelectInverse').click((e) => {
                this.selectInverse();
            });
        }

        private toggleCategoriesList() {
            var chk = $('#IsAllNamespaces');
            var filter = $('#CategoryFilterDiv');
            if (chk.prop("checked")) filter.attr("style", "display: none;");
            else filter.removeAttr("style");;
        }

        private selectAll() {
            $('#CategoryFilterInternalDiv input').each(function() { $(this).get(0).checked = true; });
            return false;
        }

        private selectNone() {
            $('#CategoryFilterInternalDiv input').each(function () { $(this).get(0).checked = false; });
            return false;
        }

        private selectInverse() {
            $('#CategoryFilterInternalDiv input').each(function () { $(this).get(0).checked = !$(this).get(0).checked; });
            return false;
        }

    }
}