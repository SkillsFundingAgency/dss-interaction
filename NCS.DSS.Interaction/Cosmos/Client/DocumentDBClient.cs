using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Interaction.Cosmos.Client
{
    public class DocumentDBClient : IDocumentDBClient
    {
        private DocumentClient _documentClient;
        private DocumentClient _customerDocumentClient;

        public DocumentClient CreateDocumentClient()
        {
            if (_documentClient != null)
                return _documentClient;

            _documentClient = new DocumentClient(new Uri(
                ConfigurationManager.AppSettings["Endpoint"]),
                ConfigurationManager.AppSettings["Key"]);

            return _documentClient;
        }
        
        public DocumentClient CreateCustomerDocumentClient()
        {
            if(_customerDocumentClient != null)
            return _customerDocumentClient;

            _customerDocumentClient = new DocumentClient(new Uri(
                ConfigurationManager.AppSettings["CustomerEndpoint"]),
                ConfigurationManager.AppSettings["CustomerKey"]);

            return _customerDocumentClient;
        }
    }
}
