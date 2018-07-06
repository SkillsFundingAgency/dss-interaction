using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace NCS.DSS.Interaction.Helpers
{
    public static class HttpResponseMessageHelper
    {

        #region Ok(200)

        public static HttpResponseMessage Ok(string message, Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(message + JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region Created(201) 

        public static HttpResponseMessage Created(string message, Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(message + JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region NoContent(204)

        public static HttpResponseMessage NoContent(string message, Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                Content = new StringContent(message + JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json")
            };

        }

        #endregion

        #region BadRequest(400)

        public static HttpResponseMessage BadRequest(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json")
            };
        }

        public static HttpResponseMessage BadRequest(string message, Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(message + JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region UnprocessableEntity(422)

        public static HttpResponseMessage UnprocessableEntity(string message, List<ValidationResult> errors)
        {
            return new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent("Validation error(s) : " + JsonConvert.SerializeObject(errors),
                    Encoding.UTF8, "application/json")
            };
        }


        #endregion



    }
}
