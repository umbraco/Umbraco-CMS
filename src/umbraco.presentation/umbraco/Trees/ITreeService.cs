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
using System.Collections.Specialized;

namespace umbraco.cms.presentation.Trees
{
	/// <summary>
	/// All Trees rely on the properties of an ITreeService interface. This has been created to avoid having trees
	/// dependant on the HttpContext
	/// </summary>
	public interface ITreeService
	{
		/// <summary>
		/// The NodeKey is a string representation of the nodeID. Generally this is used for tree's whos node's unique key value is a string in instead 
		/// of an integer such as folder names.
		/// </summary>
		string NodeKey { get; }
		int StartNodeID { get; }
		bool ShowContextMenu { get; }
		bool IsDialog { get; }
		TreeDialogModes DialogMode { get; }
		string FunctionToCall { get; }
	}
}
