using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core.Logging;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// This will format the JSON output for use with AngularJs's approach to JSON Vulnerability attacks
    /// </summary>
    /// <remarks>
    /// See: http://docs.angularjs.org/api/ng.$http (Security considerations)
    /// </remarks>
    public class AngularJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {

        /// <summary>
        /// This will prepend the special chars to the stream output that angular will strip
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="writeStream"></param>
        /// <param name="content"></param>
        /// <param name="transportContext"></param>
        /// <returns></returns>
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            //Before we were calling the base method to do this however it was causing problems:
            // http://issues.umbraco.org/issue/U4-4546
            // though I can't seem to figure out why the null ref exception was being thrown, it is very strange.
            // This code is basically what the base class does and at least we can track/test exactly what is going on.

            if (type == null) throw new ArgumentNullException("type");
            if (writeStream == null) throw new ArgumentNullException("writeStream");
            
            var task = Task.Factory.StartNew(() =>
            {
                var effectiveEncoding = SelectCharacterEncoding(content == null ? null : content.Headers);

                using (var streamWriter = new StreamWriter(writeStream, effectiveEncoding))
                using (var jsonTextWriter = new JsonTextWriter(streamWriter)
                {
                    CloseOutput = false
                })
                {
                    //write the special encoding for angular json to the start
                    // (see: http://docs.angularjs.org/api/ng.$http)
                    streamWriter.Write(")]}',\n");

                    if (Indent)
                    {
                        jsonTextWriter.Formatting = Formatting.Indented;
                    }
                    var jsonSerializer = JsonSerializer.Create(SerializerSettings);
                    jsonSerializer.Serialize(jsonTextWriter, value);

                    jsonTextWriter.Flush();
                }
            });
            return task;
        }

    }
}
