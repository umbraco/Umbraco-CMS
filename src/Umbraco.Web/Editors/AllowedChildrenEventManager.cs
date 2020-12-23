using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http.Filters;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Events;
using Umbraco.Web.Models.ContentEditing;
namespace Umbraco.Web.Editors
{
    public sealed class AllowedChildrenEventManager
    {
        public static event TypedEventHandler<HttpActionExecutedContext, AllowedChildrenEventArgs<System.Collections.Generic.IEnumerable<Umbraco.Web.Models.ContentEditing.ContentTypeBasic>>> SendingAllowedChildrenModel;

        private static void OnSendingAllowedChildrenModel(HttpActionExecutedContext sender, AllowedChildrenEventArgs<System.Collections.Generic.IEnumerable<Umbraco.Web.Models.ContentEditing.ContentTypeBasic>> e)
        {
            var handler = SendingAllowedChildrenModel;
            handler?.Invoke(sender, e);
        }
        /// <summary>
        /// Emit the Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void EmitEvent(HttpActionExecutedContext sender, AllowedChildrenEventArgs e)
        {
               OnSendingAllowedChildrenModel(sender, new AllowedChildrenEventArgs<System.Collections.Generic.IEnumerable<Umbraco.Web.Models.ContentEditing.ContentTypeBasic>>(e));
        }
    }
}
