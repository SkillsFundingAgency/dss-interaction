using System;

namespace NCS.DSS.Interaction.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid interactionId);
        Uri CreateCustomerDocumentCollectionUri();
    }
}