/// <reference path="../typescript-ref/installerconstants.ts" />
module ScrewTurn.Wiki.Web.Common {

    export class WikiPage {
        private wizard: CommonWizard;
        private showTimer: any;
        private hideTimer: any;
        private attachmentsMenuJustShown: boolean;
        private adminToolsMenuJustShown: boolean;
        private elem: any;

        constructor(wizard: CommonWizard) {
            this.wizard = wizard;
            this.showTimer = null;
            this.hideTimer = null;
            this.attachmentsMenuJustShown = false;
            this.adminToolsMenuJustShown = false;

            // Hide attachments and admin tools divs
            // This is needed because RepositionDiv cannot calculate the width of the element when it's hidden
            this.elem = document.getElementById("PageAttachmentsDiv");
            if (document.getElementById("PageAttachmentsLink")) {
                this.repositionDiv(document.getElementById("PageAttachmentsLink"), this.elem);
            }
            this.elem.style["display"] = "none";

            this.elem = document.getElementById("AdminToolsDiv");
            if (document.getElementById("AdminToolsLink")) {
                this.repositionDiv(document.getElementById("AdminToolsLink"), this.elem);
            }
            this.elem.style["display"] = "none";

            this.initBct();
        }

        public bindEvents() {
            //document.body.onclick = this.hideAllMenus;
            $(document.body).click((e) => {
                this.hideAllMenus();
            });

            $("#PageAttachmentsLink").click((e) => {
                this.toggleAttachmentsMenu(e.clientX, e.clientY);
            });
            $("#AdminToolsLink").click((e) => {
                this.toggleAdminToolsMenu(e.clientX, e.clientY);
            });
            $("#AdminPagesLink").click((e) => {
                this.wizard.requestConfirm();
            });
            $("#ShowAllTrailLink").click((e) => {
                this.showAllTrail();
            });
            $("#HideTrailLink").click((e) => {
                this.hideTrail();
            });

            var self = this;

            $("div[id^='BreadcrumbsDiv'").children('a').each(function(i) {
                if ($(this).hasClass("breadcrumbdropdown") && this.id) {
                    var id = this.id.replace("lnk", "");
                    $(this).mouseover((e) => {
                        self.showDropDown(e, id, this);
                    });
                    $(this).mouseout((e) => {
                        self.hideDropDown(id);
                    });

                    var element = $('div#' + id);
                    if (element) {
                        $(element).mouseover((e) => {
                            self.cancelHideTimer();
                        });
                        $(element).mouseout((e) => {
                            self.hideDropDown(id);
                        });
                    }
                }
            });
        }

        private showAllTrail() {
            try {
                document.getElementById("BreadcrumbsDivMin").style["display"] = "none";
                document.getElementById("BreadcrumbsDivAll").style["display"] = "";
                this.setStatus("1");
            } catch (ex) {
            }
            //return false;
        }

        private hideTrail() {
            try {
                document.getElementById("BreadcrumbsDivMin").style["display"] = "";
                document.getElementById("BreadcrumbsDivAll").style["display"] = "none";
                this.setStatus("0");
            } catch (ex) {
            }
            //return false;
        }

        private setStatus(open) {
            this.wizard.createCookie("ScrewTurnWikiBCT", open, 365);
        }

        private getStatus(): string {
            var value = this.wizard.readCookie("ScrewTurnWikiBCT");
            if (value) return value;
            else return "0";
        }

        private initBct() {
            if (this.getStatus() == "1") {
                this.showAllTrail();
            }
        }

        //var ie7Mode = false;

        private toggleAttachmentsMenu(cx: any, cy: any) {
            var element = document.getElementById("PageAttachmentsDiv");
            if (element) {
                if (element.style["display"] == "none") {
                    element.style["display"] = "";
                    //var pos = this.absolutePosition(element);
                    //if (pos.left - cx > 0) {
                    //    ie7Mode = true;
                    //    element.style["position"] = "absolute";
                    //    element.style["top"] = cy + "px";
                    //    element.style["left"] = (cx - pos.width) + "px";
                    //} else {
                        this.repositionDiv(document.getElementById("PageAttachmentsLink"), element);
                    //}
                    this.attachmentsMenuJustShown = true;
                }
            }
            //return false;
        }

