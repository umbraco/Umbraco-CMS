using System;

namespace Umbraco.Web.Editors
{
    public sealed class EditorModelEventArgs<T> : EditorModelEventArgs
    {
        private readonly EditorModelEventArgs _baseArgs;
        private T _model;

        public EditorModelEventArgs(EditorModelEventArgs baseArgs)
            : base(baseArgs.Model, baseArgs.UmbracoContext)
        {
            _baseArgs = baseArgs;
            Model = (T)baseArgs.Model;
        }

        public EditorModelEventArgs(T model, UmbracoContext umbracoContext)
            : base(model, umbracoContext)
        {
            Model = model;
        }

        public new T Model
        {
            get => _model;
            set
            {
                _model = value;
                if (_baseArgs != null)
                    _baseArgs.Model = _model;
            }
        }
    }

    public class EditorModelEventArgs : EventArgs
    {
        public EditorModelEventArgs(object model, UmbracoContext umbracoContext)
        {
            Model = model;
            UmbracoContext = umbracoContext;
        }

        public object Model { get; set; }
        public UmbracoContext UmbracoContext { get; }
    }
}
