
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using ScrewTurn.Wiki.PluginFramework;
using Microsoft.WindowsAzure.Storage;
using System.Data.Services.Client;
using ScrewTurn.Wiki.AclEngine;

namespace ScrewTurn.Wiki.Plugins.AzureStorage {

	/// <summary>
	/// Implements an Azure Table Storage settings storage provider.
	/// </summary>
	public class AzureSettingsStorageProvider : ISettingsStorageProviderV60 {

		private IHostV50 _host;
		private string _wiki;

        private CloudTableClient _cloudTableClient;

		private IAclManager _aclManager;

		private Dictionary<string, string> _settingsDictionary;

		// Cache of settings as dictionary settingKey -> settingValue
		private Dictionary<string, string> _settings {
			get {
				if(_settingsDictionary == null) {
					_settingsDictionary = new Dictionary<string, string>();
					IList<SettingsEntity> settingsEntities = GetSettingsEntities(_wiki);
					foreach(SettingsEntity settingsEntity in settingsEntities) {
						_settingsDictionary.Add(settingsEntity.RowKey, settingsEntity.Value + "");
					}
				}
				return _settingsDictionary;
			}
		}

		private IList<SettingsEntity> GetSettingsEntities(string wiki) {
            CloudTable table = _cloudTableClient.GetTableReference(SettingsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, wiki);
            TableQuery<SettingsEntity> query = (new TableQuery<SettingsEntity>()).Where(partitionKeyFilter);
            return table.ExecuteQuery<SettingsEntity>(query).ToList();
            //var query = (from e in _context.CreateQuery<SettingsEntity>(SettingsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals(wiki)
            //             select e).AsTableServiceQuery();
            //return QueryHelper<SettingsEntity>.All(query);
		}

