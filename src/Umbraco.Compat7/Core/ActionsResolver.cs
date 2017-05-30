using System.Collections.Generic;
using Umbraco.Web._Legacy.Actions;
using WebCurrent = Umbraco.Web.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core
{
    public class ActionsResolver
    {
        private ActionsResolver()
        { }

        public static ActionsResolver Current { get; } = new ActionsResolver();

        public IEnumerable<IAction> Actions => WebCurrent.Actions;
    }
}
