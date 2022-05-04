using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetInteractionFromRequest<T>(HttpRequestMessage req);
        string GetTouchpointId(HttpRequestMessage req);
        string GetSubcontractorId(HttpRequestMessage req);

        string GetApimURL(HttpRequestMessage req);
    }
}