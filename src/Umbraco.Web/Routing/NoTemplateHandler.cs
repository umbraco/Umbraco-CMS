using System.Web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Gets executed when there is no template assigned to a request and there is no hijacked MVC route
	/// </summary>
	internal class NoTemplateHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			WriteOutput(context);
		}

		internal void WriteOutput(HttpContext context)
		{
			context.Response.Clear();
			context.Response.Write("");
			context.Response.End();
		}

		public bool IsReusable
		{
			get
			{
				//yes this is reusable since it always returns the same thing
				return true;
			}
		}
	}
}