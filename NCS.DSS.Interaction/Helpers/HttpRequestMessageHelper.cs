using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetInteractionFromRequest<T>(HttpRequestMessage req)
        {
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
