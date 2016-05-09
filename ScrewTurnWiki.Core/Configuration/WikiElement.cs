using System.Configuration;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class WikiElement : SettingElementBase
    {
        private static readonly ConfigurationProperty _host = new ConfigurationProperty("host", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties;

        /// <summary>
        /// 
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [StringValidator(InvalidCharacters = " ~!@#%^&*()[]{}/'\"|", MinLength = 3)]
        public override string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public WikiElement()
        {
            _properties = new ConfigurationPropertyCollection { _name, _host };
        }

        /// <summary>
        /// 
        /// </summary>
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }
    }
}
