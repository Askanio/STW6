/// <reference path="../typescript-ref/commonconstants.ts" />
module ScrewTurn.Wiki.Web.Common {

    export class HistoryPage {
        private wizard: CommonWizard;
        private page: string;

        constructor(wizard: CommonWizard, page: string) {
            this.wizard = wizard;
            this.page = page;
        }

        public bindEvents() {
            $('#LinkToCompare').click((e) => {
                this.hrefToCompare();
            });

            $(".rollback").click((e) => {
                this.wizard.requestConfirm();
            });
        }

        private hrefToCompare() {
            var rev1 = $('#lstRev1').val();
            var rev2 = $('#lstRev2').val();
            var path = "/" + this.page + "/Diff?rev1=" + rev1 + "&rev2=" + rev2;
            $(this).attr("href", path);
        }

    }
}