		private SettingsEntity GetSettingsEntity(string wiki, string settingName) {
            CloudTable table = _cloudTableClient.GetTableReference(SettingsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, wiki);
            string rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, settingName);
            string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);
            TableQuery<SettingsEntity> query = (new TableQuery<SettingsEntity>()).Where(combinedFilter);
            return table.ExecuteQuery<SettingsEntity>(query).FirstOrDefault();
            //var query = (from e in _context.CreateQuery<SettingsEntity>(SettingsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals(wiki) && e.RowKey.Equals(settingName)
            //             select e).AsTableServiceQuery();
            //return QueryHelper<SettingsEntity>.FirstOrDefault(query);
		}

		/// <summary>
		/// Retrieves the value of a Setting.
		/// </summary>
		/// <param name="name">The name of the Setting.</param>
		/// <returns>The value of the Setting, or <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="name"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
		public string GetSetting(string name) {
			if(name == null) throw new ArgumentNullException("name");
			if(name.Length == 0) throw new ArgumentException("name");
			try {
				string val = null;
				if(_settings.TryGetValue(name, out val)) return val;
				else return null;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Stores the value of a Setting.
		/// </summary>
		/// <param name="name">The name of the Setting.</param>
		/// <param name="value">The value of the Setting. Value cannot contain CR and LF characters, which will be removed.</param>
		/// <returns>True if the Setting is stored, false otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="name"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
		public bool SetSetting(string name, string value) {
			if(name == null) throw new ArgumentNullException("name");
			if(name.Length == 0) throw new ArgumentException("name");

			_settingsDictionary = null;

			// Nulls are converted to empty strings
			if(value == null) value = "";

			SettingsEntity settingsEntity = GetSettingsEntity(_wiki, name);

            CloudTable table = _cloudTableClient.GetTableReference(SettingsTable);
            var batchOperation = new TableBatchOperation();

			if(settingsEntity == null) {
				settingsEntity = new SettingsEntity() {
					PartitionKey = _wiki,
					RowKey = name,
					Value = value
				};
                batchOperation.Insert(settingsEntity);
			}
			else {
				settingsEntity.Value = value;
                batchOperation.Replace(settingsEntity);
			}
            table.ExecuteBatch(batchOperation);
			return true;
		}

		/// <summary>
		/// Gets the all the setting values.
		/// </summary>
		/// <returns>All the settings.</returns>
		public IDictionary<string, string> GetAllSettings() {
			return _settings;
		}

		/// <summary>
		/// Starts a Bulk update of the Settings so that a bulk of settings can be set before storing them.
		/// </summary>
		public void BeginBulkUpdate() {
			// Nothing todo
		}

		/// <summary>
		/// Ends a Bulk update of the Settings and stores the settings.
		/// </summary>
		public void EndBulkUpdate() {
			// Nothing todo
		}

		// wiki -> (typeName -> (enabled, config))
		private Dictionary<string, Dictionary<string, Tuple<bool, string>>> _pluginsDictionary;

		// Cache of plugin as dictionary typeName -> Tuple<status, configuration>
		private Dictionary<string, Tuple<bool, string>> GetPlugins(string wiki) {
			if(_pluginsDictionary == null) _pluginsDictionary = new Dictionary<string, Dictionary<string, Tuple<bool, string>>>();

			if(!_pluginsDictionary.ContainsKey(wiki)) {
				_pluginsDictionary[wiki] = new Dictionary<string, Tuple<bool, string>>();
				IList<PluginEntity> pluginEntities = GetPluginEntities(wiki);
				foreach(PluginEntity pluginEntity in pluginEntities) {
					_pluginsDictionary[wiki].Add(pluginEntity.RowKey, new Tuple<bool, string>(pluginEntity.Status, pluginEntity.Configuration));
				}
			}
			return _pluginsDictionary[wiki];
		}

		private IList<PluginEntity> GetPluginEntities(string wiki) {
            CloudTable table = _cloudTableClient.GetTableReference(PluginsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, wiki);
            TableQuery<PluginEntity> query = (new TableQuery<PluginEntity>()).Where(partitionKeyFilter);
            return table.ExecuteQuery<PluginEntity>(query).ToList();
            //var query = (from e in _context.CreateQuery<PluginEntity>(PluginsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals(wiki)
            //             select e).AsTableServiceQuery();
            //return QueryHelper<PluginEntity>.All(query);
		}

		private PluginEntity GetPluginEntity(string wiki, string typeName) {
            CloudTable table = _cloudTableClient.GetTableReference(PluginsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, wiki);
            string rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, typeName);
            string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);
            TableQuery<PluginEntity> query = (new TableQuery<PluginEntity>()).Where(combinedFilter);
            return table.ExecuteQuery<PluginEntity>(query).FirstOrDefault();
            //var query = (from e in _context.CreateQuery<PluginEntity>(PluginsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals(wiki) && e.RowKey.Equals(typeName)
            //             select e).AsTableServiceQuery();
            //return QueryHelper<PluginEntity>.FirstOrDefault(query);
		}

		/// <summary>
		/// Sets the status of a plugin.
		/// </summary>
		/// <param name="typeName">The Type name of the plugin.</param>
		/// <param name="enabled">The plugin status.</param>
		/// <returns><c>true</c> if the status is stored, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="typeName"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="typeName"/> is empty.</exception>
		public bool SetPluginStatus(string typeName, bool enabled) {
			if(typeName == null) throw new ArgumentNullException("typeName");
			if(typeName.Length == 0) throw new ArgumentException("Type Name cannot be empty", "typeName");

			_pluginsDictionary = null;

			PluginEntity pluginEntity = GetPluginEntity(_wiki, typeName);

            CloudTable table = _cloudTableClient.GetTableReference(PluginsTable);
            var batchOperation = new TableBatchOperation();

			if(pluginEntity == null) {
				pluginEntity = new PluginEntity() {
					PartitionKey = _wiki,
					RowKey = typeName,
					Status = enabled
				};
                batchOperation.Insert(pluginEntity);
			}
			else {
				pluginEntity.Status = enabled;
                batchOperation.Replace(pluginEntity);
			}
            table.ExecuteBatch(batchOperation);
			return true;
		}

		/// <summary>
		/// Gets the status of a plugin.
		/// </summary>
		/// <param name="typeName">The Type name of the plugin.</param>
		/// <returns>The status (<c>false</c> for disabled, <c>true</c> for enabled), or <c>true</c> if no status is found.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="typeName"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="typeName"/> is empty.</exception>
		public bool GetPluginStatus(string typeName) {
			if(typeName == null) throw new ArgumentNullException("typeName");
			if(typeName.Length == 0) throw new ArgumentException("Type Name cannot be empty", "typeName");

			Tuple<bool, string> status = new Tuple<bool, string>(true, "");
			if(GetPlugins(_wiki).TryGetValue(typeName, out status)) return status.Item1;
			else return true;
		}

		/// <summary>
		/// Sets the configuration of a plugin.
		/// </summary>
		/// <param name="typeName">The Type name of the plugin.</param>
		/// <param name="config">The configuration.</param>
		/// <returns><c>true</c> if the configuration is stored, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="typeName"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="typeName"/> is empty.</exception>
		public bool SetPluginConfiguration(string typeName, string config) {
			if(typeName == null) throw new ArgumentNullException("typeName");
			if(typeName.Length == 0) throw new ArgumentException("Type Name cannot be empty", "typeName");

			_pluginsDictionary = null;

			config = config != null ? config : "";

			PluginEntity pluginEntity = GetPluginEntity(_wiki, typeName);

            CloudTable table = _cloudTableClient.GetTableReference(PluginsTable);
            var batchOperation = new TableBatchOperation();

			if(pluginEntity == null) {
				pluginEntity = new PluginEntity() {
					PartitionKey = _wiki,
					RowKey = typeName,
					Configuration = config
				};
                batchOperation.Insert(pluginEntity);
			}
			else {
				pluginEntity.Configuration = config;
                batchOperation.Replace(pluginEntity);
			}
            table.ExecuteBatch(batchOperation);
			return true;
		}

		/// <summary>
		/// Gets the configuration of a plugin.
		/// </summary>
		/// <param name="typeName">The Type name of the plugin.</param>
		/// <returns>The plugin configuration, or <b>String.Empty</b>.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="typeName"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="typeName"/> is empty.</exception>
		public string GetPluginConfiguration(string typeName) {
			if(typeName == null) throw new ArgumentNullException("typeName");
			if(typeName.Length == 0) throw new ArgumentException("Type Name cannot be empty", "typeName");

			Tuple<bool, string> status = new Tuple<bool, string>(false, "");
			if(GetPlugins(_wiki).TryGetValue(typeName, out status)) return status.Item2;
			else return "";
		}

		private Dictionary<string, MetadataEntity> _metadataCache;

		// Cache of settings as dictionary metadataKey -> content
		private Dictionary<string, MetadataEntity> _metadata {
			get {
				if(_metadataCache == null) {
					_metadataCache = new Dictionary<string, MetadataEntity>();
					IList<MetadataEntity> metadataEntities = GetMetadataEntities(_wiki);
					foreach(MetadataEntity metadataEntity in metadataEntities) {
						_metadataCache.Add(metadataEntity.RowKey, metadataEntity);
					}
				}
				return _metadataCache;
			}
		}

		private IList<MetadataEntity> GetMetadataEntities(string wiki) {
            CloudTable table = _cloudTableClient.GetTableReference(MetadataTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, wiki);
            TableQuery<MetadataEntity> query = (new TableQuery<MetadataEntity>()).Where(partitionKeyFilter);
            return table.ExecuteQuery<MetadataEntity>(query).ToList();
            //var query = (from e in _context.CreateQuery<MetadataEntity>(MetadataTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals(wiki)
            //             select e).AsTableServiceQuery();
            //return QueryHelper<MetadataEntity>.All(query);
		}

		/// <summary>
		/// Gets a meta-data item's content.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="tag">The tag that specifies the context (usually the namespace).</param>
		/// <returns>The content.</returns>
		public string GetMetaDataItem(MetaDataItem item, string tag) {
			try {
				MetadataEntity metadataEntity = null;
				_metadata.TryGetValue(item + "|" + tag, out metadataEntity);
				return metadataEntity == null ? "" : metadataEntity.Content + "";
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Sets a meta-data items' content.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="tag">The tag that specifies the context (usually the namespace).</param>
		/// <param name="content">The content.</param>
		/// <returns><c>true</c> if the content is set, <c>false</c> otherwise.</returns>
		public bool SetMetaDataItem(MetaDataItem item, string tag, string content) {
			try {
                CloudTable table = _cloudTableClient.GetTableReference(MetadataTable);
                var batchOperation = new TableBatchOperation();

				MetadataEntity metadataEntity = null;
				_metadata.TryGetValue(item + "|" + tag, out metadataEntity);
				if(metadataEntity == null) {
					metadataEntity = new MetadataEntity() {
						PartitionKey = _wiki,
						RowKey = item + "|" + tag,
						Content = content
					};
                    batchOperation.Insert(metadataEntity);
				}
				else {
					metadataEntity.Content = content;
                    batchOperation.Replace(metadataEntity);
				}
                table.ExecuteBatch(batchOperation);

				// Invalidate metadataCache
				_metadataCache = null;

				return true;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Gets the change from a string.
		/// </summary>
		/// <param name="change">The string.</param>
		/// <returns>The change.</returns>
		private static ScrewTurn.Wiki.PluginFramework.Change GetChange(string change) {
			switch(change.ToUpperInvariant()) {
				case "U":
					return ScrewTurn.Wiki.PluginFramework.Change.PageUpdated;
				case "D":
					return ScrewTurn.Wiki.PluginFramework.Change.PageDeleted;
				case "R":
					return ScrewTurn.Wiki.PluginFramework.Change.PageRolledBack;
				case "N":
					return ScrewTurn.Wiki.PluginFramework.Change.PageRenamed;
				case "MP":
					return ScrewTurn.Wiki.PluginFramework.Change.MessagePosted;
				case "ME":
					return ScrewTurn.Wiki.PluginFramework.Change.MessageEdited;
				case "MD":
					return ScrewTurn.Wiki.PluginFramework.Change.MessageDeleted;
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Gets the change string for a change.
		/// </summary>
		/// <param name="change">The change.</param>
		/// <returns>The change string.</returns>
		private static string GetChangeString(ScrewTurn.Wiki.PluginFramework.Change change) {
			switch(change) {
				case ScrewTurn.Wiki.PluginFramework.Change.PageUpdated:
					return "U";
				case ScrewTurn.Wiki.PluginFramework.Change.PageDeleted:
					return "D";
				case ScrewTurn.Wiki.PluginFramework.Change.PageRolledBack:
					return "R";
				case ScrewTurn.Wiki.PluginFramework.Change.PageRenamed:
					return "N";
				case ScrewTurn.Wiki.PluginFramework.Change.MessagePosted:
					return "MP";
				case ScrewTurn.Wiki.PluginFramework.Change.MessageEdited:
					return "ME";
				case ScrewTurn.Wiki.PluginFramework.Change.MessageDeleted:
					return "MD";
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Gets the recent changes of the Wiki.
		/// </summary>
		/// <returns>The recent Changes, oldest to newest.</returns>
		public RecentChange[] GetRecentChanges() {
			try {
                CloudTable table = _cloudTableClient.GetTableReference(RecentChangesTable);
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _wiki);
                TableQuery<RecentChangesEntity> query = (new TableQuery<RecentChangesEntity>()).Where(partitionKeyFilter).Take((int)(int.Parse(_host.GetSettingValue(SettingName.MaxRecentChanges)) * 0.90));
                IList<RecentChangesEntity> recentChangesEntities = table.ExecuteQuery<RecentChangesEntity>(query).ToList();
                //var query = (from e in _context.CreateQuery<RecentChangesEntity>(RecentChangesTable).AsTableServiceQuery()
                //             where e.PartitionKey.Equals(_wiki)
                //             select e).Take((int)(int.Parse(_host.GetSettingValue(SettingName.MaxRecentChanges)) * 0.90)).AsTableServiceQuery();
                //IList<RecentChangesEntity> recentChangesEntities = QueryHelper<RecentChangesEntity>.All(query);

				List<RecentChange> recentChanges = new List<RecentChange>(recentChangesEntities.Count);
				foreach(RecentChangesEntity entity in recentChangesEntities) {
					recentChanges.Add(new RecentChange(entity.Page, entity.Title, entity.MessageSubject + "", new DateTime(entity.DateTime.Ticks, DateTimeKind.Utc), entity.User, GetChange(entity.Change), entity.Description + ""));
				}
				return recentChanges.ToArray();
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Adds a new change.
		/// </summary>
		/// <param name="page">The page name.</param>
		/// <param name="title">The page title.</param>
		/// <param name="messageSubject">The message subject (or <c>null</c>).</param>
		/// <param name="dateTime">The date/time.</param>
		/// <param name="user">The user.</param>
		/// <param name="change">The change.</param>
		/// <param name="descr">The description (optional).</param>
		/// <returns><c>true</c> if the change is saved, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="page"/>, <paramref name="title"/> or <paramref name="user"/> are <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="page"/>, <paramref name="title"/> or <paramref name="user"/> are empty.</exception>
		public bool AddRecentChange(string page, string title, string messageSubject, DateTime dateTime, string user, ScrewTurn.Wiki.PluginFramework.Change change, string descr) {
			if(page == null) throw new ArgumentNullException("page");
			if(title == null) throw new ArgumentNullException("title");
			if(user == null) throw new ArgumentNullException("user"); 
			if(page.Length == 0) throw new ArgumentException("page");
			if(title.Length == 0) throw new ArgumentException("title");
			if(user.Length == 0) throw new ArgumentException("user");

			try {
				RecentChangesEntity recentChangesEntity = new RecentChangesEntity() {
					PartitionKey = _wiki,
					RowKey = dateTime.Ticks + "-" + Guid.NewGuid().ToString("N"),
					Page = page,
					Title = title,
					MessageSubject = messageSubject,
					DateTime = dateTime,
					User = user,
					Change = GetChangeString(change),
					Description = descr
				};
                CloudTable table = _cloudTableClient.GetTableReference(RecentChangesTable);
                TableOperation operation = TableOperation.Insert(recentChangesEntity);
                table.Execute(operation);
 
				return true;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Gets the ACL Manager instance.
		/// </summary>
		public ScrewTurn.Wiki.AclEngine.IAclManager AclManager {
			get { return _aclManager; }
		}

		/// <summary>
		/// Stores the outgoing links of a page, overwriting existing data.
		/// </summary>
		/// <param name="page">The full name of the page.</param>
		/// <param name="outgoingLinks">The full names of the pages that <b>page</b> links to.</param>
		/// <returns><c>true</c> if the outgoing links are stored, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="page"/> or <paramref name="outgoingLinks"/> are <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="page"/> or <paramref name="outgoingLinks"/> are empty.</exception>
		public bool StoreOutgoingLinks(string page, string[] outgoingLinks) {
			if(page == null) throw new ArgumentNullException("page");
			if(outgoingLinks == null) throw new ArgumentNullException("outgoingLinks");
			if(page.Length == 0) throw new ArgumentException("page");

			try {
                CloudTable table = _cloudTableClient.GetTableReference(OutgoingLinksTable);
                var batchOperation = new TableBatchOperation();
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _wiki);
                string sourcePageFilter = TableQuery.GenerateFilterCondition("SourcePage", QueryComparisons.Equal, page);
                string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, sourcePageFilter);
                TableQuery<OutgoingLinkEntity> query = (new TableQuery<OutgoingLinkEntity>()).Where(combinedFilter);
                var entities = table.ExecuteQuery<OutgoingLinkEntity>(query).ToList();
                //var query = (from e in _context.CreateQuery<OutgoingLinkEntity>(OutgoingLinksTable).AsTableServiceQuery()
                //             where e.PartitionKey.Equals(_wiki) && e.SourcePage.Equals(page)
                //             select e).AsTableServiceQuery();
                //var entities = QueryHelper<OutgoingLinkEntity>.All(query);
				foreach(var entity in entities) {
                    batchOperation.Delete(entity);
				}
                if (batchOperation.Count > 0)
                    table.ExecuteBatch(batchOperation);

                batchOperation = new TableBatchOperation();
				foreach(string outgoingLink in outgoingLinks) {
					if(outgoingLink == null) throw new ArgumentNullException("outgoingLinks", "Null element in outgoing links array");
					if(outgoingLink.Length == 0) throw new ArgumentException("Elements in outgoing links cannot be empty", "outgoingLinks");

					OutgoingLinkEntity outgoingLinksEntity = new OutgoingLinkEntity() {
						PartitionKey = _wiki,
						RowKey = Guid.NewGuid().ToString("N"),
						SourcePage = page,
						DestinationPage = outgoingLink
					};
                    batchOperation.Insert(outgoingLinksEntity);
				}
                if (batchOperation.Count > 0)
                    table.ExecuteBatch(batchOperation);
				return true;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Gets the outgoing links of a page.
		/// </summary>
		/// <param name="page">The full name of the page.</param>
		/// <returns>The outgoing links.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="page"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="page"/> is empty.</exception>
		public string[] GetOutgoingLinks(string page) {
			if(page == null) throw new ArgumentNullException("page");
			if(page.Length == 0) throw new ArgumentException("page");

			try {
                CloudTable table = _cloudTableClient.GetTableReference(OutgoingLinksTable);
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _wiki);
                string sourcePageFilter = TableQuery.GenerateFilterCondition("SourcePage", QueryComparisons.Equal, page);
                string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, sourcePageFilter);
                TableQuery<OutgoingLinkEntity> query = (new TableQuery<OutgoingLinkEntity>()).Where(combinedFilter);
                var entities = table.ExecuteQuery<OutgoingLinkEntity>(query).ToList();
                //var query = (from e in _context.CreateQuery<OutgoingLinkEntity>(OutgoingLinksTable).AsTableServiceQuery()
                //             where e.PartitionKey.Equals(_wiki) && e.SourcePage.Equals(page)
                //             select e).AsTableServiceQuery();
                //var entities = QueryHelper<OutgoingLinkEntity>.All(query);
				List<string> outgoingLinks = new List<string>(entities.Count);
				foreach(var entity in entities) {
					outgoingLinks.Add(entity.DestinationPage);
				}
				return outgoingLinks.ToArray();
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Gets all the outgoing links stored.
		/// </summary>
		/// <returns>The outgoing links, in a dictionary in the form page-&gt;outgoing_links.</returns>
		public IDictionary<string, string[]> GetAllOutgoingLinks() {
			try {
                CloudTable table = _cloudTableClient.GetTableReference(OutgoingLinksTable);
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _wiki);
                TableQuery<OutgoingLinkEntity> query = (new TableQuery<OutgoingLinkEntity>()).Where(partitionKeyFilter);
                IList<OutgoingLinkEntity> outgoingLinksEntities = table.ExecuteQuery<OutgoingLinkEntity>(query).ToList();
                //var query = (from e in _context.CreateQuery<OutgoingLinkEntity>(OutgoingLinksTable).AsTableServiceQuery()
                //             where e.PartitionKey.Equals(_wiki)
                //             select e).AsTableServiceQuery();
                //IList<OutgoingLinkEntity> outgoingLinksEntities = QueryHelper<OutgoingLinkEntity>.All(query);

				Dictionary<string, List<string>> outgoingLinksWithList = new Dictionary<string, List<string>>(outgoingLinksEntities.Count);
				foreach(OutgoingLinkEntity entity in outgoingLinksEntities) {
					if(!outgoingLinksWithList.ContainsKey(entity.SourcePage)) outgoingLinksWithList[entity.SourcePage] = new List<string>();
					outgoingLinksWithList[entity.SourcePage].Add(entity.DestinationPage);
				}
				Dictionary<string, string[]> outgoingLinks = new Dictionary<string, string[]>(outgoingLinksWithList.Count);
				foreach(var outgoingLink in outgoingLinksWithList) {
					outgoingLinks.Add(outgoingLink.Key, outgoingLink.Value.ToArray());
				}
				return outgoingLinks;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Deletes the outgoing links of a page and all the target links that include the page.
		/// </summary>
		/// <param name="page">The full name of the page.</param>
		/// <returns><c>true</c> if the links are deleted, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="page"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="page"/> is empty.</exception>
		public bool DeleteOutgoingLinks(string page) {
			if(page == null) throw new ArgumentNullException("page");
			if(page.Length == 0) throw new ArgumentException("page");

			try {
                CloudTable table = _cloudTableClient.GetTableReference(OutgoingLinksTable);
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _wiki);
                string sourcePageFilter = TableQuery.GenerateFilterCondition("SourcePage", QueryComparisons.Equal, page);
                string destinationPageFilter = TableQuery.GenerateFilterCondition("DestinationPage", QueryComparisons.Equal, page);
                string pageFilter = TableQuery.CombineFilters(sourcePageFilter, TableOperators.Or, destinationPageFilter);
                //string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, pageFilter);
                string combinedFilter = string.Format("({0}) {1} ({2})", partitionKeyFilter, TableOperators.And, pageFilter);
                TableQuery<OutgoingLinkEntity> query = (new TableQuery<OutgoingLinkEntity>()).Where(combinedFilter);
                var entities = table.ExecuteQuery<OutgoingLinkEntity>(query).ToList();
                //var query = (from e in _context.CreateQuery<OutgoingLinkEntity>(OutgoingLinksTable).AsTableServiceQuery()
                //            where e.PartitionKey.Equals(_wiki) && (e.SourcePage.Equals(page) || e.DestinationPage.Equals(page))
                //            select e).AsTableServiceQuery();
                //var entities = QueryHelper<OutgoingLinkEntity>.All(query);
				if(entities == null || entities.Count == 0) return false;
                var batchOperation = new TableBatchOperation();
				foreach(var entity in entities) {
                    batchOperation.Delete(entity);
				}
                if (batchOperation.Count > 0)
                    table.ExecuteBatch(batchOperation);

				return true;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Updates all outgoing links data for a page rename.
		/// </summary>
		/// <param name="oldName">The old page name.</param>
		/// <param name="newName">The new page name.</param>
		/// <returns><c>true</c> if the data is updated, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="oldName"/> or <paramref name="newName"/> are <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="oldName"/> or <paramref name="newName"/> are empty.</exception>
		public bool UpdateOutgoingLinksForRename(string oldName, string newName) {
			if(oldName == null) throw new ArgumentNullException("oldName");
			if(newName == null) throw new ArgumentNullException("newName");
			if(oldName.Length == 0) throw new ArgumentException("oldName");
			if(newName.Length == 0) throw new ArgumentException("newName");

			try {
				bool updateExecuted = false;
                CloudTable table = _cloudTableClient.GetTableReference(OutgoingLinksTable);
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _wiki);
                string sourcePageFilter = TableQuery.GenerateFilterCondition("SourcePage", QueryComparisons.Equal, oldName);
                string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, sourcePageFilter);
                TableQuery<OutgoingLinkEntity> query = (new TableQuery<OutgoingLinkEntity>()).Where(combinedFilter);
                var entities = table.ExecuteQuery<OutgoingLinkEntity>(query).ToList();
                //var query = (from e in _context.CreateQuery<OutgoingLinkEntity>(OutgoingLinksTable).AsTableServiceQuery()
                //             where e.PartitionKey.Equals(_wiki) && e.SourcePage.Equals(oldName)
                //             select e).AsTableServiceQuery();
                //var entities = QueryHelper<OutgoingLinkEntity>.All(query);
				if(entities != null && entities.Count > 0) {
                    var batchOperation = new TableBatchOperation();
					updateExecuted = true;
					foreach(var entity in entities) {
						entity.SourcePage = newName;
                        batchOperation.Replace(entity);
					}
                    if (batchOperation.Count > 0)
                        table.ExecuteBatch(batchOperation);
				}

                string destinationPageFilter = TableQuery.GenerateFilterCondition("DestinationPage", QueryComparisons.Equal, oldName);
                combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, destinationPageFilter);
                query = (new TableQuery<OutgoingLinkEntity>()).Where(combinedFilter);
                entities = table.ExecuteQuery<OutgoingLinkEntity>(query).ToList();
                //query = (from e in _context.CreateQuery<OutgoingLinkEntity>(OutgoingLinksTable).AsTableServiceQuery()
                //         where e.PartitionKey.Equals(_wiki) && e.DestinationPage.Equals(oldName)
                //         select e).AsTableServiceQuery();
                //entities = QueryHelper<OutgoingLinkEntity>.All(query);
				if(entities != null && entities.Count > 0) {
                    var batchOperation = new TableBatchOperation();
					updateExecuted = true;
					foreach(var entity in entities) {
						entity.DestinationPage= newName;
                        batchOperation.Replace(entity);
					}
                    if (batchOperation.Count > 0)
                        table.ExecuteBatch(batchOperation);
				}

				return updateExecuted;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Determines whether the wiki was started for the first time.
		/// </summary>
		/// <returns><c>true</c> if the wiki was started for the first time, <c>false</c> otherwise.</returns>
		public bool IsFirstApplicationStart() {
			return _settings.Count == 0;
		}
		
		/// <summary>
		/// The settings table name.
		/// </summary>
		public static readonly string SettingsTable = "Settings";

		/// <summary>
		/// The metadata table name.
		/// </summary>
		public static readonly string MetadataTable = "Metadata";

		/// <summary>
		/// The recent changes table name.
		/// </summary>
		public static readonly string RecentChangesTable = "RecentChanges";

		/// <summary>
		/// The outgoing links table name.
		/// </summary>
		public static readonly string OutgoingLinksTable = "OutgoingLinks";

		/// <summary>
		/// The outgoing links table name.
		/// </summary>
		public static readonly string AclEntriesTable = "AclEntries";

		/// <summary>
		/// The Plugins table name.
		/// </summary>
		public static readonly string PluginsTable = "Plugins";

		/// <summary>
		/// Gets the wiki that has been used to initialize the current instance of the provider.
		/// </summary>
		public string CurrentWiki {
			get { return _wiki; }
		}

		/// <summary>
		/// Initializes the Storage Provider.
		/// </summary>
		/// <param name="host">The Host of the Component.</param>
		/// <param name="config">The Configuration data, if any.</param>
		/// <param name="wiki">The wiki.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="host"/> or <paramref name="config"/> are <c>null</c>.</exception>
		/// <exception cref="InvalidConfigurationException">If <paramref name="config"/> is not valid or is incorrect.</exception>
		public void Init(IHostV50 host, string config, string wiki) {
			if(host == null) throw new ArgumentNullException("host");
			if(config == null) throw new ArgumentNullException("config");

			if(config == "") config = Config.GetConnectionString();

			_host = host;
			_wiki = string.IsNullOrEmpty(wiki) ? "root" : wiki.ToLowerInvariant();

            _cloudTableClient = TableStorage.TableClient(config);

			_aclManager = new AzureTableStorageAclManager(StoreEntry, DeleteEntries, RenameAclResource, RetrieveAllAclEntries, RetrieveAclEntriesForResource, RetrieveAclEntriesForSubject);
		}

		/// <summary>
		/// Sets up the Storage Provider.
		/// </summary>
		/// <param name="host">The Host of the Component.</param>
		/// <param name="config">The Configuration data, if any.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="host"/> or <paramref name="config"/> are <c>null</c>.</exception>
		/// <exception cref="InvalidConfigurationException">If <paramref name="config"/> is not valid or is incorrect.</exception>
		public void SetUp(IHostV50 host, string config) {
			if(host == null) throw new ArgumentNullException("host");
			if(config == null) throw new ArgumentNullException("config");

			if(config == "") config = Config.GetConnectionString();

			TableStorage.CreateTable(config, SettingsTable);
			TableStorage.CreateTable(config, MetadataTable);
			TableStorage.CreateTable(config, RecentChangesTable);
			TableStorage.CreateTable(config, OutgoingLinksTable);
			TableStorage.CreateTable(config, AclEntriesTable);
			TableStorage.CreateTable(config, PluginsTable);
		}

		/// <summary>
		/// Gets the Information about the Provider.
		/// </summary>
		public ComponentInformation Information {
			get { return new ComponentInformation("AzureTableStorage Settings Provider", "Threeplicate Srl","4.0.5.143", "http://www.sunhorizon.info", null); }
		}

		/// <summary>
		/// Gets a brief summary of the configuration string format, in HTML. Returns <c>null</c> if no configuration is needed.
		/// </summary>
		public string ConfigHelpHtml {
			get { return ""; }
		}

	    /// <summary>
	    /// Validates a configuration data.
	    /// </summary>
	    /// <param name="config"></param>
	    public void ValidateConfig(string config)
	    {
	        // TODO:
	    }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
			// Nothing todo
		}

		#region AclManager backend methods

		private List<AclEntry> _aclEntriesList;

		private List<AclEntry> _aclEntries {
			get {
				if(_aclEntriesList == null) {
					IList<AclEntriesEntity> aclEntriesEntities = GetAclEntriesEntities(_wiki);
					List<AclEntry> aclEntries = new List<AclEntry>(aclEntriesEntities == null ? 0 : aclEntriesEntities.Count);
					foreach(AclEntriesEntity entity in aclEntriesEntities) {
						aclEntries.Add(new AclEntry(entity.Resource, entity.Action, entity.Subject, AclEntryValueFromString(entity.Value)));
					}
					_aclEntriesList = aclEntries;
				}
				return _aclEntriesList;
			}
		}

		private IList<AclEntriesEntity> GetAclEntriesEntities(string wiki) {
            CloudTable table = _cloudTableClient.GetTableReference(AclEntriesTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, wiki);
            TableQuery<AclEntriesEntity> query = (new TableQuery<AclEntriesEntity>()).Where(partitionKeyFilter);
            return table.ExecuteQuery<AclEntriesEntity>(query).ToList();
            //var query = (from e in _context.CreateQuery<AclEntriesEntity>(AclEntriesTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals(wiki)
            //             select e).AsTableServiceQuery();
            //return QueryHelper<AclEntriesEntity>.All(query);
		}

		/// <summary>
		/// Converts a <see cref="T:Value" /> to its corresponding string representation.
		/// </summary>
		/// <param name="value">The <see cref="T:Value" />.</param>
		/// <returns>The character representation.</returns>
		private static string AclEntryValueToString(Value value) {
			switch(value) {
				case Value.Grant:
					return "G";
				case Value.Deny:
					return "D";
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Converts a string representation of a <see cref="T:Value" /> back to the enum value.
		/// </summary>
		/// <param name="c">The character representation.</param>
		/// <returns>The <see cref="T:Value" />.</returns>
		private static Value AclEntryValueFromString(string c) {
			switch(c.ToUpperInvariant()) {
				case "G":
					return Value.Grant;
				case "D":
					return Value.Deny;
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Retrieves all ACL entries.
		/// </summary>
		/// <returns>The ACL entries.</returns>
		private AclEntry[] RetrieveAllAclEntries() {
			return _aclEntries.ToArray();
		}

		/// <summary>
		/// Retrieves all ACL entries for a resource.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <returns>The ACL entries for the resource.</returns>
		private AclEntry[] RetrieveAclEntriesForResource(string resource) {
			return _aclEntries.FindAll(e => e.Resource == resource).ToArray();
		}

		/// <summary>
		/// Retrieves all ACL entries for a subject.
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <returns>The ACL entries for the subject.</returns>
		private AclEntry[] RetrieveAclEntriesForSubject(string subject) {
			return _aclEntries.FindAll(e => e.Subject == subject).ToArray();
		}

		/// <summary>
		/// Deletes some ACL entries.
		/// </summary>
		/// <param name="entries">The entries to delete.</param>
		/// <returns><c>true</c> if one or more entries were deleted, <c>false</c> otherwise.</returns>
		private bool DeleteEntries(AclEntry[] entries) {
			try {
				_aclEntriesList = null;
				IList<AclEntriesEntity> aclEntriesEntities = GetAclEntriesEntities(_wiki);
				if(aclEntriesEntities == null || aclEntriesEntities.Count == 0) return false;

                CloudTable table = _cloudTableClient.GetTableReference(AclEntriesTable);
                var batchOperation = new TableBatchOperation();

				bool deleteExecuted = false;
				foreach(AclEntry aclEntry in entries) {
					foreach(AclEntriesEntity entity in aclEntriesEntities) {
						if(aclEntry.Resource == entity.Resource &&
						   aclEntry.Subject == entity.Subject &&
						   aclEntry.Action == entity.Action) {
								deleteExecuted = true;
                                batchOperation.Delete(entity);
						}
					}
				}
                if (deleteExecuted && batchOperation.Count > 0) table.ExecuteBatch(batchOperation);
				return deleteExecuted;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Stores a ACL entry.
		/// </summary>
		/// <param name="entry">The entry to store.</param>
		/// <returns><c>true</c> if the entry was stored, <c>false</c> otherwise.</returns>
		private bool StoreEntry(AclEntry entry) {
			try {
				AclEntriesEntity aclEntriesEntity = new AclEntriesEntity() {
					PartitionKey = _wiki,
					RowKey = Guid.NewGuid().ToString("N"),
					Resource = entry.Resource,
					Subject = entry.Subject,
					Action = entry.Action,
					Value = AclEntryValueToString(entry.Value)
				};
                CloudTable table = _cloudTableClient.GetTableReference(AclEntriesTable);
                TableOperation operation = TableOperation.Insert(aclEntriesEntity);
                table.Execute(operation);
				return true;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Renames a ACL resource.
		/// </summary>
		/// <param name="resource">The resource to rename.</param>
		/// <param name="newName">The new name of the resource.</param>
		/// <returns><c>true</c> if one or more entries weere updated, <c>false</c> otherwise.</returns>
		private bool RenameAclResource(string resource, string newName) {
			try {
                CloudTable table = _cloudTableClient.GetTableReference(AclEntriesTable);
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _wiki);
                string resourceFilter = TableQuery.GenerateFilterCondition("Resource", QueryComparisons.Equal, resource);
                string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, resourceFilter);
                TableQuery<AclEntriesEntity> query = (new TableQuery<AclEntriesEntity>()).Where(combinedFilter);
                IList<AclEntriesEntity> aclEntriesEntities = table.ExecuteQuery<AclEntriesEntity>(query).ToList();
                //var query = (from e in _context.CreateQuery<AclEntriesEntity>(AclEntriesTable).AsTableServiceQuery()
                //             where e.PartitionKey.Equals(_wiki) && e.Resource.Equals(resource)
                //             select e).AsTableServiceQuery();
                //IList<AclEntriesEntity> aclEntriesEntities = QueryHelper<AclEntriesEntity>.All(query);

				if(aclEntriesEntities == null || aclEntriesEntities.Count == 0) return false;

                var batchOperation = new TableBatchOperation();
				foreach(AclEntriesEntity entity in aclEntriesEntities) {
					entity.Resource = newName;
                    batchOperation.Replace(entity);;
				}
                if (batchOperation.Count > 0)
                    table.ExecuteBatch(batchOperation);
				return true;
			}
			catch(Exception ex) {
				throw ex;
			}
		}

		#endregion
	}

	internal class SettingsEntity : TableEntity {
		// PartitionKey = wiki
		// RowKey = settingKey

		public string Value { get; set; }
	}

    internal class MetadataEntity : TableEntity
    {
		// PartitionKey = wiki
		// RowKey = item|tag

		public string Content { get; set; }
	}

    internal class RecentChangesEntity : TableEntity
    {
		// PartitionKey = wiki
		// RowKey = remainder (seconds) + salt

		public string Page { get; set; }
		public string Title { get; set; }
		public string MessageSubject { get; set; }
		public DateTime DateTime { get; set; }
		public string User { get; set; }
		public string Change { get; set; }
		public string Description { get; set; }
	}

    internal class OutgoingLinkEntity : TableEntity
    {
		// PartitionKey = wiki
		// RowKey = GUID

		public string SourcePage { get; set; }
		public string DestinationPage { get; set; }
	}

    internal class AclEntriesEntity : TableEntity
    {
		// PartitionKey = wiki
		// RowKey = GUID

		public string Resource { get; set; }
		public string Subject { get; set; }
		public string Action { get; set; }
		public string Value { get; set; }
	}

    internal class PluginEntity : TableEntity
    {
		// PartitionKey = wiki
		// RowKey = typeName

		public bool Status { get; set; }
		public string Configuration { get; set; }
	}

}
