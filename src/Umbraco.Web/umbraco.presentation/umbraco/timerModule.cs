using System.Data;
using System.Threading;
using System.Web;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;

namespace umbraco
{
	/// <summary>
	/// Summary description for timerModule.
	/// </summary>
	public class timerModule : IHttpModule
	{
		protected Timer t;

		#region IHttpModule Members

		public void Init(HttpApplication context)
		{
            LogHelper.Debug<timerModule>("timer init");

			t = new Timer(new TimerCallback(this.doStuff), context.Context, 1000, 1000);
		}

		private void doStuff(object sender)
		{
            LogHelper.Debug<timerModule>("timer ping");
		}

		public void Dispose()
		{
			if(t != null)
				t.Dispose();
			t = null;
		}

		#endregion
	}
}