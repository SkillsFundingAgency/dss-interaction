using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Interaction.Cosmos.Client
{
    public class DocumentDBClient : IDocumentDBClient
    {
        private DocumentClient _documentClient;

        public DocumentClient CreateDocumentClient()
        {
            if (_documentClient != null)
                return _documentClient;

            _documentClient = new DocumentClient(new Uri(
                ConfigurationManager.AppSettings["Endpoint"]),
                ConfigurationManager.AppSettings["Key"]);

            return _documentClient;
        }
    }
}
