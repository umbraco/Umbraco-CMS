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

            if (type == null) throw new ArgumentNullException("type");
            if (writeStream == null) throw new ArgumentNullException("writeStream");

            var effectiveEncoding = SelectCharacterEncoding(content == null ? null : content.Headers);

            using (var streamWriter = new StreamWriter(writeStream, effectiveEncoding))
            {
                //write the special encoding for angular json to the start
                // (see: http://docs.angularjs.org/api/ng.$http)
                streamWriter.Write(")]}',\n");
                streamWriter.Flush();
                return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
            }

            
        }

    }
}
