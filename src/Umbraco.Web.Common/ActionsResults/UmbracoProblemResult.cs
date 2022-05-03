using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.ActionsResults;

// TODO: What is the purpose of this? Doesn't seem to add any benefit
public class UmbracoProblemResult : ObjectResult
{
    public UmbracoProblemResult(string message, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
        : base(new { Message = message }) => StatusCode = (int)httpStatusCode;
}
