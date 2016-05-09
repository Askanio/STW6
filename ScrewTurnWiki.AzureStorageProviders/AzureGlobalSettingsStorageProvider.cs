
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Configuration;

namespace ScrewTurn.Wiki.Plugins.AzureStorage {

	/// <summary>
	/// Implements an Azure Table Storage global settings storage provider.
	/// </summary>
	public class AzureGlobalSettingsStorageProvider :IGlobalSettingsStorageProviderV60 {

		private IHostV50 _host;
		private string _wiki;

		private CloudBlobClient _client;
	    private CloudTableClient _cloudTableClient;

		#region IGlobalSettingsStorageProviderV50 Members

		private Dictionary<string, string> _settingsDictionary;

		// Cache of settings as dictionary settingKey -> settingValue
		private Dictionary<string, string> _settings {
			get {
				if(_settingsDictionary == null) {
					_settingsDictionary = new Dictionary<string, string>();
					IList<GlobalSettingsEntity> settingsEntities = GetGlobalSettingsEntities();
					foreach(GlobalSettingsEntity settingsEntity in settingsEntities) {
						_settingsDictionary.Add(settingsEntity.RowKey, settingsEntity.Value + "");
					}
				}
				return _settingsDictionary;
			}
		}

		private IList<GlobalSettingsEntity> GetGlobalSettingsEntities() {
            CloudTable table = _cloudTableClient.GetTableReference(GlobalSettingsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
            TableQuery<GlobalSettingsEntity> query = (new TableQuery<GlobalSettingsEntity>()).Where(partitionKeyFilter);
            return table.ExecuteQuery<GlobalSettingsEntity>(query).ToList();
  
            //var query = (from e in _context.CreateQuery<GlobalSettingsEntity>(GlobalSettingsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals("0")
            //             select e).AsTableServiceQuery();
            //return QueryHelper<GlobalSettingsEntity>.All(query);
		}

		private GlobalSettingsEntity GetGlobalSettingsEntity(string settingName) {
            CloudTable table = _cloudTableClient.GetTableReference(GlobalSettingsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
            string rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, settingName);
            string combinedFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);
            TableQuery<GlobalSettingsEntity> query = (new TableQuery<GlobalSettingsEntity>()).Where(combinedFilter);
            return table.ExecuteQuery<GlobalSettingsEntity>(query).FirstOrDefault();

            //var query = (from e in _context.CreateQuery<GlobalSettingsEntity>(GlobalSettingsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals("0") && e.RowKey.Equals(settingName)
            //             select e).AsTableServiceQuery();
            //return QueryHelper<GlobalSettingsEntity>.FirstOrDefault(query);
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
		/// Gets the all the setting values.
		/// </summary>
		/// <returns>All the settings.</returns>
		public IDictionary<string, string> GetAllSettings() {
			return _settings;
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

			GlobalSettingsEntity settingsEntity = GetGlobalSettingsEntity(name);
            CloudTable table = _cloudTableClient.GetTableReference(GlobalSettingsTable);
            var batchOperation = new TableBatchOperation();

			if(settingsEntity == null) {
				settingsEntity = new GlobalSettingsEntity() {
					PartitionKey = "0",
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

		private IList<Configuration.Wiki> GetWikiList() {
			if(RoleEnvironment.IsAvailable) {
				string config = RoleEnvironment.GetConfigurationSettingValue("Wikis");
				if(string.IsNullOrEmpty(config)) throw new InvalidConfigurationException("Wikis not specified in service configuration");

				string[] wikis = config.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				if(wikis.Length == 0) throw new InvalidConfigurationException("Wikis not specified in service configuration");

				char[] sep = new[] { '=' };

				List<Configuration.Wiki> result = new List<Configuration.Wiki>();
				foreach(string line in wikis) {
					string[] fields = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
					if(fields.Length == 0) continue;

                    Configuration.Wiki wiki = new Configuration.Wiki(fields[0], fields.Length > 1 ? fields[1].Split(';').ToList() : new List<string>());

					result.Add(wiki);
				}
				return result;
			}
			else {
				return ApplicationSettings.Instance.Wikis;
			}


		}

		/// <summary>
		/// Gets alls the wikis.
		/// </summary>
		/// <returns>A collection of wiki identifiers.</returns>
		public ScrewTurn.Wiki.Configuration.Wiki[] GetAllWikis() {
			return GetWikiList().ToArray();
		}

		/// <summary>
		/// Extracts the name of the wiki from the given host.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns>The name of the wiki.</returns>
		/// <exception cref="WikiNotFoundException">If no wiki is found corresponding to the given host.</exception>
		public string ExtractWikiName(string host) {
			foreach(Configuration.Wiki wiki in GetWikiList()) {
				if(wiki.Hosts.Contains(host)) return wiki.WikiName;
			}
			throw new WikiNotFoundException("The given host: " + host + " does not correspond to any wiki.");
		}

		/// <summary>
		/// Lists the stored plugin assemblies.
		/// </summary>
		public string[] ListPluginAssemblies() {
			var containerRef = _client.GetContainerReference(AssembliesContainer);
			var blobs = containerRef.ListBlobs();

			List<string> assemblies = new List<string>();
			foreach(var blob in blobs) {
				string blobName = blob.Uri.PathAndQuery;
				blobName = blobName.Substring(blobName.IndexOf(blob.Container.Name) + blob.Container.Name.Length);
				assemblies.Add(blobName.Trim('/'));
			}
			return assemblies.ToArray();
		}

		/// <summary>
		/// Stores a plugin's assembly, overwriting existing ones if present.
		/// </summary>
		/// <param name="filename">The file name of the assembly, such as "Assembly.dll".</param>
		/// <param name="assembly">The assembly content.</param>
		/// <returns><c>true</c> if the assembly is stored, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="filename"/> or <paramref name="assembly"/> are <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="filename"/> or <paramref name="assembly"/> are empty.</exception>
		public bool StorePluginAssembly(string filename, byte[] assembly) {
			if(filename == null) throw new ArgumentNullException("filename");
			if(filename.Length == 0) throw new ArgumentException("Filename cannot be empty", "filename");
			if(assembly == null) throw new ArgumentNullException("assembly");
			if(assembly.Length == 0) throw new ArgumentException("Assembly cannot be empty", "assembly");

			var containerRef = _client.GetContainerReference(AssembliesContainer);
			var blobRef = containerRef.GetBlockBlobReference(filename);
			blobRef.UploadByteArray(assembly);
			return true;
		}

		/// <summary>
		/// Retrieves a plugin's assembly.
		/// </summary>
		/// <param name="filename">The file name of the assembly.</param>
		/// <returns>The assembly content, or <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="filename"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="filename"/> is empty.</exception>
		public byte[] RetrievePluginAssembly(string filename) {
			if(filename == null) throw new ArgumentNullException("filename");
			if(filename.Length == 0) throw new ArgumentException("Filename cannot be empty", "filename");

			try {
				var containerRef = _client.GetContainerReference(AssembliesContainer);
				var blobRef = containerRef.GetBlockBlobReference(filename);
				byte[] assembly = blobRef.DownloadByteArray();
				return assembly;
			}
			catch(StorageException ex) {
                var errorCode = ex.RequestInformation.ExtendedErrorInformation.ErrorCode;
                if (errorCode == BlobErrorCodeStrings.BlobNotFound) return null;
				throw ex;
			}
		}

		/// <summary>
		/// Removes a plugin's assembly.
		/// </summary>
		/// <param name="filename">The file name of the assembly to remove, such as "Assembly.dll".</param>
		/// <returns><c>true</c> if the assembly is removed, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="filename"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="filename"/> is empty.</exception>
		public bool DeletePluginAssembly(string filename) {
			if(filename == null) throw new ArgumentNullException(filename);
			if(filename.Length == 0) throw new ArgumentException("Filename cannot be empty", "filename");

			try {
				var containerRef = _client.GetContainerReference(AssembliesContainer);
				var blobRef = containerRef.GetBlockBlobReference(filename);
				return blobRef.DeleteIfExists();
			}
			catch(StorageException ex) {
                var errorCode = ex.RequestInformation.ExtendedErrorInformation.ErrorCode;
                if (errorCode == BlobErrorCodeStrings.BlobNotFound) return false;
				throw ex;
			}
		}

		private const int EstimatedLogEntrySize = 100; // bytes

		/// <summary>
		/// Converts an <see cref="T:EntryType" /> to a string.
		/// </summary>
		/// <param name="type">The entry type.</param>
		/// <returns>The corresponding string.</returns>
		private static string EntryTypeToString(EntryType type) {
			switch(type) {
				case EntryType.General:
					return "G";
				case EntryType.Warning:
					return "W";
				case EntryType.Error:
					return "E";
				default:
					return "G";
			}
		}

		/// <summary>
		/// Converts an entry type string to an <see cref="T:EntryType" />.
		/// </summary>
		/// <param name="value">The string.</param>
		/// <returns>The <see cref="T:EntryType" />.</returns>
		private static EntryType EntryTypeParse(string value) {
			switch(value) {
				case "G":
					return EntryType.General;
				case "W":
					return EntryType.Warning;
				case "E":
					return EntryType.Error;
				default:
					return EntryType.General;
			}
		}

		/// <summary>
		/// Records a message to the System Log.
		/// </summary>
		/// <param name="message">The Log Message.</param>
		/// <param name="entryType">The Type of the Entry.</param>
		/// <param name="user">The User.</param>
		/// <param name="wiki">The wiki, <c>null</c> if is an application level log.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="message"/> or <paramref name="user"/> are <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="message"/> or <paramref name="user"/> are empty.</exception>
		public void LogEntry(string message, EntryType entryType, string user, string wiki) {
			if(message == null) throw new ArgumentNullException("message");
			if(message.Length == 0) throw new ArgumentException("Message cannot be empty", "message");
			if(user == null) throw new ArgumentNullException("user");
			if(user.Length == 0) throw new ArgumentException("User cannot be empty", "user");

			DateTime dateTime = DateTime.UtcNow;

			LogEntity logEntity = new LogEntity() {
				PartitionKey = "0",
				RowKey = dateTime.Ticks + "-" + Guid.NewGuid().ToString("N"),
				Type = EntryTypeToString(entryType),
				DateTime = dateTime,
				User = user,
				Message = message,
				Wiki = wiki
			};
            CloudTable table = _cloudTableClient.GetTableReference(LogsTable);
            TableOperation insertOperation = TableOperation.Insert(logEntity);
            table.Execute(insertOperation);

			int logSize = LogSize;
			if(logSize > int.Parse(_host.GetGlobalSettingValue(GlobalSettingName.MaxLogSize))) {
				CutLog((int)(logSize * 0.75));
			}
		}

		/// <summary>
		/// Reduces the size of the Log to the specified size (or less).
		/// </summary>
		/// <param name="size">The size to shrink the log to (in bytes).</param>
		private void CutLog(int size) {
			size = size * 1024;

            CloudTable table = _cloudTableClient.GetTableReference(LogsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
            TableQuery<LogEntity> query = (new TableQuery<LogEntity>()).Where(partitionKeyFilter);
            LogEntity[] logEntities = table.ExecuteQuery<LogEntity>(query).ToArray();

            //var query = (from e in _context.CreateQuery<LogEntity>(LogsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals("0")
            //             select e).AsTableServiceQuery();
            //LogEntity[] logEntities = QueryHelper<LogEntity>.All(query).ToArray();
            int estimatedSize = logEntities.Length * EstimatedLogEntrySize;

			if(size < estimatedSize) {
				int difference = estimatedSize - size;
				int entriesToDelete = difference / EstimatedLogEntrySize;
				// Add 10% to avoid 1-by-1 deletion when adding new entries
				entriesToDelete += entriesToDelete / 10;

				if(entriesToDelete > 0) {

                    var batchOperation = new TableBatchOperation();

					int count = 0;
					for(int i = 0; i < entriesToDelete; i++) {
                        batchOperation.Delete(logEntities[i]);
						if(count > 98) {
                            table.ExecuteBatch(batchOperation);
                            batchOperation = new TableBatchOperation();
							count = 0;
						}
						else {
							count++;
						}
					}
                    if (batchOperation.Count > 0)
                        table.ExecuteBatch(batchOperation);
				}
			}
		}

		/// <summary>
		/// Gets all the Log Entries, sorted by date/time (oldest to newest).
		/// </summary>
		/// <returns>The Log Entries.</returns>
		public LogEntry[] GetLogEntries() {

            CloudTable table = _cloudTableClient.GetTableReference(LogsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
            TableQuery<LogEntity> query = (new TableQuery<LogEntity>()).Where(partitionKeyFilter).Take((int)(int.Parse(_host.GetGlobalSettingValue(GlobalSettingName.MaxLogSize)) * 9));
            LogEntity[] logEntities = table.ExecuteQuery<LogEntity>(query).ToArray();

            //var query = (from e in _context.CreateQuery<LogEntity>(LogsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals("0")
            //             select e).Take((int)(int.Parse(_host.GetGlobalSettingValue(GlobalSettingName.MaxLogSize)) * 9)).AsTableServiceQuery();
            //IList<LogEntity> logEntities = QueryHelper<LogEntity>.All(query);

			List<LogEntry> logEntries = new List<LogEntry>();
			foreach(LogEntity entity in logEntities) {
				logEntries.Add(new LogEntry(EntryTypeParse(entity.Type), new DateTime(entity.DateTime.Ticks, DateTimeKind.Utc), entity.Message, entity.User, entity.Wiki));
			}
			return logEntries.ToArray();
		}

		/// <summary>
		/// Clear the Log.
		/// </summary>
		public void ClearLog() {
            CloudTable table = _cloudTableClient.GetTableReference(LogsTable);
            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
            TableQuery<LogEntity> query = (new TableQuery<LogEntity>()).Where(partitionKeyFilter);
            LogEntity[] logEntities = table.ExecuteQuery<LogEntity>(query).ToArray();

            //var query = (from e in _context.CreateQuery<LogEntity>(LogsTable).AsTableServiceQuery()
            //             where e.PartitionKey.Equals("0")
            //             select e).AsTableServiceQuery();
            //IList<LogEntity> logEntities = QueryHelper<LogEntity>.All(query);
            var batchOperation = new TableBatchOperation();
            int count = 0;
			foreach(LogEntity logEntity in logEntities) {
                batchOperation.Delete(logEntity);
				if(count > 98) {
                    table.ExecuteBatch(batchOperation);
                    batchOperation = new TableBatchOperation();
					count = 0;
				}
				else {
					count++;
				}
			}
            if (batchOperation.Count > 0)
                table.ExecuteBatch(batchOperation);
		}

		/// <summary>
		/// Gets the current size of the Log, in KB.
		/// </summary>
		public int LogSize {
			get {
                CloudTable table = _cloudTableClient.GetTableReference(LogsTable);
                string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0");
                TableQuery<LogEntity> query = (new TableQuery<LogEntity>()).Where(partitionKeyFilter);
                LogEntity[] logEntities = table.ExecuteQuery<LogEntity>(query).ToArray();
                //var query = (from e in _context.CreateQuery<LogEntity>(LogsTable).AsTableServiceQuery()
                //             where e.PartitionKey.Equals("0")
                //             select e).AsTableServiceQuery();
                //IList<LogEntity> logEntities = QueryHelper<LogEntity>.All(query);
				int estimatedSize = logEntities.Count() * EstimatedLogEntrySize;

				return estimatedSize / 1024;
			}
		}

		#endregion

		#region IProviderV50 Members

		/// <summary>
		/// The global settings table name.
		/// </summary>
		public static readonly string GlobalSettingsTable = "GlobalSettings";

		/// <summary>
		/// The Logs table name.
		/// </summary>
		public static readonly string LogsTable = "Logs";

		/// <summary>
		/// The assembly blob container name.
		/// </summary>
		public static readonly string AssembliesContainer = "assemblies";

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
			_wiki = wiki;

			_client = TableStorage.StorageAccount(config).CreateCloudBlobClient();

            _cloudTableClient = TableStorage.TableClient(config);
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

			TableStorage.CreateTable(config, GlobalSettingsTable);
			TableStorage.CreateTable(config, LogsTable);

			_client = TableStorage.StorageAccount(config).CreateCloudBlobClient();

			CloudBlobContainer containerRef = _client.GetContainerReference(AssembliesContainer);
			containerRef.CreateIfNotExists();
		}

		/// <summary>
		/// Gets the Information about the Provider.
		/// </summary>
		public ComponentInformation Information {
			get { return new ComponentInformation("AzureTableStorage Global Settings Provider", "Threeplicate Srl", "4.0.5.143", "http://www.sunhorizon.info", null); }
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

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
			// Nothing todo
		}

		#endregion
	}

    internal class GlobalSettingsEntity : TableEntity
	{
		// PartitionKey = "0"
		// RowKey = settingKey
        public string Value { get; set; }
	}

    internal class LogEntity : TableEntity
    {
		// PartitionKey = "0"
		// RowKey = remainder (seconds) + salt

		public string Type { get; set; }
		public DateTime DateTime { get; set; }
		public string Message { get; set; }
		public string User { get; set; }
		public string Wiki { get; set; }
	}

}
