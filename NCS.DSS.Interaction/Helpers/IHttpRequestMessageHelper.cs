using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetInteractionFromRequest<T>(HttpRequestMessage req);
    }
}