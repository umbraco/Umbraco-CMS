using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Umbraco.Web.Mvc
{

    /// <summary>
    /// Custom json result using newtonsoft json.net
    /// </summary>
    public class JsonNetResult : ActionResult
    {
        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public object Data { get; set; }

        public JsonSerializerSettings SerializerSettings { get; set; }
        public Formatting Formatting { get; set; }

        /// <summary>
        /// Default, unchanged JsonSerializerSettings
        /// </summary>
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings();

        public JsonNetResult()
        {
            SerializerSettings = new JsonSerializerSettings();
        }
        public JsonNetResult(JsonSerializerSettings jsonSerializerSettings)
        {
            SerializerSettings = jsonSerializerSettings;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            HttpResponseBase response = context.HttpContext.Response;

            response.ContentType = string.IsNullOrEmpty(ContentType) == false
              ? ContentType
              : "application/json";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data != null)
            {
                var writer = new JsonTextWriter(response.Output) { Formatting = Formatting };

                var serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(writer, Data);

                writer.Flush();
            }
        }
    }

}
