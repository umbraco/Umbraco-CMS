using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace umbraco.MacroEngines {

    public abstract class DynamicNodeContext : IMacroContext {

        private MacroModel _macro;
        private DynamicNode _dynamicNode;
        private ParameterDictionary _parameters;

        public dynamic Parameters { get { return _parameters; } }
        public MacroModel Macro { get { return _macro; } }
        public DynamicNode Current { get { return _dynamicNode; } }

        public void SetMembers(MacroModel macro, INode node) {
            _macro = macro;
            _dynamicNode = new DynamicNode(node);
            _parameters = new ParameterDictionary(macro.Properties);
        }

    }

}
