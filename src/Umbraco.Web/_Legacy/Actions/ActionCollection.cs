using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web._Legacy.Actions
{
    public class ActionCollection : IBuilderCollection<IAction>
    {
        private Func<IEnumerable<Type>> _producer;
        private readonly object _locker = new object();
        private IAction[] _items;

        internal ActionCollection(Func<IEnumerable<Type>> producer)
        {
            _producer = producer;
        }

        internal T GetAction<T>()
            where T : IAction
        {
            return this.OfType<T>().SingleOrDefault();
        }

        internal IEnumerable<IAction> GetByLetters(IEnumerable<string> letters)
        {
            var all = this.ToArray();
            return letters.Select(x => all.FirstOrDefault(y => y.Letter.ToString(CultureInfo.InvariantCulture) == x))
                .WhereNotNull()
                .ToArray();
        }

        private IAction[] Items
        {
            get
            {
                lock (_locker)
                {
                    if (_items != null) return _items;

                    var actions = new List<IAction>();
                    foreach (var type in _producer())
                    {
                        var getter = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                        var instance = getter == null
                            ? Activator.CreateInstance(type) as IAction
                            : getter.GetValue(null, null) as IAction;
                        if (instance == null) continue;
                        actions.Add(instance);
                    }

                    return _items = actions.ToArray();
                }
            }
        }

        internal void Reset(Func<IEnumerable<Type>> producer)
        {
            lock (_locker)
            {
                _items = null;
                _producer = producer;
            }
        }

        public int Count => Items.Length;

        public IEnumerator<IAction> GetEnumerator()
        {
            return ((IEnumerable<IAction>) Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
