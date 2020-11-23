using Microsoft.Azure.Documents.Client;
using System;

namespace NCS.DSS.Interaction.Cosmos.Client
{
    public static class DocumentDBClient
    {
        private static DocumentClient _documentClient;

        public static DocumentClient CreateDocumentClient()
        {
            if (_documentClient != null)
                return _documentClient;

            _documentClient = new DocumentClient(new Uri(
                Environment.GetEnvironmentVariable("Endpoint")),
                Environment.GetEnvironmentVariable("Key"));

            return _documentClient;
        }
    }
}
