using System;
using System.Dynamic;

namespace umbraco.MacroEngines {

    public abstract class LambdaDictionary<TValue> {

        protected Func<string, TValue> RequestLambda;

        protected LambdaDictionary() {}

        protected LambdaDictionary(Func<string, TValue> requestLambda) {
            if (requestLambda == null)
                throw new ArgumentNullException("requestLambda");
            RequestLambda = requestLambda;
        }

        public TValue this[string alias] {
            get {
                return RequestLambda.Invoke(alias);
            }
        }

    }

}
