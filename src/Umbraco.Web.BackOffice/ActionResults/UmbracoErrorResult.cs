using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.BackOffice.ActionResults;

public class UmbracoErrorResult : ObjectResult
{
    public UmbracoErrorResult(HttpStatusCode statusCode, string message) : this(statusCode, new MessageWrapper(message))
    {
    }

    public UmbracoErrorResult(HttpStatusCode statusCode, object value) : base(value) => StatusCode = (int)statusCode;

    private class MessageWrapper
    {
        public MessageWrapper(string message) => Message = message;

        public string Message { get; }
    }
}
