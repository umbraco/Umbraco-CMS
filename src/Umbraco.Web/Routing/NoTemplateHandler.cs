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
			var response = context.Response;

			response.Clear();
			response.Write("");
			response.End();
		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}