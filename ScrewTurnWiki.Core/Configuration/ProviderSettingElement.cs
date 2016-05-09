using System.Configuration;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class ProviderSettingElement : SettingElementBase
    {
        private static readonly ConfigurationProperty _assembly = new ConfigurationProperty("assembly", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _config = new ConfigurationProperty("paramsConfig", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _isDefault = new ConfigurationProperty("isDefault", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
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
        public ProviderSettingElement()
        {
            _properties = new ConfigurationPropertyCollection { _name, _assembly, _config, _isDefault };
        }

        /// <summary>
        /// 
        /// </summary>
        [StringValidator(InvalidCharacters = " ~!@#%^&*()[]{}/'\"|", MinLength = 3)]
        public string Assembly
        {
            get { return (string)this["assembly"]; }
            set { this["assembly"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [StringValidator(InvalidCharacters = " ~!@#%^&*()[]{}/'\"|", MinLength = 3)]
        public string Config
        {
            get { return (string)this["paramsConfig"]; }
            set { this["paramsConfig"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDefault
        {
            get {
                bool result = false;
                bool.TryParse((string)this["isDefault"], out result);
                return result;
            }
            set { this["isDefault"] = value.ToString(); }
        }
    }
}
