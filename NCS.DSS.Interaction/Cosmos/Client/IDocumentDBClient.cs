using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Interaction.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
        DocumentClient CreateCustomerDocumentClient();
    }
}