using System;
using System.Collections.Generic;
using umbraco.interfaces;

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
