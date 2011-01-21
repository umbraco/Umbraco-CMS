using System;
using System.Dynamic;

namespace umbraco.MacroEngines {

    public abstract class DynamicLambdaDictionary<TValue> : DynamicObject {

        protected Func<string, TValue> RequestLambda;

        protected DynamicLambdaDictionary() {}

        protected DynamicLambdaDictionary(Func<string, TValue> requestLambda) {
            if (requestLambda == null)
                throw new ArgumentNullException("requestLambda");
            RequestLambda = requestLambda;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = RequestLambda.Invoke(binder.Name);
            return true;
        }

    }

}
