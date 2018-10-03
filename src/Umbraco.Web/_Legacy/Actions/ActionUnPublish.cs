using System;

namespace Umbraco.Web._Legacy.Actions
{



    /// <summary>
    /// This action is invoked when a document is being unpublished
    /// </summary>
    public class ActionUnpublish : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionUnpublish m_instance = new ActionUnpublish();
#pragma warning restore 612,618

        public static ActionUnpublish Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'Z';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return "";
            }
        }

        public string JsSource
        {
            get
            {
                return null;
            }
        }

        public string Alias
        {
            get
            {
                return "unpublish";
            }
        }

        public string Icon
        {
            get
            {
                return "circle-dotted";
            }
        }

        public bool ShowInNotifier
        {
            get
            {
                return false;
            }
        }
        public bool CanBePermissionAssigned
        {
            get
            {
                return false;
            }
        }
        #endregion
    }

}
