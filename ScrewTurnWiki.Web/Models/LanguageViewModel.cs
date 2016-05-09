using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrewTurn.Wiki.Web.Models
{
    public class LanguageViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public LanguageViewModel(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static IEnumerable<LanguageViewModel> SupportedLocales()
        {
            List<LanguageViewModel> languages = new List<LanguageViewModel>()
            {
                new LanguageViewModel("en", "English"),
                new LanguageViewModel("ru", "Pусский"),
            };

            return languages;
        }
    }
}