using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using umbraco.cms.businesslogic.macro;

namespace umbraco.MacroEngines {

    public class ParameterDictionary : DynamicObject {

        private readonly IEnumerable<MacroPropertyModel> _parameters;

        public ParameterDictionary(IEnumerable<MacroPropertyModel> parameters) {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            _parameters = parameters;
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            return _parameters.Select(p => p.Key);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            var model = _parameters.FirstOrDefault(p => p.Key == binder.Name);
            if (model == null)
            {
                result = string.Empty;
                return true;
            }
            result = model.Value;
            return true;
        }

    }
}
