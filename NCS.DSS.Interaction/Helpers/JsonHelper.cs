using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Interaction.Helpers
{
    public class JsonHelper
    {
        private const string ResourceIdName = "InteractionId";

        public static string SerializeObject<T>(T resource)
        {
            var json = JsonConvert.SerializeObject(resource);
            var resourceJObject = JObject.Parse(json);

            if (!resourceJObject.HasValues)
                return json;

            var prop = resourceJObject.Property("id");
            RenameProperty(prop, ResourceIdName);

            return resourceJObject.ToString();
        }

        public static string SerializeObjects<T>(List<T> resource)
        {
            var json = JsonConvert.SerializeObject(resource);
            var tokens = JArray.Parse(json);

            foreach (var jToken in tokens)
            {
                var item = (JObject) jToken;

                if(item == null) 
                    continue;

                var prop = item.Property("id");
                RenameProperty(prop, ResourceIdName);
            }

            return tokens.ToString();
        }

        private static void RenameProperty(JToken token, string newName)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token), "Cannot rename a null token");

            JProperty property;

            if (token.Type == JTokenType.Property)
            {
                if (token.Parent == null)
                    throw new InvalidOperationException("Cannot rename a property with no parent");

                property = (JProperty)token;
            }
            else
            {
                if (token.Parent == null || token.Parent.Type != JTokenType.Property)
                    throw new InvalidOperationException("This token's parent is not a JProperty; cannot rename");

                property = (JProperty)token.Parent;
            }

            var newProperty = new JProperty(newName, property.Value);
            property.Replace(newProperty);
        }
    }
}
