using System;
using System.Web.WebPages;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace umbraco.MacroEngines {

    public abstract class DynamicNodeContext : WebPage, IMacroContext {

        private MacroModel _macro;
        private DynamicNode _dynamicNode;
        private DynamicLambdaDictionary<string> _parameter;
        private DynamicLambdaDictionary<string> _culture;

        public dynamic Parameter { get { return _parameter; } }
        public dynamic Dictionary { get { return _culture; } }

        public MacroModel Macro { get { return _macro; } }
        public DynamicNode Current { get { return _dynamicNode; } }
        public new dynamic Model { get { return _dynamicNode; } }

        public void SetMembers(MacroModel macro, INode node) {
            if (macro == null)
                throw new ArgumentNullException("macro");
            if (node == null)
                throw new ArgumentNullException("node");
            _macro = macro;
            _dynamicNode = new DynamicNode(node);
            _parameter = new ParameterDictionary(macro.Properties);
            _culture = new CultureDictionary();
        }

    }

}
