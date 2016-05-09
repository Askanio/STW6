
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using System.Data.Services.Client;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ScrewTurn.Wiki.Plugins.AzureStorage {

	/// <summary>
	/// 
	/// </summary>
	public static class TableStorage {

		/// <summary>
		/// Get the storage account.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		public static CloudStorageAccount StorageAccount(string connectionString) {
            //CloudStorageAccount myAccount = CloudStorageAccount.DevelopmentStorageAccount;
            //connectionString = myAccount.ToString();
		    return CloudStorageAccount.Parse(connectionString);
		}

		/// <summary>
        /// Get the TableClient
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static CloudTableClient TableClient(string connectionString) {
			var client = StorageAccount(connectionString).CreateCloudTableClient();
            client.DefaultRequestOptions = GetDefaultTableRequestOptions();
		    return client;
		}

		/// <summary>
		/// Creates a table if it does not exist.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="tableName">The name of the table.</param>
		public static void CreateTable(string connectionString, string tableName) {
            var tableClient = TableClient(connectionString);
            var cloudTable = tableClient.GetTableReference(tableName);
            cloudTable.CreateIfNotExists();
		}

		/// <summary>
		/// Removes all data from a table.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="tableName">The table name.</param>
		public static void TruncateTable(string connectionString, string tableName) {
			var client = TableClient(connectionString);
            if (!client.GetTableReference(tableName).Exists()) return;

            CloudTable table = client.GetTableReference(tableName);

            var entities = (from entry in table.CreateQuery<DummyEntity>() select entry);

            foreach (var e in entities)
            {
                TableOperation deleteOperation = TableOperation.Delete(e);
                table.Execute(deleteOperation);
            }
		}

		private class DummyEntity : TableEntity {
		}

		#region blobs

		/// <summary>
		/// Deletes all blobs in all containers.
		/// Used only in tests tear-down method
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		public static void DeleteAllBlobs(string connectionString) {
			CloudBlobClient _client = TableStorage.StorageAccount(connectionString).CreateCloudBlobClient();
            _client.DefaultRequestOptions = GetDefaultBlobRequestOptions();

			foreach(CloudBlobContainer containerRef in _client.ListContainers()) {
                IEnumerable<IListBlobItem> blobs = containerRef.ListBlobs(useFlatBlobListing: true);
				foreach(IListBlobItem blob in blobs) {
                    var blobRef = _client.GetBlobReferenceFromServer(blob.Uri); // v.1.7: GetBlobReference(blob.Uri.AbsoluteUri)
					blobRef.DeleteIfExists();
				}
			}
		}

		#endregion

		/// <summary>
        /// Gets the default table request options.
		/// </summary>
        public static TableRequestOptions GetDefaultTableRequestOptions()
        {
            return new TableRequestOptions
            {
                RetryPolicy = GetDefaultRetryPolicy(),
                //PayloadFormat = TablePayloadFormat.JsonNoMetadata
            };
		}

        /// <summary>
        /// Gets the default blob request options.
        /// </summary>
        public static BlobRequestOptions GetDefaultBlobRequestOptions()
        {
            return new BlobRequestOptions
            {
                RetryPolicy = GetDefaultRetryPolicy(),
                //PayloadFormat = TablePayloadFormat.JsonNoMetadata
            };
        }

        /// <summary>
        /// Gets the default retry policy.
        /// </summary>
	    private static IRetryPolicy GetDefaultRetryPolicy()
	    {
	        return new ExponentialRetry(TimeSpan.FromSeconds(0.5), 10);
	            //RetryPolicies.RetryExponential(10, TimeSpan.FromSeconds(0.5));
	    }
	}
}
