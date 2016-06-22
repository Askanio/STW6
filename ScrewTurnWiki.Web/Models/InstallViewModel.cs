using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.Web.Localization.Install;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.Web.Code.Attributes;

namespace ScrewTurn.Wiki.Web.Models
{
    /// <summary>
    /// Represents settings for the site, some of which are stored in the web.config.
    /// </summary>
    [Serializable]
    public class InstallViewModel
    {
        public InstallViewModel()
        {
            // Default values
            PublicDirectory = "public";
            AdvancedSettings = false;
            Wikis = new Dictionary<string, string> ();
        }

        public string LanguageCode { get; set; }

        [Required(ErrorMessageResourceType = typeof(InstallStrings), ErrorMessageResourceName = "ValidationPublicDirectoryEmpty")]
        [StringLength(50)]
        public string PublicDirectory { get; set; }

        public bool AdvancedSettings { get; set; }

        public string ConnectionSchemeName { get; set; }

        public string ConnectionString { get; set; }


        public bool Installed { get; set; }

        public bool NeedMasterPassword { get; set; }

        [WikisValidator]
        public Dictionary<string, string> Wikis { get; set; }

        /// <summary>
        /// Gets an IEnumerable{SelectListItem} from a the InstallViewModel.ConnectionSchemesAvailable, as a default
        /// SelectList doesn't add option value attributes.
        /// </summary>
        public List<SelectListItem> ConnectionSchemesAsSelectList
        {
            get
            {
                List<SelectListItem> items = new List<SelectListItem>();

                foreach (string name in ConnectionSchemesAvailable)
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = name;
                    item.Value = name;

                    if (name == ConnectionSchemeName)
                        item.Selected = true;

                    items.Add(item);
                }

                return items;
            }
        }

        private IEnumerable<string> ConnectionSchemesAvailable
        {
            get
            {
                return ConnectionScheme.AllSchemes.Select(x => x.Name);
            }
        }
    }
}