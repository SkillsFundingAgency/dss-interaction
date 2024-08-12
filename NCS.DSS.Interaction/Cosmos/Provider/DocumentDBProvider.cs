using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Interaction.Cosmos.Client;
using NCS.DSS.Interaction.Cosmos.Helper;

namespace NCS.DSS.Interaction.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;
            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public async Task<ResourceResponse<Document>> GetInteractionAsync(Guid interactionId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(interactionId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReadDocumentAsync(documentUri);

            return response;
        }

        public async Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

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
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

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

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, interaction);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateInteractionAsync(Models.Interaction interaction)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(interaction.InteractionId.GetValueOrDefault());

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, interaction);

            return response;
        }
    }
}