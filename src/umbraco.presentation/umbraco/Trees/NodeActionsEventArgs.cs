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
using umbraco.BusinessLogic.Utils;
using umbraco.BusinessLogic.Actions;

namespace umbraco.cms.presentation.Trees
{
    public class NodeActionsEventArgs : EventArgs
    {
        public NodeActionsEventArgs(bool isRoot, List<IAction> currActions)
        {
            AllowedActions = currActions;
            IsRoot = isRoot;
        }

        public List<IAction> AllowedActions { get; private set; }
        public bool IsRoot { get; private set; }
    }
}
