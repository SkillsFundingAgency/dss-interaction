﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetInteractionFromRequest<T>(HttpRequestMessage req)
        {
            if (req == null)
                return default(T);

            if (req.Content?.Headers != null)
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await req.Content.ReadAsAsync<T>();
        }

        public string GetTouchpointId(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("TouchpointId"))
                return null;

            var touchpointId = req.Headers.GetValues("TouchpointId").FirstOrDefault();

            return string.IsNullOrEmpty(touchpointId) ? string.Empty : touchpointId;
        }
        public string GetSubcontractorId(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("SubcontractorId"))
                return null;

            var subcontractorId = req.Headers.GetValues("SubcontractorId").FirstOrDefault();

            return string.IsNullOrEmpty(subcontractorId) ? string.Empty : subcontractorId;
        }

        public string GetApimURL(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("apimurl"))
                return null;

            var ApimURL = req.Headers.GetValues("apimurl").FirstOrDefault();

            if (ApimURL.EndsWith("/"))
                ApimURL = ApimURL.Substring(0, ApimURL.Length - 1);


            return string.IsNullOrEmpty(ApimURL) ? string.Empty : ApimURL;
        }
    }
}
