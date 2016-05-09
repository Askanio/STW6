using System.Configuration;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SettingElementCollection<T> : ConfigurationElementCollection where T : SettingElementBase,  new()
    {
        #region Overrides of ConfigurationElementCollection
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((T)element).Name;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return (T)BaseGet(index); }
            set { if (BaseGet(index) != null) BaseRemoveAt(index); BaseAdd(index, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Get(string name)
        {
            return (T)BaseGet(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public void Add(T element)
        {
            BaseAdd(element);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public void Remove(T element)
        {
            BaseRemove(element.Name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            BaseRemoveAt(index);
        }
    }
}
