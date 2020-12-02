using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Events;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Used to emit events for editor models in the back office
    /// </summary>
    public sealed class EditorModelEventManager
    {
        public static event TypedEventHandler<ResultExecutedContext, EditorModelEventArgs<ContentItemDisplay>> SendingContentModel;
        public static event TypedEventHandler<ResultExecutedContext, EditorModelEventArgs<MediaItemDisplay>> SendingMediaModel;
        public static event TypedEventHandler<ResultExecutedContext, EditorModelEventArgs<MemberDisplay>> SendingMemberModel;
        public static event TypedEventHandler<ResultExecutedContext, EditorModelEventArgs<UserDisplay>> SendingUserModel;

        public static event TypedEventHandler<ResultExecutedContext, EditorModelEventArgs<IEnumerable<Tab<IDashboardSlim>>>> SendingDashboardSlimModel;

        private static void OnSendingDashboardModel(ResultExecutedContext sender, EditorModelEventArgs<IEnumerable<Tab<IDashboardSlim>>> e)
        {
            var handler = SendingDashboardSlimModel;
            handler?.Invoke(sender, e);
        }

        private static void OnSendingUserModel(ResultExecutedContext sender, EditorModelEventArgs<UserDisplay> e)
        {
            var handler = SendingUserModel;
            handler?.Invoke(sender, e);
        }

        private static void OnSendingContentModel(ResultExecutedContext sender, EditorModelEventArgs<ContentItemDisplay> e)
        {
            var handler = SendingContentModel;
            handler?.Invoke(sender, e);
        }

        private static void OnSendingMediaModel(ResultExecutedContext sender, EditorModelEventArgs<MediaItemDisplay> e)
        {
            var handler = SendingMediaModel;
            handler?.Invoke(sender, e);
        }

        private static void OnSendingMemberModel(ResultExecutedContext sender, EditorModelEventArgs<MemberDisplay> e)
        {
            var handler = SendingMemberModel;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Based on the type, emit's a specific event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void EmitEvent(ResultExecutedContext sender, EditorModelEventArgs e)
        {
            if (e.Model is ContentItemDisplay)
                OnSendingContentModel(sender, new EditorModelEventArgs<ContentItemDisplay>(e));

            if (e.Model is MediaItemDisplay)
                OnSendingMediaModel(sender, new EditorModelEventArgs<MediaItemDisplay>(e));

            if (e.Model is MemberDisplay)
                OnSendingMemberModel(sender, new EditorModelEventArgs<MemberDisplay>(e));

            if (e.Model is UserDisplay)
                OnSendingUserModel(sender, new EditorModelEventArgs<UserDisplay>(e));

            if (e.Model is IEnumerable<Tab<IDashboardSlim>>)
                OnSendingDashboardModel(sender, new EditorModelEventArgs<IEnumerable<Tab<IDashboardSlim>>>(e));
        }
    }
}
