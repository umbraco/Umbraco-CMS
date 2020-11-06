using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Web.BackOffice.ActionResults
{
    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            this.Content = script;
            this.ContentType = "application/javascript";
        }
    }
}
