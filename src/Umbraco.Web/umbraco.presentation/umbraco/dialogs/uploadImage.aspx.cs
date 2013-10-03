using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Xml;
using umbraco.BusinessLogic;
using Umbraco.Core.IO;

namespace umbraco.dialogs
{
	[Obsolete("Use the UploadMediaImage control instead")]
	public partial class uploadImage : BasePages.UmbracoEnsuredPage
	{
	    public uploadImage()
	    {
	        CurrentApp = DefaultApps.media.ToString();
	    }
	}
}
