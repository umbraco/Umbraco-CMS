using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Umbraco.Web.BackOffice.ActionResults
{
    /// <summary>
    /// Custom json result using newtonsoft json.net
    /// </summary>
    public class JsonNetResult : IActionResult
    {
        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public object Data { get; set; }

        public JsonSerializerSettings SerializerSettings { get; set; }
        public Formatting Formatting { get; set; }

        public JsonNetResult()
        {
            SerializerSettings = new JsonSerializerSettings();
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var response = context.HttpContext.Response;

            response.ContentType = string.IsNullOrEmpty(ContentType) == false
                ? ContentType
                : System.Net.Mime.MediaTypeNames.Application.Json;

            if (!(ContentEncoding is null))
                response.Headers.Add(Microsoft.Net.Http.Headers.HeaderNames.ContentEncoding, ContentEncoding.ToString());

            if (!(Data is null))
            {
                using var bodyWriter = new StreamWriter(response.Body);
                using var writer = new JsonTextWriter(bodyWriter) { Formatting = Formatting };
                var serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(writer, Data);
            }

            return Task.CompletedTask;
        }
    }
}
