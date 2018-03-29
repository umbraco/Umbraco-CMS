using System.Web.Http.Filters;
using Umbraco.Core.Events;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Used to emit events for editor models in the back office
    /// </summary>
    public sealed class EditorModelEventManager
    {
        public static event TypedEventHandler<HttpActionExecutedContext, EditorModelEventArgs<ContentItemDisplay>> SendingContentModel;
        public static event TypedEventHandler<HttpActionExecutedContext, EditorModelEventArgs<MediaItemDisplay>> SendingMediaModel;
        public static event TypedEventHandler<HttpActionExecutedContext, EditorModelEventArgs<MemberDisplay>> SendingMemberModel;
        public static event TypedEventHandler<HttpActionExecutedContext, EditorModelEventArgs<UserDisplay>> SendingUserModel;

        private static void OnSendingUserModel(HttpActionExecutedContext sender, EditorModelEventArgs<UserDisplay> e)
        {
            var handler = SendingUserModel;
            handler?.Invoke(sender, e);
        }

        private static void OnSendingContentModel(HttpActionExecutedContext sender, EditorModelEventArgs<ContentItemDisplay> e)
        {
            var handler = SendingContentModel;
            handler?.Invoke(sender, e);
        }

        private static void OnSendingMediaModel(HttpActionExecutedContext sender, EditorModelEventArgs<MediaItemDisplay> e)
        {
            var handler = SendingMediaModel;
            handler?.Invoke(sender, e);
        }

        private static void OnSendingMemberModel(HttpActionExecutedContext sender, EditorModelEventArgs<MemberDisplay> e)
        {
            var handler = SendingMemberModel;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Based on the type, emit's a specific event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void EmitEvent(HttpActionExecutedContext sender, EditorModelEventArgs e)
        {
            if (e.Model is ContentItemDisplay)
                OnSendingContentModel(sender, new EditorModelEventArgs<ContentItemDisplay>(e));

            if (e.Model is MediaItemDisplay)
                OnSendingMediaModel(sender, new EditorModelEventArgs<MediaItemDisplay>(e));

            if (e.Model is MemberDisplay)
                OnSendingMemberModel(sender, new EditorModelEventArgs<MemberDisplay>(e));

            if (e.Model is UserDisplay)
                OnSendingUserModel(sender, new EditorModelEventArgs<UserDisplay>(e));
        }
    }
}
