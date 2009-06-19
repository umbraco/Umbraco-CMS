using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace umbraco.presentation.ClientDependency
{
	public class ClientDependencyInclude : Control
	{

		public ClientDependencyInclude()
		{
			Type = ClientDependencyType.Javascript;
		}

		public ClientDependencyType Type { get; set; }
		public string File { get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (string.IsNullOrEmpty(File))
				throw new NullReferenceException("Both File and Type properties must be set");
		}

	}
}
