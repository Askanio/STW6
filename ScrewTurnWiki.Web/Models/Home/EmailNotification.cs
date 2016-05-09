using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrewTurn.Wiki.Web.Models.Home
{
    public class EmailNotification
    {
        public string PageFullName { get; set; }

        public bool DiscussMode { get; set; }

        public string CssClass { get; set; }

        public string ToolTip { get; set; }
    }
}