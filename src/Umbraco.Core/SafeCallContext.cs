using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core.Persistence;

namespace Umbraco.Core
{
    internal class SafeCallContext : IDisposable
    {
        private static readonly List<Func<object>> EnterFuncs = new List<Func<object>>();
        private static readonly List<Action<object>> ExitActions = new List<Action<object>>();
        private static int _count;
        private readonly List<object> _objects;
        private bool _disposed;

        public static void Register(Func<object> enterFunc, Action<object> exitAction)
        {
            if (enterFunc == null) throw new ArgumentNullException("enterFunc");
            if (exitAction == null) throw new ArgumentNullException("exitAction");

            lock (EnterFuncs)
            {
                if (_count > 0) throw new Exception("Busy.");
                EnterFuncs.Add(enterFunc);
                ExitActions.Add(exitAction);
            }
        }

        public static void ClearCallContext()
        {
            lock (EnterFuncs)
            {
                var ignore = EnterFuncs.Select(x => x()).ToList();
            }
        }

        // tried to make the UmbracoDatabase serializable but then it leaks to weird places
        // in ReSharper and so on, where Umbraco.Core is not available. Tried to serialize
        // as an object instead but then it comes *back* deserialized into the original context
        // as an object and of course it breaks everything. Cannot prevent this from flowing,
        // and ExecutionContext.SuppressFlow() works for threads but not domains. and we'll
        // have the same issue with anything that toys with logical call context... so this
        // class let them register & deal with the situation (removing themselves)

        public SafeCallContext()
        {
            lock (EnterFuncs)
            {
                _count++;
                _objects = EnterFuncs.Select(x => x()).ToList();
            }
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException("this");
            _disposed = true;
            lock (EnterFuncs)
            {
                for (var i = 0; i < ExitActions.Count; i++)
                    ExitActions[i](_objects[i]);
                _count--;
            }
        }
    }
}