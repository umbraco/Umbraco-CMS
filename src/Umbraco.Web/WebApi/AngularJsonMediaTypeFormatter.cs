using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
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
        public async override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            
            using (var memStream = new MemoryStream())
            {
                try
                {
                    //Let the base class do all the processing using our custom stream
                    await base.WriteToStreamAsync(type, value, memStream, content, transportContext);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<AngularJsonMediaTypeFormatter>("An error occurred writing to the output stream", ex);
                    throw;
                }

                memStream.Flush();
                memStream.Position = 0;

                //read the result string from the stream
                // (see: http://docs.angularjs.org/api/ng.$http)
                string output;
                using (var reader = new StreamReader(memStream))
                {
                    output = reader.ReadToEnd();
                }

                //pre-pend the angular chars to the result
                output = ")]}',\n" + output;

                //write out the result to the original stream
                using (var writer = new StreamWriter(writeStream))
                {
                    writer.Write(output);
                }
            }
            
        }

    }
}
