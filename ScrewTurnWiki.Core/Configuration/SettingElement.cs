using System.Configuration;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class SettingElement : SettingElementBase
    {
        private static readonly ConfigurationProperty _value = new ConfigurationProperty("value", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
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
        public SettingElement()
        {
            _properties = new ConfigurationPropertyCollection { _name, _value };
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
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    }
}
