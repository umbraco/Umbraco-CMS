using System;

namespace Umbraco.Web.Editors
{
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
}