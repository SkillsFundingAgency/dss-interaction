using System;

namespace NCS.DSS.Interaction.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
    }
}