using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web;

namespace ScrewTurn.Wiki.Web.Code
{
    /// <summary>
    /// Implements a Resource Exchanger.
    /// </summary>
    public class ResourceExchanger : IResourceExchanger
    {

        private ResourceManager manager;

        /// <summary>
        /// Initialises a new instance of the <b>ResourceExchanger</b> class.
        /// </summary>
        public ResourceExchanger()
        {
            manager = new ResourceManager("ScrewTurn.Wiki.Web.Localization.Messages.Messages", typeof(Localization.Messages.Messages).Assembly);
        }

        /// <summary>
        /// Gets a Resource String.
        /// </summary>
        /// <param name="name">The Name of the Resource.</param>
        /// <returns>The Resource String.</returns>
        public string GetResource(string name)
        {
            return manager.GetString(name);
        }

    }
}