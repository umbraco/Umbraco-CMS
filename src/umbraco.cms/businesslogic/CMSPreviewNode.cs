using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic
{
    public class CMSPreviewNode
    {
        public int NodeId { get; set; }
        public int Level { get; set; }
        public Guid Version { get; set; }
        public int ParentId { get; set; }
        public int SortOrder { get; set; }
        public string Xml { get; set; }
        public bool IsDraft { get; set; }

        public CMSPreviewNode()
        {

        }

        public CMSPreviewNode(int nodeId, Guid version, int parentId, int level, int sortOrder, string xml, bool isDraft)
        {
            NodeId = nodeId;
            Version = version;
            ParentId = parentId;
            Level = level;
            SortOrder = sortOrder;
            Xml = xml;
            IsDraft = isDraft;
        }
    }
}
