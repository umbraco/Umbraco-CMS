using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.ActionsResults
{
    public class UmbracoProblemResult : ObjectResult
    {
        public UmbracoProblemResult(string message, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) : base(new {Message = message})
        {
            StatusCode = (int) httpStatusCode;
        }
    }
}
