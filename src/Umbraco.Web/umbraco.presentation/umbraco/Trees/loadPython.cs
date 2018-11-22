using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;
using Umbraco.Core;


namespace umbraco
{
	/// <summary>
	/// Handles loading of python items into the developer application tree
	/// </summary>
	[Obsolete("This tree is no longer shipped by default, it will be removed in the future")]
    [Tree(Constants.Applications.Developer, "python", "Python Files", sortOrder: 4)]
    public class loadPython : loadDLRScripts
	{
        public loadPython(string application) : base(application) { }
    }
    
}
