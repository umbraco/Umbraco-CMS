using System;
using System.Xml;

namespace umbraco.standardFormhandlers
{
	/// <summary>
	/// Summary description for validateLogin.
	/// </summary>
	public class validateLogin : interfaces.IFormhandler
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="validateLogin"/> class.
        /// </summary>
		public validateLogin()
		{
		}

		#region IFormhandler Members

		private int _redirectID = -1;

        /// <summary>
        /// Executes the specified formhandler node.
        /// </summary>
        /// <param name="formhandlerNode">The formhandler node.</param>
        /// <returns></returns>
		public bool Execute(XmlNode formhandlerNode)
		{
			bool temp = false;
			if (helper.Request("umbracoMemberLogin") != "" && helper.Request("umbracoMemberPassword") != "") 
			{
				cms.businesslogic.member.Member m = cms.businesslogic.member.Member.GetMemberFromLoginNameAndPassword(helper.Request("umbracoMemberLogin"), helper.Request("umbracoMemberPassword"));
				if (m != null) 
				{
					System.Web.HttpContext.Current.Trace.Write("validateLogin", "Member found...");
					cms.businesslogic.member.Member.AddMemberToCache(m);
					temp = true;
				} else
					System.Web.HttpContext.Current.Trace.Write("validateLogin", "No member found...");
			} else
				System.Web.HttpContext.Current.Trace.Write("validateLogin", "No login or password requested...");
			return temp;
		}

        /// <summary>
        /// Gets the redirect ID.
        /// </summary>
        /// <value>The redirect ID.</value>
		public int redirectID
		{
			get
			{
				// TODO:  Add formMail.redirectID getter implementation
				return _redirectID;
			}
		}

		#endregion
	}
}
