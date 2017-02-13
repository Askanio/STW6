using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code
{
    /// <summary>
    /// Represents a missing or orphaned page.
    /// </summary>
    public class WantedPageRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PageRow" /> class.
        /// </summary>
        /// <param name="currentWiki">Current wiki</param>
        /// <param name="nspace">The namespace.</param>
        /// <param name="nspacePrefix">The namespace prefix.</param>
        /// <param name="name">The full name.</param>
        /// <param name="linkingPages">The pages that link the wanted page.</param>
        public WantedPageRow(string currentWiki, string nspace, string nspacePrefix, string name, List<string> linkingPages)
        {
            this.Nspace = nspace;
            this.NspacePrefix = nspacePrefix;
            this.Name = name;

            StringBuilder sb = new StringBuilder(100);
            for (int i = 0; i < linkingPages.Count; i++)
            {
                PageContent page = Pages.FindPage(currentWiki, linkingPages[i]);
                if (page != null)
                {
                    sb.AppendFormat(@"<a href=""/{0}{1}"" title=""{2}"" target=""_blank"">{2}</a>, ", page.FullName, GlobalSettings.PageExtension,
                        FormattingPipeline.PrepareTitle(currentWiki, page.Title, false, FormattingContext.Other, page.FullName));
                }
            }
            this.LinkingPages = sb.ToString().TrimEnd(' ', ',');
        }

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        public string Nspace { get; }

        /// <summary>
        /// Gets the namespace prefix.
        /// </summary>
        public string NspacePrefix { get; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the linker pages.
        /// </summary>
        public string LinkingPages { get; }
    }
}