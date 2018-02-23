using System;
using umbraco.BasePages;
using umbraco.interfaces;

namespace umbraco.cms.Actions
{
    public class ActionExportMember: IAction
    {
        //create singleton
#pragma warning disable 612, 618
        private static readonly ActionExportMember m_instance = new ActionExportMember();
#pragma warning restore 612, 618

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionExportMember() { }

        public static ActionExportMember Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'E';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionExportMember()", ClientTools.Scripts.GetAppActions);
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
                return "export";
            }
        }

        public string Icon
        {
            get
            {
                return "download-alt";
            }
        }

        public bool ShowInNotifier
        {
            get
            {
                return true;
            }
        }
        public bool CanBePermissionAssigned
        {
            get
            {
                return true;
            }
        }
        #endregion

    }
}
