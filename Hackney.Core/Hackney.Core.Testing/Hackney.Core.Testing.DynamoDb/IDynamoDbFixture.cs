using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.DynamoDb
{
    /// <summary>
    /// DynamoDb fixture interface to be used to set up a local database instance for use in tests where a 
    /// "real" instance is required.
    /// </summary>
    public interface IDynamoDbFixture : IAsyncDisposable
    {
        /// <summary>
        /// A IDynamoDBContext reference
        /// </summary>
        IDynamoDBContext DynamoDbContext { get; }

        /// <summary>
        /// A IAmazonDynamoDB reference
        /// </summary>
        IAmazonDynamoDB DynamoDb { get; }

        /// <summary>
        /// Uses the configured DynamoDb instance to created the required table(s). 
        /// If the table already exists then the ResourceInUseException is *not* rethrown.
        /// </summary>
        /// <param name="tables">List of table definitions to create</param>
        void EnsureTablesExist(List<TableDef> tables);

        /// <summary>
        /// Saves the supplied entity to the configured DynamoDb instance. 
        /// Also amtains a reference to the save entity and will remove it from the database automatically when disposed.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entity">The entity instance</param>
        /// <returns>Task</returns>
        Task SaveEntityAsync<T>(T entity) where T : class;
    }
}
