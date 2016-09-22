using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class SearchController : PageController
    {

        public SearchController(ApplicationSettings appSettings) : base(appSettings)
        {
        }

        // GET: Search
        public ActionResult Search(string ns)
        {
            // TODO:
            return new ContentResult();
        }
    }
}