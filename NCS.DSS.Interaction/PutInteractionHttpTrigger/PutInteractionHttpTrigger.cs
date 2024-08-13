namespace NCS.DSS.Interaction.PutInteractionHttpTrigger
{
    public static class PutInteractionHttpTrigger
    {
        /*[Disable]
        [Function("Put")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/Interactions/{interactionId}")] HttpRequest req, ILogger log, string interactionId)
        {
            log.LogInformation("Put Interaction C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(interactionId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Replaced Interaction record with Id of : " + interactionGuid)
            };
        }*/
    }
}