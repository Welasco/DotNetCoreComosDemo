namespace todo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    
    public class DocumentDBRepository<T> : IDocumentDBRepository<T> where T : class
    {

        //private readonly string Endpoint = "https://vwscosmossql.documents.azure.com:443/";
        //private readonly string Key = "bFh9ksHBUhGEWsUQvvI2Ncsf0NO674onvXiRP8ZiqRccMWwNBdV8oK6JhKUackMqqj4AU4xNw31o6iYhy9m5Tw==";

        private readonly string Endpoint = System.Environment.GetEnvironmentVariable("EnvEndpoint");
        private readonly string Key = System.Environment.GetEnvironmentVariable("EnvKey");

        private readonly string EnvEndpoint = System.Environment.GetEnvironmentVariable("EnvEndpoint");
        private readonly string EnvKey = System.Environment.GetEnvironmentVariable("EnvKey");
        private readonly string EnvTest = System.Environment.GetEnvironmentVariable("test");

        private readonly string DatabaseId = "ToDoList";
        private readonly string CollectionId = "Items";
        private DocumentClient client;

        public DocumentDBRepository()
        {
            try
            {
                var test = System.Environment.GetEnvironmentVariables();
                this.client = new DocumentClient(new Uri(Endpoint), Key);
                CreateDatabaseIfNotExistsAsync().Wait();
                CreateCollectionIfNotExistsAsync().Wait();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception at DocumentDBRepository: " + e.Message);
                System.Console.WriteLine("Stack at DocumentDBRepository: " + e.StackTrace);
            }

        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> CreateItemAsync(T item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
        }

        public async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item);
        }

        public async Task DeleteItemAsync(string id)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}