using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Soundscripter.Mongo
{
    public class CosmosUtils
    {
        public static IMongoDatabase ConnectToDatabase(string connectionString, string database)
        {
            var clientOptions = MongoClientSettings.FromConnectionString(connectionString);
            clientOptions.RetryWrites = false;
            var client = new MongoClient(clientOptions);
            return client.GetDatabase(database);
        }

        public static IMongoCollection<SamplesCollection> GetCollection(IMongoDatabase database, string collection)
        {
            return database.GetCollection<SamplesCollection>(collection);
        }

        public static async Task AddDocumentAsync(IMongoCollection<SamplesCollection> collection, SamplesCollection document)
        {
            await collection.InsertOneAsync(document);
        }

        public static async Task UpdateDocumentAsync(IMongoCollection<SamplesCollection> collection, SamplesCollection document)
        {
            await collection.ReplaceOneAsync(x => x.transcriptId == document.transcriptId, document);
        }

        public static async Task<IEnumerable<SamplesCollection>> GetAllAsync(IMongoCollection<SamplesCollection> collection)
        {
            return (await collection.FindAsync(FilterDefinition<SamplesCollection>.Empty)).ToEnumerable();
        }

        public static async Task DeleteAsync(IMongoCollection<SamplesCollection> collection, SamplesCollection document)
        {
            await collection.DeleteOneAsync(x => x.transcriptId == document.transcriptId);
        }
    }
}