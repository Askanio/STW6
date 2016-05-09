using System;

namespace ScrewTurn.Wiki.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfigReaderWriter
    {
        /// <summary>
        /// Gets the current application settings.
        /// </summary>
        /// <returns>A new <see cref="ApplicationSettings"/> instance</returns>
        ApplicationSettings GetApplicationSettings();

        /// <summary>
        /// Saves the configuration settings. This will save a subset of the <see cref="ApplicationSettings"/> based on 
        /// the values that match those found in the <see cref="ScrewTurnWikiSection"/>
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <exception cref="Exception">An exception occurred while updating the settings.</exception>
        void Save(ApplicationSettings settings);

        /// <summary>
        /// Resets the state the configuration file/store so the 'installed' property is false.
        /// </summary>
        /// <exception cref="Exception">An exception occurred while resetting web.config install state to false.</exception>
        void ResetInstalledState();

        /// <summary>
        /// Tests the app.config or web.config file to ensure that it can be written to.
        /// </summary>
        /// <returns>
        /// An empty string if no error occurred; otherwise the error message.
        /// </returns>
        string TestSaveWebConfig();

        /// <summary>
        /// Checks exists the public pirectory and attempts to save same file to it
        /// </summary>
        /// <returns>
        /// An empty string if no error occurred; otherwise the error message.
        /// </returns>
        string TestPublicDirectory(string publicDirectory);
    }
}
