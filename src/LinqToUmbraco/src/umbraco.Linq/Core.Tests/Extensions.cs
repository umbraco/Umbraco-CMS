using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeMock.ArrangeActAssert;

namespace umbraco.Linq.Core.Tests
{
    public static class Extensions
    {
        public static ActionRepeater<TReturn> WillReturnRepeat<TReturn>(this IPublicNonVoidMethodHandler<TReturn> ret, TReturn value, int numberOfReturns)
        {
            for (var i = 0; i < numberOfReturns; i++)
                ret.WillReturn(value);

            return new ActionRepeater<TReturn>(ret);
        }

        public static ActionRepeater<TReturn> CallOriginalRepeat<TReturn>(this IPublicNonVoidMethodHandler<TReturn> ret, int numberOfReturns)
        {
            for (var i = 0; i < numberOfReturns; i++)
                ret.CallOriginal();

            return new ActionRepeater<TReturn>(ret);
        }
    }

    public class ActionRepeater<TReturn>
    {
        private IPublicNonVoidMethodHandler<TReturn> _actionRepeater;
        public ActionRepeater(IPublicNonVoidMethodHandler<TReturn> actionRepeater)
        {
            _actionRepeater = actionRepeater;
        }

        public IPublicNonVoidMethodHandler<TReturn> AndThen()
        {
            return _actionRepeater;
        }
    }
}

