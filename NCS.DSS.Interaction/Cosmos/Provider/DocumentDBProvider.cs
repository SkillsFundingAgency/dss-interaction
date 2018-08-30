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
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();

        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var customerByIdQuery = client
                ?.CreateDocumentQuery<Document>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.Id == customerId.ToString())
                .AsDocumentQuery();

            if (customerByIdQuery == null)
                return false;

            var customerQuery = await customerByIdQuery.ExecuteNextAsync<Document>();

            var customer = customerQuery?.FirstOrDefault();

            if (customer == null)
                return false;

            var dateOfTermination = customer.GetPropertyValue<DateTime?>("DateOfTermination");

            return dateOfTermination.HasValue;
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

            var interactionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Interaction>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.InteractionId == interactionId)
                .AsDocumentQuery();

            if (interactionForCustomerQuery == null)
                return null;

            var interactions = await interactionForCustomerQuery.ExecuteNextAsync<Models.Interaction>();

            return interactions?.FirstOrDefault();
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