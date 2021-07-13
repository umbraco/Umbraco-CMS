using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Editors
{
    public sealed class AllowedChildrenEventArgs<T> : AllowedChildrenEventArgs
    {
        private readonly AllowedChildrenEventArgs _baseArgs;
        private T _model;

        public AllowedChildrenEventArgs(AllowedChildrenEventArgs baseArgs)
            : base(baseArgs.Model, baseArgs.UmbracoContext, baseArgs.ContentId)
        {
            _baseArgs = baseArgs;
            Model = (T)baseArgs.Model;
            ContentId = baseArgs.ContentId;
        }

        public AllowedChildrenEventArgs(T model, UmbracoContext umbracoContext, int? contentId)
            : base(model, umbracoContext,contentId)
        {
            Model = model;
            ContentId = contentId;
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

    public class AllowedChildrenEventArgs : EventArgs
    {
        public AllowedChildrenEventArgs(object model, UmbracoContext umbracoContext, int? contentId)
        {
            Model = model;
            ContentId = contentId;
            UmbracoContext = umbracoContext;
        }

        public object Model { get; set; }
        public int? ContentId { get; set; }
        public UmbracoContext UmbracoContext { get; }
    }
}
