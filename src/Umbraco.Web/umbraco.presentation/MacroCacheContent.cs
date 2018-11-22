using System.Web.UI;

namespace umbraco
{
	public class MacroCacheContent
	{
		private readonly Control _control;
		private readonly string _id;

		public MacroCacheContent(Control control, string ID)
		{
			_control = control;
			_id = ID;
		}

		public string ID
		{
			get { return _id; }
		}

		public Control Content
		{
			get { return _control; }
		}
	}
}