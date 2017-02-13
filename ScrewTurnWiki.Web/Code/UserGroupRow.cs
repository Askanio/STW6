using System;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Web.Code
{
    /// <summary>
    /// Represents a User Group for display purposes.
    /// </summary>
    public class UserGroupRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:UserGroupRow" /> class.
        /// </summary>
        /// <param name="group">The original group.</param>
        /// <param name="selected">A value indicating whether the user group is selected.</param>
        public UserGroupRow(UserGroup group, bool selected)
        {
            Name = group.Name;
            Description = group.Description;
            Provider = group.Provider.Information.Name;
            AdditionalClass = selected ? " selected" : "";
            Users = group.Users.Length;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        public string Provider { get; }

        /// <summary>
        /// Gets the additional CSS class.
        /// </summary>
        public string AdditionalClass { get; }

        /// <summary>
        /// Gets the users.
        /// </summary>
        public int Users { get; }
    }
}