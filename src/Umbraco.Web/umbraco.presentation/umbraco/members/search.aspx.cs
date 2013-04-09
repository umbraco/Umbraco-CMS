using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Web.Security;
using umbraco.cms.businesslogic.member;
using umbraco.DataLayer.SqlHelpers;
using umbraco.BusinessLogic;
using Examine;

namespace umbraco.presentation.members
{


    public partial class search : BasePages.UmbracoEnsuredPage
    {
        public search()
        {
            CurrentApp = DefaultApps.member.ToString();
        }
    }
}
