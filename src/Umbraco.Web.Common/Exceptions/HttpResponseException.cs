using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace Umbraco.Web.Common.Exceptions
{
    [Serializable]
    public class HttpResponseException : Exception
    {
        public HttpResponseException(HttpStatusCode status = HttpStatusCode.InternalServerError, object value = null)
        {
            Status = status;
            Value = value;
        }

        public HttpStatusCode Status { get; set; }
        public object Value { get; set; }

        public IDictionary<string, string> AdditionalHeaders { get; } = new Dictionary<string, string>();


        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">info</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Status), Enum.GetName(typeof(HttpStatusCode), Status));
            info.AddValue(nameof(Value), Value);
            info.AddValue(nameof(AdditionalHeaders), AdditionalHeaders);

            base.GetObjectData(info, context);
        }

        public static HttpResponseException CreateValidationErrorResponse(object model)
        {
            return new HttpResponseException(HttpStatusCode.BadRequest, model)
            {
                AdditionalHeaders =
                {
                    ["X-Status-Reason"] = "Validation failed"
                }
            };
        }
    }
}
