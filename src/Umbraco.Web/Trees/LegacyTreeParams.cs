using System.Collections.Generic;
using System.Linq;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.Trees
{

    //Temporary, but necessary until we refactor trees in general
    internal class LegacyTreeParams : ITreeService
    {
        public LegacyTreeParams()
        {

        }

        public LegacyTreeParams(IEnumerable<KeyValuePair<string, string>> formCollection)
        {
            var p = TreeRequestParams.FromDictionary(formCollection.ToDictionary(x => x.Key, x => x.Value));
            NodeKey = p.NodeKey;
            StartNodeID = p.StartNodeID;
            ShowContextMenu = p.ShowContextMenu;
            IsDialog = p.IsDialog;
            DialogMode = p.DialogMode;
            FunctionToCall = p.FunctionToCall;
        }

        public string NodeKey { get; set; }
        public int StartNodeID { get; set; }
        public bool ShowContextMenu { get; set; }
        public bool IsDialog { get; set; }
        public TreeDialogModes DialogMode { get; set; }
        public string FunctionToCall { get; set; }
    }
}