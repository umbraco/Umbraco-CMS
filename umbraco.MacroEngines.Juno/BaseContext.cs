using System;
using System.Web.WebPages;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
    public abstract class BaseContext<T> : WebPage, IMacroContext
    {
        private MacroModel _macro;
        protected T CurrentModel;
        protected DynamicLambdaDictionary<string> ParameterDictionary;
        protected DynamicLambdaDictionary<string> CultureDictionary;

        public dynamic Parameter { get { return ParameterDictionary; } }
        public dynamic Dictionary { get { return CultureDictionary; } }

        public MacroModel Macro { get { return _macro; } }
        public T Current { get { return CurrentModel; } }
        public new dynamic Model { get { return CurrentModel; } }

        public virtual void SetMembers(MacroModel macro, INode node) {
            if (macro == null)
                throw new ArgumentNullException("macro");
            if (node == null)
                throw new ArgumentNullException("node");
            _macro = macro;
            ParameterDictionary = new ParameterDictionary(macro.Properties);
            CultureDictionary = new CultureDictionary();
        }
    }
}
