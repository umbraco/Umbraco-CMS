using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.BackOffice.ActionResults;

public class JavaScriptResult : ContentResult
{
    public JavaScriptResult(string? script)
    {
        Content = script;
        ContentType = "application/javascript";
    }
}
