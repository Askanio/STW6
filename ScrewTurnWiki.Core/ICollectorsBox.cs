
using System;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki {

	/// <summary>
	/// Contains the interface of the Providers Collectors Box.
	/// </summary>
	public interface ICollectorsBox {

		/// <summary>
		/// The global settings storage provider.
		/// </summary>
		/// <returns>The globalSettingsProvider.</returns>
		IGlobalSettingsStorageProviderV60 GlobalSettingsProvider { get; }

		/// <summary>
		/// Gets the settings provider.
		/// </summary>
		/// <param name="wiki">The wiki.</param>
		/// <returns>The settingsProvider initialized for the given wiki.</returns>
		ISettingsStorageProviderV60 GetSettingsProvider(string wiki);

		/// <summary>
		/// The index directory provider.
		/// </summary>
		/// <param name="wiki">The wiki.</param>
		/// <returns>The indexDirectoryProvider initialized for the given wiki.</returns>
		IIndexDirectoryProviderV60 GetIndexDirectoryProvider(string wiki);

		/// <summary>
		/// Gets the files provider collector.
		/// </summary>
		ProviderCollector<IFilesStorageProviderV60> FilesProviderCollector { get; }

		/// <summary>
		/// Gets the formatter provider collector.
		/// </summary>
		ProviderCollector<IFormatterProviderV50> FormatterProviderCollector { get; }

		/// <summary>
		/// Gets the pages provider collector.
		/// </summary>
		ProviderCollector<IPagesStorageProviderV60> PagesProviderCollector { get; }

		/// <summary>
		/// Gets the theme provider collector.
		/// </summary>
		ProviderCollector<IThemesStorageProviderV60> ThemesProviderCollector { get; }

		/// <summary>
		/// Gets the users provider collector.
		/// </summary>
		ProviderCollector<IUsersStorageProviderV60> UsersProviderCollector { get; }

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		void Dispose();
		
	}
}
