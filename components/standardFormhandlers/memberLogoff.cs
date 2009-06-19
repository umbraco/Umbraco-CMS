using System;
using System.Xml;

namespace umbraco.standardFormhandlers
{
	/// <summary>
	/// Summary description for memberLogoff.
	/// </summary>
	public class memberLogoff : interfaces.IFormhandler	
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="memberLogoff"/> class.
        /// </summary>
		public memberLogoff()
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
		    int _currentMemberId = cms.businesslogic.member.Member.CurrentMemberId();
            if (_currentMemberId > 0 )
				cms.businesslogic.member.Member.ClearMemberFromClient(_currentMemberId);
			return true;
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
