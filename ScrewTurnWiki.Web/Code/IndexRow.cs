using System;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code
{
    /// <summary>
    /// Represents the status of a search engine index.
    /// </summary>
    public class IndexRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:IndexRow" /> class.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="provider">The original provider.</param>
        public IndexRow(string command, IStorageProviderV60 provider)
        {
            this.Command = command;
            this.Provider = provider.Information.Name;
            ProviderType = provider.GetType().FullName;
        }

        public string Command { get; }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        public string Provider { get; }

        /// <summary>
        /// Gets the provider type.
        /// </summary>
        public string ProviderType { get; }
    }
}