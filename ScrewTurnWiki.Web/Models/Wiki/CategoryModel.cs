using System;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Wiki
{
    public class CategoryModel : WikiPageModel
    {
        [AllowHtml]
        public MvcHtmlString CatList { get; set; }
    }
}