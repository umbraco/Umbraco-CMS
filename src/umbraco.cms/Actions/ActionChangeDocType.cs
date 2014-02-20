using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
    /// <summary>
    /// This action is invoked when the document type of a piece of content is changed
    /// </summary>
    public class ActionChangeDocType : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionChangeDocType m_instance = new ActionChangeDocType();
#pragma warning restore 612,618

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionChangeDocType() { }

        public static ActionChangeDocType Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return '7';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionChangeDocType()", ClientTools.Scripts.GetAppActions);
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

                return "changeDocType";
            }
        }

        public string Icon
        {
            get
            {

                return "axis-rotation-2";
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
