using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace umbraco.MacroEngines {

    public interface IMacroContext {
        void SetMembers(MacroModel macro, INode node);
        MacroModel Macro { get; }
        INode Node { get; }
    }

}
