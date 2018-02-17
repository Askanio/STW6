using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ScrewTurn.Wiki.Web.Localization.Common;

namespace ScrewTurn.Wiki.Web.Models.Edit
{
    public class EditModel : WikiSABaseModel
    {
        // http://peterkeating.co.uk/using-ckeditor-with-razor-for-net-mvc3/
        public bool DisplayCaptcha { get; set; }

        public bool AutoTemplateVisible { get; set; }

        public HtmlString AutoTemplateLabel { get; set; }

        public HtmlString EditNotice { get; set; }

        public string Keywords { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        [Required(ErrorMessageResourceName = "RfvName_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Common.Edit))]
        public HtmlString Name { get; set; }

        public bool NameEnabled { get; set; }

        public bool ManualNameVisible { get; set; }

        [Required(ErrorMessageResourceName = "RfvTitle_ErrorMessage", ErrorMessageResourceType = typeof(Localization.Common.Edit))]
        public HtmlString PageTitle { get; set; }

        public bool PageNameVisible { get; set; }

        public bool MinorChange{ get; set; }

        public bool MinorChangeVisible { get; set; }

        public bool MinorChangeEnabled { get; set; }

        public bool SaveAsDraft { get; set; }

        public bool SaveAsDraftVisible { get; set; }

        public bool SaveAsDraftEnabled { get; set; }
    }
}