        private hideAttachmentsMenu(): boolean {
            var element = document.getElementById("PageAttachmentsDiv");
            if (element && !this.attachmentsMenuJustShown) {
                element.style["display"] = "none";
                //if (ie7Mode) element.style["left"] = "10000px";
            }
            this.attachmentsMenuJustShown = false;
            return true; // Needed to enabled next clicks' action (file download)
        }

        private toggleAdminToolsMenu(cx: any, cy: any) {
            var element = document.getElementById("AdminToolsDiv");
            if (element) {
                if (element.style["display"] == "none") {
                    element.style["display"] = "";
                    //var pos = this.absolutePosition(element);
                    //if (pos.left - cx > 0) {
                    //    ie7Mode = true;
                    //    element.style["position"] = "absolute";
                    //    element.style["top"] = cy + "px";
                    //    element.style["left"] = (cx - pos.width) + "px";
                    //} else {
                        this.repositionDiv(document.getElementById("AdminToolsLink"), element);
                    //}
                        this.adminToolsMenuJustShown = true;
                }
            }
            //return false;
        }

        private hideAdminToolsMenu(): boolean {
            var element = document.getElementById("AdminToolsDiv");
            if (element && !this.adminToolsMenuJustShown) {
                element.style["display"] = "none";
                //if (ie7Mode) element.style["left"] = "10000px";
            }
            this.adminToolsMenuJustShown = false;
            return true; // Needed to enable next clicks' action (admin tools)
        }

        private hideAllMenus() {
            this.hideAttachmentsMenu();
            this.hideAdminToolsMenu();
        }

        private absolutePosition(obj) {
            var pos = null;
            if (obj != null) {
                pos = new Object();
                pos.top = obj.offsetTop;
                pos.left = obj.offsetLeft;
                pos.width = obj.offsetWidth;
                pos.height = obj.offsetHeight;

                obj = obj.offsetParent;
                while (obj != null) {
                    pos.top += obj.offsetTop;
                    pos.left += obj.offsetLeft;
                    obj = obj.offsetParent;
                }
            }
            return pos;
        }

        private showDropDown(e, divId, parent) {
            // Set a timer
            // On mouse out, cancel the timer and start a 2nd timer that hides the menu
            // When the 1st timer elapses
            //   show the drop-down
            //   on menu mouseover, cancel the 2nd timer
            //   on menu mouse out, hide the menu
            var self = this;
            this.hideTimer = setTimeout(function () {
                self.showDropDownForReal(e.clientX, e.clientY, divId, parent.id);
            }, 200);
            //this.showTimer = setTimeout('this.showDropDownForReal(' + e.clientX + ', ' + e.clientY + ', "' + divId + '", "' + parent.id + '");', 200);
        }

        public showDropDownForReal(cx, cy, divId, parentId) {
            var pos = this.absolutePosition(document.getElementById(parentId));
            var menu = document.getElementById(divId);

            // This is needed to trick IE7 which, for some reason,
            // does not position the drop-down correctly with the new Default theme
            if (pos.left - cx > 30) {
                menu.style["display"] = "";
                menu.style["position"] = "absolute";
                menu.style["top"] = cy + "px";
                menu.style["left"] = (cx - 10) + "px";
            } else {
                menu.style["display"] = "";
                menu.style["position"] = "absolute";
                menu.style["top"] = (pos.top + pos.height) + "px";
                menu.style["left"] = pos.left + "px";
            }
            this.showTimer = null;
        }

        private hideDropDown(divId) {
            if (this.showTimer) clearTimeout(this.showTimer);
            var self = this;
            this.hideTimer = setTimeout(function () {
                self.hideDropDownForReal(divId);
            }, 200);
            //this.hideTimer =  setTimeout('this.hideDropDownForReal("' + divId + '");', 200);
        }

        private hideDropDownForReal(divId) {
            document.getElementById(divId).style["display"] = "none";
            this.hideTimer = null;
        }

        private cancelHideTimer() {
            if (this.hideTimer) clearTimeout(this.hideTimer);
        }

        private repositionDiv(link, element) {
            var absPos = this.absolutePosition(link);
            var elemAbsPos = this.absolutePosition(element);

            element.style["top"] = (absPos.top + absPos.height) + "px";
            element.style["left"] = (absPos.left - (elemAbsPos.width - absPos.width)) + "px";
            element.style["position"] = "absolute";
        }

    }
}