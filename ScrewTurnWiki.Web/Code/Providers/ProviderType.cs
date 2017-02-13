using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrewTurn.Wiki.Web.Code.Providers
{
    /// <summary>
    /// Lists legal provider types.
    /// </summary>
    public enum ProviderType
    {
        /// <summary>
        /// Users storage providers.
        /// </summary>
        Users,
        /// <summary>
        /// Pages storage providers.
        /// </summary>
        Pages,
        /// <summary>
        /// Files storage providers.
        /// </summary>
        Files,
        /// <summary>
        ///  Theme providers.
        /// </summary>
        Themes
    }
}