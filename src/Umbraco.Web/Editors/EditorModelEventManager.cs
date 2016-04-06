using System;
using System.Web.Http.Filters;
using Umbraco.Core.Events;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors
{
    public class EditorModelEventArgs : EventArgs
    {
        public EditorModelEventArgs(object model, UmbracoContext umbracoContext)
        {
            Model = model;
            UmbracoContext = umbracoContext;
        }

        public object Model { get; private set; }
        public UmbracoContext UmbracoContext { get; private set; }
    }

    public sealed class EditorModelEventArgs<T> : EditorModelEventArgs
    {
        public EditorModelEventArgs(EditorModelEventArgs baseArgs)
            : base(baseArgs.Model, baseArgs.UmbracoContext)
        {
            Model = (T)baseArgs.Model;
        }

        public EditorModelEventArgs(T model, UmbracoContext umbracoContext)
            : base(model, umbracoContext)
        {
            Model = model;
        }

        public new T Model { get; private set; }
    }

    /// <summary>
    /// Used to emit events for editor models in the back office
    /// </summary>
    public sealed class EditorModelEventManager
    {
        public static event TypedEventHandler<HttpActionExecutedContext, EditorModelEventArgs<ContentItemDisplay>> SendingContentModel;
        public static event TypedEventHandler<HttpActionExecutedContext, EditorModelEventArgs<MediaItemDisplay>> SendingMediaModel;
        public static event TypedEventHandler<HttpActionExecutedContext, EditorModelEventArgs<MemberDisplay>> SendingMemberModel;

        private static void OnSendingContentModel(HttpActionExecutedContext sender, EditorModelEventArgs<ContentItemDisplay> e)
        {
            var handler = SendingContentModel;
            if (handler != null) handler(sender, e);
        }

        private static void OnSendingMediaModel(HttpActionExecutedContext sender, EditorModelEventArgs<MediaItemDisplay> e)
        {
            var handler = SendingMediaModel;
            if (handler != null) handler(sender, e);
        }

        private static void OnSendingMemberModel(HttpActionExecutedContext sender, EditorModelEventArgs<MemberDisplay> e)
        {
            var handler = SendingMemberModel;
            if (handler != null) handler(sender, e);
        }

        /// <summary>
        /// Based on the type, emit's a specific event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void EmitEvent(HttpActionExecutedContext sender, EditorModelEventArgs e)
        {
            var contentItemDisplay = e.Model as ContentItemDisplay;
            if (contentItemDisplay != null)
            {
                OnSendingContentModel(sender, new EditorModelEventArgs<ContentItemDisplay>(e));
            }

            var mediaItemDisplay = e.Model as MediaItemDisplay;
            if (mediaItemDisplay != null)
            {
                OnSendingMediaModel(sender, new EditorModelEventArgs<MediaItemDisplay>(e));
            }

            var memberItemDisplay = e.Model as MemberDisplay;
            if (memberItemDisplay != null)
            {
                OnSendingMemberModel(sender, new EditorModelEventArgs<MemberDisplay>(e));
            }
        }
        
    }
}