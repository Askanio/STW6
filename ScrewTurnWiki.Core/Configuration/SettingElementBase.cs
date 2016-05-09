using System.Configuration;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SettingElementBase : ConfigurationElement
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract string Name { get; set; }
    }
}
