using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.Common
{
    public class AccessDeniedModel: WikiSABaseModel
    {
        public MvcHtmlString Description { get; set; }
    }
}