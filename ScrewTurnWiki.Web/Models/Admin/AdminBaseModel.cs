using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code;

namespace ScrewTurn.Wiki.Web.Models.Admin
{
    public class AdminBaseModel : WikiBaseModel
    {
        public AdminBaseModel()
        {
            MenuCssClass = new Dictionary<AdminMenu, string>();
        }

        public bool HomeAvailable { get; set; }

        public bool GroupsAvailable { get; set; }

        public bool UsersAvailable { get; set; }

        public bool NamespacesAvailable { get; set; }

        public bool PagesAvailable { get; set; }

        public bool CategoriesAvailable { get; set; }

        public bool SnippetsAvailable { get; set; }

        public bool NavPathsAvailable { get; set; }

        public bool ContentAvailable { get; set; }

        public bool PluginsAvailable { get; set; }

        public bool ConfigAvailable { get; set; }

        public bool ThemeAvailable { get; set; }

        public bool GlobalHomeAvailable { get; set; }

        public bool GlobalConfigAvailable { get; set; }

        public bool ProvidersManagementAvailable { get; set; }

        public bool ImportExportAvailable { get; set; }

        public bool LogAvailable { get; set; }

        public Dictionary<AdminMenu, string> MenuCssClass { get; set; } 

        public string HomeDisplayStyle { get; set; }


        [AllowHtml]
        public string ResultText { get; set; }

        public string ResultCss { get; set; }
    }
}