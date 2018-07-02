﻿using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked upon creation of a document, media, member
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.StructureCategory)]
    public class ActionMove : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionMove m_instance = new ActionMove();
#pragma warning restore 612,618

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionMove() { }

        public static ActionMove Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'M';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionMove()", ClientTools.Scripts.GetAppActions);
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

                return "move";
            }
        }

        public string Icon
        {
            get
            {

                return "enter";
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
