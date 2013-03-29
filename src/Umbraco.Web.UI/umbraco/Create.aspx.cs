using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.UI.Umbraco
{
    public partial class CreateDialog : global::umbraco.cms.presentation.Create
    {

        //protected override void OnLoad(EventArgs e)
        //{
        //    if (SecurityCheck(Request.QueryString["nodeType"]))
        //    {
        //        //if we're allowed, then continue
        //        base.OnLoad(e);
        //    }
        //    else
        //    {
        //        //otherwise show an error
        //        UI.Visible = false;
        //        AccessError.Visible = true;
        //    }
        //}

        //private bool SecurityCheck(string treeAlias)
        //{
        //    var tree = TreeDefinitionCollection.Instance.FindTree(treeAlias);
        //    if (tree != null)
        //    {
        //        //does the current user have access to the current app?
        //        var user = this.getUser();
        //        var userApps = user.Applications;
        //        return userApps.Any(x => x.alias.InvariantEquals(tree.App.alias));
        //    }
        //    return false;
        //}

    }
}