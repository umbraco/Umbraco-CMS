using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace umbraco.presentation.ClientDependency
{
    public class ClientDependencyInclude : Control, IClientDependencyFile
	{

		public ClientDependencyInclude()
		{
			DependencyType = ClientDependencyType.Javascript;
            Priority = DefaultPriority;
		}

        /// <summary>
        /// If a priority is not set, the default will be 100.
        /// </summary>
        /// <remarks>
        /// This will generally mean that if a developer doesn't specify a priority it will come after all other dependencies that 
        /// have unless the priority is explicitly set above 100.
        /// </remarks>
        protected const int DefaultPriority = 100;

		public ClientDependencyType DependencyType { get; set; }
		public string FilePath { get; set; }
        public string PathNameAlias { get; set; }
        public string CompositeGroupName { get; set; }
        public int Priority { get; set; }
        public string InvokeJavascriptMethodOnLoad { get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (string.IsNullOrEmpty(FilePath))
				throw new NullReferenceException("Both File and Type properties must be set");
		}

	}
}
