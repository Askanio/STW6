using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.Web.Code.Attributes;

namespace ScrewTurn.Wiki.Web.Controllers.Admin
{
    [RoutePrefix("Admin")]
    //[CheckActionForGlobals(Action = CheckActionForGlobalsAttribute.ActionForGlobals.ManageConfiguration)]
    public class UsersController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public UsersController(ApplicationSettings settings) : base(settings)
        {
        }

        // GET: AdminHome
        public ActionResult Index()
        {
            return new ContentResult();
        }
    }
}