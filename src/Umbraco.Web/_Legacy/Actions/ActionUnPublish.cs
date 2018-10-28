using System;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{

    /// <summary>
    /// This action is invoked when a document is being unpublished
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionUnpublish : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionUnpublish m_instance = new ActionUnpublish();
#pragma warning restore 612,618

        public static ActionUnpublish Instance => m_instance;

        public char Letter => 'Z';
        public string JsFunctionName => "";
        public string JsSource => null;
        public string Alias => "unpublish";
        public string Icon => "circle-dotted";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
        public bool OpensDialog => false;

    }

}
