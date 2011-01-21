using System;
using System.Linq;
using System.Collections.Generic;
using umbraco.cms.businesslogic.macro;

namespace umbraco.MacroEngines {
    public class ParameterDictionary : DynamicLambdaDictionary<string> {
        public ParameterDictionary(IEnumerable<MacroPropertyModel> properties) {
            RequestLambda = new Func<string, string>(key => {
                    var model = properties.FirstOrDefault(p => p.Key == key);
                    if (model == null)
                        return string.Empty;
                    return model.Value;
            });
        }

        public ParameterDictionary(Func<string, string> request) : base(request) {}
    }
}
