
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki {
	/// <summary>
	/// Contains instances of the Providers Collectors.
	/// </summary>
	public class CollectorsBox : IDisposable, ICollectorsBox {

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectorsBox"/> class.
		/// </summary>
		/// <param name="globalSettingsProvider">The global settings provider Type.</param>
		/// <param name="globalSettingsProviderAssembly">The global settings provider assembly.</param>
		/// <param name="settingsProvider">The settings provider Type.</param>
		/// <param name="settingsProviderAssembly">The settings provider assembly.</param>
		/// <param name="indexDirectoryProvider">The indexDirectory provider Type.</param>
		/// <param name="indexDirectoryProviderAssembly">The indexDirectory provider assembly.</param>
		/// <param name="usersProviderCollector">The users provider collector.</param>
		/// <param name="themeProviderCollector">The theme provider collector.</param>
		/// <param name="pagesProviderCollector">The pages provider collector.</param>
		/// <param name="filesProviderCollector">The files provider collector.</param>
		/// <param name="formatterProviderCollector">The formatter provider collector.</param>
		public CollectorsBox(Type globalSettingsProvider,
			System.Reflection.Assembly globalSettingsProviderAssembly,
			Type settingsProvider,
			System.Reflection.Assembly settingsProviderAssembly,
			Type indexDirectoryProvider,
			System.Reflection.Assembly indexDirectoryProviderAssembly,
			ProviderCollector<IUsersStorageProviderV60> usersProviderCollector,
			ProviderCollector<IThemesStorageProviderV60> themeProviderCollector,
			ProviderCollector<IPagesStorageProviderV60> pagesProviderCollector,
			ProviderCollector<IFilesStorageProviderV60> filesProviderCollector,
			ProviderCollector<IFormatterProviderV50> formatterProviderCollector) {

			_globalSettingsProviderType = globalSettingsProvider;
			_globalSettingsProviderAssembly = globalSettingsProviderAssembly;
			_settingsProviderType = settingsProvider;
			_settingsProviderAssembly = settingsProviderAssembly;
			_settingsProvider = new Dictionary<string, ISettingsStorageProviderV60>();
			_indexDirectoryProviderType = indexDirectoryProvider;
			_indexDirectoryProviderAssembly = indexDirectoryProviderAssembly;
			_indexDirectoryProvider = new Dictionary<string, IIndexDirectoryProviderV60>();
			_usersProviderCollector = usersProviderCollector;
			_themesProviderCollector = themeProviderCollector;
			_pagesProviderCollector = pagesProviderCollector;
			_filesProviderCollector = filesProviderCollector;
			_formatterProviderCollector = formatterProviderCollector; ;
		}

		private Type _globalSettingsProviderType;
		private System.Reflection.Assembly _globalSettingsProviderAssembly;
		private IGlobalSettingsStorageProviderV60 _globalSettingsProvider;

		private Type _settingsProviderType;
		private System.Reflection.Assembly _settingsProviderAssembly;
		private Dictionary<string, ISettingsStorageProviderV60> _settingsProvider;

		private Type _indexDirectoryProviderType;
		private System.Reflection.Assembly _indexDirectoryProviderAssembly;
		private Dictionary<string, IIndexDirectoryProviderV60> _indexDirectoryProvider;

		private ProviderCollector<IUsersStorageProviderV60> _usersProviderCollector;
		private ProviderCollector<IThemesStorageProviderV60> _themesProviderCollector;
		private ProviderCollector<IPagesStorageProviderV60> _pagesProviderCollector;
		private ProviderCollector<IFilesStorageProviderV60> _filesProviderCollector;
		private ProviderCollector<IFormatterProviderV50> _formatterProviderCollector;

		/// <summary>
		/// The global settings storage provider.
		/// </summary>
		/// <returns>The globalSettingsProvider.</returns>
		public IGlobalSettingsStorageProviderV60 GlobalSettingsProvider {
			get {
				if(_globalSettingsProvider == null) {
					_globalSettingsProvider = ProviderLoader.CreateInstance<IGlobalSettingsStorageProviderV60>(_globalSettingsProviderAssembly, _globalSettingsProviderType);
					_globalSettingsProvider.Init(Host.Instance, StartupTools.GetGlobalSettingsStorageProviderConfiguration(), null);
				}
				return _globalSettingsProvider;
			}
		}

		/// <summary>
		/// The settings storage provider.
		/// </summary>
		/// <param name="wiki">The wiki.</param>
		/// <returns>The settingsProvider initialized for the given wiki.</returns>
		public ISettingsStorageProviderV60 GetSettingsProvider(string wiki) {
			string wikiKey = wiki != null ? wiki : "-";
			if(!_settingsProvider.ContainsKey(wikiKey)) {
				_settingsProvider[wikiKey] = ProviderLoader.CreateInstance<ISettingsStorageProviderV60>(_settingsProviderAssembly, _settingsProviderType);
				_settingsProvider[wikiKey].Init(Host.Instance, ProviderLoader.LoadStorageProviderConfiguration(_settingsProviderType.FullName), wiki);
			}
			return _settingsProvider[wikiKey];
		}

		/// <summary>
		/// The index directory provider.
		/// </summary>
		/// <param name="wiki">The wiki.</param>
		/// <returns>The indexDirectoryProvider initialized for the given wiki.</returns>
		public IIndexDirectoryProviderV60 GetIndexDirectoryProvider(string wiki) {
			string wikiKey = wiki != null ? wiki : "-";
			if(!_indexDirectoryProvider.ContainsKey(wikiKey)) {
				_indexDirectoryProvider[wikiKey] = ProviderLoader.CreateInstance<IIndexDirectoryProviderV60>(_indexDirectoryProviderAssembly, _indexDirectoryProviderType);
				_indexDirectoryProvider[wikiKey].Init(Host.Instance, ProviderLoader.LoadStorageProviderConfiguration(_indexDirectoryProviderType.FullName), wiki);
			}
			return _indexDirectoryProvider[wikiKey];
		}

		/// <summary>
		/// The Users Provider Collector instance.
		/// </summary>
		public ProviderCollector<IUsersStorageProviderV60> UsersProviderCollector {
			get {
				return _usersProviderCollector;
			}
		}

		/// <summary>
		/// The Themes Provider Collector istance.
		/// </summary>
		public ProviderCollector<IThemesStorageProviderV60> ThemesProviderCollector {
			get {
				return _themesProviderCollector;
			}
		}

		/// <summary>
		/// The Pages Provider Collector instance.
		/// </summary>
		public ProviderCollector<IPagesStorageProviderV60> PagesProviderCollector {
			get {
				return _pagesProviderCollector;
			}
		}

		/// <summary>
		/// The Files Provider Collector instance.
		/// </summary>
		public ProviderCollector<IFilesStorageProviderV60> FilesProviderCollector {
			get {
				return _filesProviderCollector;
			}
		}

		/// <summary>
		/// The Formatter Provider Collector instance.
		/// </summary>
		public ProviderCollector<IFormatterProviderV50> FormatterProviderCollector {
			get {
				return _formatterProviderCollector;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			if(_settingsProvider != null) {
				foreach(var settingsProvider in _settingsProvider.Values) {
					settingsProvider.Dispose();
				}
				_settingsProvider.Clear();
			}
			if(_indexDirectoryProvider != null) {
				foreach(var IndexDirectoryProvider in _indexDirectoryProvider.Values) {
					IndexDirectoryProvider.Dispose();
				}
				_indexDirectoryProvider.Clear();
			}
			_usersProviderCollector.Dispose();
			_themesProviderCollector.Dispose();
			_pagesProviderCollector.Dispose();
			_filesProviderCollector.Dispose();
			_formatterProviderCollector.Dispose();
		}

	}
}
