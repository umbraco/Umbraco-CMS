using System;

namespace Umbraco.Web._Legacy.Actions
{



    /// <summary>
    /// This action is invoked when a document is being unpublished
    /// </summary>
    public class ActionUnPublish : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionUnPublish m_instance = new ActionUnPublish();
#pragma warning restore 612,618

        public static ActionUnPublish Instance
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
