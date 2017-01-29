using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ScrewTurn.Wiki.Web.Models.User
{
    public class LanguageModel : WikiSABaseModel
    {

        public string SelectedLanguage { get; set; }

        public string SelectedTimezone { get; set; }

        public List<SelectListItem> Languages { get; set; }

        public List<SelectListItem> Timezones { get; set; }
    }
}