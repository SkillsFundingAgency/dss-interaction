using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Reflection;

namespace NCS.DSS.Interaction.APIDefinition
{
    public class ApiDefinition
    {
        public const string APITitle = "Interactions";
        public const string APIDefinitionName = "API-Definition";
        public const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        public const string APIDescription = "Basic details of a National Careers Service " + APITitle + " Resource";
        public const string ApiVersion = "2.0.0";
        private ISwaggerDocumentGenerator swaggerDocumentGenerator;

        public ApiDefinition(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            this.swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [Function(APIDefinitionName)]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = APIDefRoute)] HttpRequest req)
        {
            var swagger = swaggerDocumentGenerator.GenerateSwaggerDocument(req, APITitle, APIDescription,
                APIDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
                return new NoContentResult();

            return new OkObjectResult(swagger);
        }
    }
}