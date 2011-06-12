using System;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{

    public abstract class DynamicNodeContext : BaseContext<DynamicNode>
    {

        public override void SetMembers(MacroModel macro, INode node)
        {
            if (macro == null)
                throw new ArgumentNullException("macro");
            if (node == null)
                throw new ArgumentNullException("node");
            var backingItem = new DynamicBackingItem(node);
            var dynamicNode = new DynamicNode(backingItem);
            CurrentModel = dynamicNode;
            base.SetMembers(macro, node);
        }

    }

}
