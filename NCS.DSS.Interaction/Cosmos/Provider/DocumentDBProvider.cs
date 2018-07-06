using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Interaction.Cosmos.Client;
using NCS.DSS.Interaction.Cosmos.Helper;

namespace NCS.DSS.Interaction.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var query = client.CreateDocumentQuery<Models.Interaction>(collectionUri, new FeedOptions {MaxItemCount = 1});
            var customerExists = query.Where(x => x.CustomerId == customerId).AsEnumerable().Any();

            return customerExists;
        }

        public async Task<ResourceResponse<Document>> GetInteractionAsync(Guid interactionId)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(interactionId);

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReadDocumentAsync(documentUri);

            return response;
        }

        public async Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var addressForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Interaction>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.InteractionId == interactionId)
                .AsDocumentQuery();

            if (addressForCustomerQuery == null)
                return null;

            var addressess = await addressForCustomerQuery.ExecuteNextAsync<Models.Interaction>();

            return addressess?.FirstOrDefault();
        }


        public async Task<List<Models.Interaction>> GetInteractionsForCustomerAsync(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var queryInteractions = client.CreateDocumentQuery<Models.Interaction>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var interactions = new List<Models.Interaction>();

            while (queryInteractions.HasMoreResults)
            {
                var response = await queryInteractions.ExecuteNextAsync<Models.Interaction>();
                interactions.AddRange(response);
            }

            return interactions.Any() ? interactions : null;

        }

        public async Task<ResourceResponse<Document>> CreateInteractionAsync(Models.Interaction interaction)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, interaction);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateInteractionAsync(Models.Interaction interaction)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(interaction.InteractionId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, interaction);

            return response;
        }
    }
}