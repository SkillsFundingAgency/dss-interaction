using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Interaction.Annotations;
using NCS.DSS.Interaction.Ioc;

namespace NCS.DSS.Interaction.APIDefinition
{
    public static class ApiDefinition
    {

        public const string APITitle = "Interactions";
        public const string APIDefinitionName = "API-Definition";
        public const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        public const string APIDescription = "Basic details of a National Careers Service " + APITitle + " Resource";

        public class Result<T>
        {
            public Result(T value) { Value = value; }
            public T Value{ get; set; }
        }
        
        [FunctionName(APIDefinitionName)]
        [ResponseType(typeof(void))]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = APIDefRoute)]HttpRequestMessage req)
        {
            var assembly = Assembly.GetExecutingAssembly();

            dynamic doc = new ExpandoObject();
            doc.swagger = "2.0";
            doc.info = new ExpandoObject();
            doc.info.title = APITitle;
            doc.info.version = "1.0.0";
            doc.info.description = APIDescription;
            doc.host = req.RequestUri.Authority;
            doc.basePath = "/";
            doc.schemes = new[] { "https" };
            if (doc.host.Contains("127.0.0.1") || doc.host.Contains("localhost"))
            {
                doc.schemes = new[] { "http" };
            }

            doc.definitions = new ExpandoObject();
            doc.paths = GeneratePaths(assembly, doc);
            doc.securityDefinitions = GenerateSecurityDefinitions();

            return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<object>(doc, new JsonMediaTypeFormatter()),

            });
        }

        private static dynamic GenerateSecurityDefinitions()
        {
            dynamic securityDefinitions = new ExpandoObject();
            securityDefinitions.apikeyQuery = new ExpandoObject();
            securityDefinitions.apikeyQuery.type = "apiKey";
            securityDefinitions.apikeyQuery.name = "code";
            securityDefinitions.apikeyQuery.@in = "query";

            // Microsoft Flow import doesn't like two apiKey options, so we leave one out.

            //securityDefinitions.apikeyHeader = new ExpandoObject();
            //securityDefinitions.apikeyHeader.type = "apiKey";
            //securityDefinitions.apikeyHeader.name = "x-functions-key";
            //securityDefinitions.apikeyHeader.@in = "header";
            return securityDefinitions;
        }

        private static dynamic GeneratePaths(Assembly assembly, dynamic doc)
        {
            dynamic paths = new ExpandoObject();
            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(FunctionNameAttribute), false).Length > 0).ToArray();
            foreach (MethodInfo methodInfo in methods)
            {
                //hide any disabled methods
                if(methodInfo.GetCustomAttributes(typeof(DisableAttribute), true).Any())
                    continue;

                var route = "/api/";

                var functionAttr = (FunctionNameAttribute)methodInfo.GetCustomAttributes(typeof(FunctionNameAttribute), false)
                    .Single();

                if (functionAttr.Name == APIDefinitionName) continue;

                HttpTriggerAttribute triggerAttribute = null;
                foreach (ParameterInfo parameter in methodInfo.GetParameters())
                {
                    triggerAttribute = parameter.GetCustomAttributes(typeof(HttpTriggerAttribute), false)
                        .FirstOrDefault() as HttpTriggerAttribute;
                    if (triggerAttribute != null) break;
                }
                if (triggerAttribute == null) continue; // Trigger attribute is required in an Azure function

                if (!string.IsNullOrWhiteSpace(triggerAttribute.Route))
                {
                    route += triggerAttribute.Route;
                }
                else
                {
                    route += functionAttr.Name;
                }

                dynamic path = new ExpandoObject();

                var verbs = triggerAttribute.Methods ?? new[] { "get", "post", "delete", "head", "patch", "put", "options" };
                foreach (string verb in verbs)
                {
                    dynamic operation = new ExpandoObject();
                    operation.operationId = ToTitleCase(functionAttr.Name);
                    operation.produces = new[] { "application/json" };
                    operation.consumes = new[] { "application/json" };
                    operation.parameters = GenerateFunctionParametersSignature(methodInfo, route, doc);

                    // Summary is title
                    operation.summary = GetFunctionName(methodInfo, functionAttr.Name);
                    // Verbose description
                    operation.description = GetFunctionDescription(methodInfo, functionAttr.Name);

                    operation.responses = GenerateResponseParameterSignature(methodInfo, doc);
                    operation.tags = new[] { APITitle };

                    dynamic keyQuery = new ExpandoObject();
                    keyQuery.apikeyQuery = new string[0];
                    operation.security = new ExpandoObject[] { keyQuery };

                    // Microsoft Flow import doesn't like two apiKey options, so we leave one out.
                    //dynamic apikeyHeader = new ExpandoObject();
                    //apikeyHeader.apikeyHeader = new string[0];
                    //operation.security = new ExpandoObject[] { keyQuery, apikeyHeader };

                    AddToExpando(path, verb, operation);
                }
                AddToExpando(paths, route, path);
            }
            return paths;
        }

        private static string GetFunctionDescription(MethodInfo methodInfo, string funcName)
        {
            var displayAttr = (DisplayAttribute)methodInfo.GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault();
            return !string.IsNullOrWhiteSpace(displayAttr?.Description) ? displayAttr.Description : $"This function will run {funcName}";
        }

        /// <summary>
        /// Max 80 characters in summary/title
        /// </summary>
        private static string GetFunctionName(MethodInfo methodInfo, string funcName)
        {
            var displayAttr = (DisplayAttribute)methodInfo.GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault();
            if (!string.IsNullOrWhiteSpace(displayAttr?.Name))
            {
                return displayAttr.Name.Length > 80 ? displayAttr.Name.Substring(0, 80) : displayAttr.Name;
            }
            return $"Run {funcName}";
        }

        private static string GetPropertyDescription(PropertyInfo propertyInfo)
        {
            var displayAttr = (DisplayAttribute)propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault();
            return !string.IsNullOrWhiteSpace(displayAttr?.Description) ? displayAttr.Description : $"This returns {propertyInfo.PropertyType.Name}";
        }

        private static dynamic GenerateResponseParameterSignature(MethodInfo methodInfo, dynamic doc)
        {
            dynamic responses = new ExpandoObject();
            dynamic responseDef = new ExpandoObject();
            

            var returnType = methodInfo.ReturnType;
            if (returnType.IsGenericType)
            {
                var genericReturnType = returnType.GetGenericArguments().FirstOrDefault();
                if (genericReturnType != null)
                {
                    returnType = genericReturnType;
                }
            }
            if (returnType == typeof(HttpResponseMessage))
            {
                var responseTypeAttr = (ResponseTypeAttribute)methodInfo
                    .GetCustomAttributes(typeof(ResponseTypeAttribute), false).FirstOrDefault();
                if (responseTypeAttr != null)
                {
                    returnType = responseTypeAttr.ResponseType;
                }
                else
                {
                    returnType = typeof(void);
                }
            }
            if (returnType != typeof(void))
            {
                responseDef.schema = new ExpandoObject();

                if (returnType.Namespace == "System")
                {
                    // Warning:
                    // Allthough valid, it's always better to wrap single values in an object
                    // Returning { Value = "foo" } is better than just "foo"
                    SetParameterType(returnType, responseDef.schema, null);
                }
                else
                {
                    string name = returnType.Name;
                    if (returnType.IsGenericType)
                    {
                        var realType = returnType.GetGenericArguments()[0];
                        if (realType.Namespace == "System")
                        {
                            dynamic inlineSchema = GetObjectSchemaDefinition(null, returnType);
                            responseDef.schema = inlineSchema;
                        }
                        else
                        {
                            AddToExpando(responseDef.schema, "$ref", "#/definitions/" + name);
                            AddParameterDefinition((IDictionary<string, object>)doc.definitions, returnType);
                        }
                    }
                    else
                    {
                        AddToExpando(responseDef.schema, "$ref", "#/definitions/" + name);
                        AddParameterDefinition((IDictionary<string, object>)doc.definitions, returnType);
                    }
                }
            }
            // automatically get data(http code, description and show schema) from the new custom response class
            var responseCodes = methodInfo.GetCustomAttributes(typeof(Response), false);

            foreach (var response in responseCodes)
            {
                var interactionResponse = (Response)response;

                if (!interactionResponse.ShowSchema)
                    responseDef = new ExpandoObject();

                responseDef.description = interactionResponse.Description;
                AddToExpando(responses, interactionResponse.HttpStatusCode.ToString(), responseDef);
            }

            return responses;
        }

        private static List<object> GenerateFunctionParametersSignature(MethodInfo methodInfo, string route, dynamic doc)
        {
            var parameterSignatures = new List<object>();

            dynamic opHeaderParam = new ExpandoObject();
            opHeaderParam.name = "TouchpointId";
            opHeaderParam.@in = "header";
            opHeaderParam.required = true;
            opHeaderParam.type = "string";
            parameterSignatures.Add(opHeaderParam);

            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                if (parameter.ParameterType == typeof(HttpRequestMessage)) continue;
                if (parameter.ParameterType == typeof(TraceWriter)) continue;
                if (parameter.ParameterType == typeof(Microsoft.Extensions.Logging.ILogger)) continue;
                if (parameter.GetCustomAttributes().Any(attr => attr is InjectAttribute)) continue;

                bool hasUriAttribute = parameter.GetCustomAttributes().Any(attr => attr is FromUriAttribute);

                if (route.Contains('{' + parameter.Name))
                {
                    dynamic opParam = new ExpandoObject();
                    opParam.name = parameter.Name;
                    opParam.@in = "path";
                    opParam.required = true;
                    SetParameterType(parameter.ParameterType, opParam, null);
                    parameterSignatures.Add(opParam);
                }
                else if (hasUriAttribute && parameter.ParameterType.Namespace == "System")
                {
                    dynamic opParam = new ExpandoObject();
                    opParam.name = parameter.Name;
                    opParam.@in = "query";
                    opParam.required = parameter.GetCustomAttributes().Any(attr => attr is RequiredAttribute);
                    SetParameterType(parameter.ParameterType, opParam, doc.definitions);
                    parameterSignatures.Add(opParam);
                }
                else if (hasUriAttribute && parameter.ParameterType.Namespace != "System")
                {
                    AddObjectProperties(parameter.ParameterType, "", parameterSignatures, doc);
                }
                else
                {
                    dynamic opParam = new ExpandoObject();
                    opParam.name = parameter.Name;
                    opParam.@in = "body";
                    opParam.required = true;
                    opParam.schema = new ExpandoObject();
                    if (parameter.ParameterType.Namespace == "System")
                    {
                        SetParameterType(parameter.ParameterType, opParam.schema, null);
                    }
                    else
                    {
                        AddToExpando(opParam.schema, "$ref", "#/definitions/" + parameter.ParameterType.Name);
                        AddParameterDefinition((IDictionary<string, object>)doc.definitions, parameter.ParameterType);
                    }
                    parameterSignatures.Add(opParam);
                }
            }
            return parameterSignatures;
        }

        private static void AddObjectProperties(Type t, string parentName, List<object> parameterSignatures, dynamic doc)
        {
            var publicProperties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in publicProperties)
            {
                if (!string.IsNullOrWhiteSpace(parentName))
                {
                    parentName += ".";
                }
                if (property.PropertyType.Namespace != "System")
                {
                    AddObjectProperties(property.PropertyType, parentName + property.Name, parameterSignatures, doc);
                }
                else
                {
                    dynamic opParam = new ExpandoObject();

                    opParam.name = parentName + property.Name;
                    opParam.@in = "query";
                    opParam.required = property.GetCustomAttributes().Any(attr => attr is RequiredAttribute);
                    opParam.description = GetPropertyDescription(property);
                    SetParameterType(property.PropertyType, opParam, doc.definitions);
                    parameterSignatures.Add(opParam);
                }
            }
        }

        private static void AddParameterDefinition(IDictionary<string, object> definitions, Type parameterType)
        {
            dynamic objDef;
            if (!definitions.TryGetValue(parameterType.Name, out objDef))
            {
                objDef = GetObjectSchemaDefinition(definitions, parameterType);
                definitions.Add(parameterType.Name, objDef);
            }
        }

        private static dynamic GetObjectSchemaDefinition(IDictionary<string, object> definitions, Type parameterType)
        {
            dynamic objDef = new ExpandoObject();
            objDef.type = "object";
            objDef.properties = new ExpandoObject();
            var publicProperties = parameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<string> requiredProperties = new List<string>();
            foreach (PropertyInfo property in publicProperties)
            {
                if (property.GetCustomAttributes().Any(attr => attr is RequiredAttribute))
                {
                    requiredProperties.Add(property.Name);
                }
                dynamic propDef = new ExpandoObject();
                propDef.description = GetPropertyDescription(property);

                var exampleAttribute = (Example)property.GetCustomAttributes(typeof(Example), false).FirstOrDefault();
                if (exampleAttribute != null)
                    propDef.example = exampleAttribute.Description;

                var stringAttribute = (StringLengthAttribute)property.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault();
                if (stringAttribute != null)
                {
                    propDef.maxLength = stringAttribute.MaximumLength;
                    propDef.minLength = stringAttribute.MinimumLength;
                }

                var regexAttribute = (RegularExpressionAttribute)property.GetCustomAttributes(typeof(RegularExpressionAttribute), false).FirstOrDefault();
                if (regexAttribute != null)
                    propDef.pattern = regexAttribute.Pattern;


                SetParameterType(property.PropertyType, propDef, definitions);
                AddToExpando(objDef.properties, property.Name, propDef);
            }
            if (requiredProperties.Count > 0)
            {
                objDef.required = requiredProperties;
            }
            return objDef;
        }

        private static void SetParameterType(Type parameterType, dynamic opParam, dynamic definitions)
        {
            var inputType = parameterType;
            string paramType = parameterType.UnderlyingSystemType.ToString();

            var setObject = opParam;
            if (inputType.IsArray)
            {
                opParam.type = "array";
                opParam.items = new ExpandoObject();
                setObject = opParam.items;
                parameterType = parameterType.GetElementType();
            }
            //else if (inputType.IsGenericType)
            //{
            //    opParam.type = "array";
            //    opParam.items = new ExpandoObject();
            //    setObject = opParam.items;
            //    parameterType = parameterType.GetGenericArguments()[0];
            //}

            if (inputType.Namespace == "System" || (inputType.IsGenericType && inputType.GetGenericArguments()[0].Namespace == "System"))
            {
                if (paramType.Contains("System.String"))
                {
                    setObject.type = "string";
                }
                else if (paramType.Contains("System.DateTime"))
                {
                    setObject.format = "date";
                    setObject.type = "string";
                }
                else if (paramType.Contains("System.Int32"))
                {
                    setObject.format = "int32";
                    setObject.type = "integer";
                }
                else if (paramType.Contains("System.Int64"))
                {
                    setObject.format = "int64";
                    setObject.type = "integer";
                }
                else if (paramType.Contains("System.Single"))
                {
                    setObject.format = "float";
                    setObject.type = "number";
                }
                else if (paramType.Contains("System.Boolean"))
                {
                    setObject.type = "boolean";
                }
                else
                {
                    setObject.type = "string";
                }

            }

            //if (inputType.Namespace == "System" || (inputType.IsGenericType && inputType.GetGenericArguments()[0].Namespace == "System"))
            //{
            //    TypeCode myTypeCode = Type.GetTypeCode(inputType);

            //    switch (Type.GetTypeCode(inputType))
            //    {
            //        case TypeCode.Int32:
            //            setObject.format = "int32";
            //            setObject.type = "integer";
            //            break;
            //        case TypeCode.Int64:
            //            setObject.format = "int64";
            //            setObject.type = "integer";
            //            break;
            //        case TypeCode.Single:
            //            setObject.format = "float";
            //            setObject.type = "number";
            //            break;
            //        case TypeCode.Double:
            //            setObject.format = "double";
            //            setObject.type = "number";
            //            break;
            //        case TypeCode.String:
            //            setObject.type = "string";
            //            break;
            //        case TypeCode.Byte:
            //            setObject.format = "byte";
            //            setObject.type = "string";
            //            break;
            //        case TypeCode.Boolean:
            //            setObject.type = "boolean";
            //            break;
            //        case TypeCode.DateTime:
            //            setObject.format = "date";
            //            setObject.type = "string";
            //            break;
            //        default:
            //            setObject.type = "string";
            //            break;
            //    }
            //}
            else if (inputType.IsEnum)
            {
                opParam.type = "string";

                var enumValues = new List<string>();

                foreach (var item in Enum.GetValues(inputType))
                {
                    var memInfo = inputType.GetMember(inputType.GetEnumName(item));
                    var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                    var description = string.Empty;

                    if (descriptionAttributes.Length > 0)
                        description = ((DescriptionAttribute)descriptionAttributes[0]).Description;

                    if (string.IsNullOrEmpty(description))
                        description = item.ToString();

                    enumValues.Add(Convert.ToInt32(item) + " - " + description);
                }

                opParam.@enum = enumValues.ToArray();

            }
            else if (definitions != null)
            {
                AddToExpando(setObject, "$ref", "#/definitions/" + parameterType.Name);
                AddParameterDefinition((IDictionary<string, object>)definitions, parameterType);
            }
        }


        public static string ToTitleCase(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        public static void AddToExpando(ExpandoObject obj, string name, object value)
        {
            if (((IDictionary<string, object>)obj).ContainsKey(name))
            {
                // Fix for functions with same routes but different verbs
                var existing = (IDictionary<string, object>)((IDictionary<string, object>)obj)[name];
                var append = (IDictionary<string, object>)value;
                foreach (KeyValuePair<string, object> keyValuePair in append)
                {
                    existing.Add(keyValuePair);
                }
            }
            else
            {
                ((IDictionary<string, object>)obj).Add(name, value);
            }
        }
    }
}