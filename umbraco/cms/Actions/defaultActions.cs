using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{

    /// <summary>
    /// This permission is assigned to a node when there are no permissions assigned to the node.
    /// This is used internally to assign no permissions to a node for a user and shouldn't be used in code.
    /// </summary>
    public class ActionNull : IAction
    {
        //create singleton
        private static readonly ActionNull instance = new ActionNull();
        private ActionNull() { }
        public static ActionNull Instance
        {
            get { return instance; }
        }

        #region IAction Members

        public char Letter
        {
            get { return '-'; }
        }

        public bool ShowInNotifier
        {
            get { return false; }
        }

        public bool CanBePermissionAssigned
        {
            get { return false; }
        }

        public string Icon
        {
            get { return string.Empty; }
        }

        public string Alias
        {
            get { return string.Empty; }
        }

        public string JsFunctionName
        {
            get { return string.Empty; }
        }

        public string JsSource
        {
            get { return string.Empty; }
        }

        #endregion
    }
    
    /// <summary>
    /// This action is used as a security constraint that grants a user the ability to view nodes in a tree 
    /// that has  permissions applied to it.
    /// </summary>
    /// <remarks>
    /// This action should not be invoked. It is used as the minimum required permission to view nodes in the content tree. By
    /// granting a user this permission, the user is able to see the node in the tree but not edit the document. This may be used by other trees
    /// that support permissions in the future.
    /// </remarks>
    public class ActionBrowse : IAction
    {
        //create singleton
        private static readonly ActionBrowse instance = new ActionBrowse();
        private ActionBrowse() { }        
        public static ActionBrowse Instance
        {
            get { return instance; }
        }

        #region IAction Members

        public char Letter
        {
            get { return 'F'; }
        }

        public bool ShowInNotifier
        {
            get { return false; }
        }

        public bool CanBePermissionAssigned
        {
            get { return true; }
        }

        public string Icon
        {
            get { return ""; }
        }

        public string Alias
        {
            get { return "browse"; }
        }

        public string JsFunctionName
        {
            get { return ""; }
        }

        public string JsSource
        {
            get { return ""; }
        }

        #endregion
    }


    public class ActionLiveEdit : IAction
    {
        //create singleton
        private static readonly ActionLiveEdit instance = new ActionLiveEdit();
        private ActionLiveEdit() { }
        public static ActionLiveEdit Instance
        {
            get { return instance; }
        }

        #region IAction Members

        public char Letter
        {
            get { return ':'; }
        }

        public bool ShowInNotifier
        {
            get { return false; }
        }

        public bool CanBePermissionAssigned
        {
            get { return true; }
        }

        public string Icon
        {
            get { return ".sprLiveEdit"; }
        }

        public string Alias
        {
            get { return "liveEdit"; }
        }

        public string JsFunctionName
        {
			get
			{
				return string.Format("{0}.actionLiveEdit()", ClientTools.Scripts.GetAppActions);
			}
        }

        public string JsSource
        {
            get { return ""; }
        }

        #endregion
    }

    /// <summary>
    /// Used simply to define context menu seperator items.
    /// This should not be used directly in any code except for creating menus.
    /// </summary>
    public class ContextMenuSeperator : IAction
    {
        //create singleton
        private static readonly ContextMenuSeperator instance = new ContextMenuSeperator();
        private ContextMenuSeperator() { }
        public static ContextMenuSeperator Instance
        {
            get { return instance; }
        }

        #region IAction Members

        public char Letter
        {
            get { return ','; }
        }

        public string JsFunctionName
        {
            get { return string.Empty; }
        }
        public string JsSource
        {
            get { return string.Empty; }
        }
        public string Alias
        {
            get { return string.Empty; }
        }
        public string Icon
        {
            get { return string.Empty; }
        }
        public bool ShowInNotifier
        {
            get { return false; }
        }
        public bool CanBePermissionAssigned
        {
            get { return false; }
        }

        #endregion
    }

    /// <summary>
    /// This action is invoked upon creation of a document
    /// </summary>
    public class ActionNew : IAction {
        //create singleton
        private static readonly ActionNew m_instance = new ActionNew();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionNew() { }

        public static ActionNew Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter {
            get {
                return 'C';
            }
        }

        public string JsFunctionName {
			get
			{
				return string.Format("{0}.actionNew()", ClientTools.Scripts.GetAppActions);
			}
        }

        public string JsSource {
            get {
                return null;
            }
        }

        public string Alias {
            get {
                return "create";
            }
        }

        public string Icon {
            get {
                return ".sprNew";
            }
        }

        public bool ShowInNotifier {
            get {
                return true;
            }
        }
        public bool CanBePermissionAssigned {
            get {
                return true;
            }
        }

        #endregion
    }

    /// <summary>
    /// This action is invoked upon creation of a document
    /// </summary>
    public class ActionNewFolder : IAction {
        //create singleton
        private static readonly ActionNewFolder m_instance = new ActionNewFolder();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionNewFolder() { }

        public static ActionNewFolder Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter {
            get {
                return '!';
            }
        }

        public string JsFunctionName {
            get {
				return string.Format("{0}.actionNewFolder()", ClientTools.Scripts.GetAppActions);
            }
        }

        public string JsSource {
            get {
                return null;
            }
        }

        public string Alias {
            get {
                return "createFolder";
            }
        }

        public string Icon {
            get {
                return ".sprCreateFolder";
            }
        }

        public bool ShowInNotifier {
            get {
                return false;
            }
        }
        public bool CanBePermissionAssigned {
            get {
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// This action is invoked when a send to translate request occurs
    /// </summary>
    public class ActionSendToTranslate : IAction
    {
        //create singleton
        private static readonly ActionSendToTranslate m_instance = new ActionSendToTranslate();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionSendToTranslate() { }

        public static ActionSendToTranslate Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return '5';
            }
        }

        public string JsFunctionName
        {
            get
            {
				return string.Format("{0}.actionSendToTranslate()", ClientTools.Scripts.GetAppActions);
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
                return "sendToTranslate";
            }
        }

        public string Icon
        {
            get
            {
                return ".sprSendToTranslate";
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

    /// <summary>
    /// This action is invoked when the trash can is emptied
    /// </summary>
    public class ActionEmptyTranscan : IAction
    {
        //create singleton
        private static readonly ActionEmptyTranscan m_instance = new ActionEmptyTranscan();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionEmptyTranscan() { }

        public static ActionEmptyTranscan Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'N';
            }
        }

        public string JsFunctionName
        {
            get
            {
				return string.Format("{0}.actionEmptyTranscan()", ClientTools.Scripts.GetAppActions);
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
                return "emptyTrashcan";
            }
        }

        public string Icon
        {
            get
            {
                return ".sprBinEmpty";
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

    /// <summary>
    /// This action is invoked when a translation occurs
    /// </summary>
    public class ActionTranslate : IAction
    {
        //create singleton
        private static readonly ActionTranslate m_instance = new ActionTranslate();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionTranslate() { }

        public static ActionTranslate Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return '4';
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
                return "translate";
            }
        }

        public string Icon
        {
            get
            {
                return "sprTranslate";
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

	/// <summary>
	/// This action is invoked upon saving of a document, media, member
	/// </summary>
	public class ActionSave : IAction
	{
        //create singleton
        private static readonly ActionSave m_instance = new ActionSave();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionSave() { }

        public static ActionSave Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return '0';
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
				return "save";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprSave";
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

    /// <summary>
    /// This action is invoked when importing a document type
    /// </summary>
    public class ActionImport : IAction
    {
        //create singleton
        private static readonly ActionImport m_instance = new ActionImport();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionImport() { }

        public static ActionImport Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return '8';
            }
        }

        public string JsFunctionName
        {
            get
            {
				return string.Format("{0}.actionImport()", ClientTools.Scripts.GetAppActions);
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
                return "importDocumentType";
            }
        }

        public string Icon
        {
            get
            {
                return ".sprImportDocumentType";
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

    /// <summary>
    /// This action is invoked when exporting a document type
    /// </summary>
    public class ActionExport : IAction
    {
        //create singleton
        private static readonly ActionExport m_instance = new ActionExport();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionExport() { }

        public static ActionExport Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return '9';
            }
        }

        public string JsFunctionName
        {
            get
            {
				return string.Format("{0}.actionExport()", ClientTools.Scripts.GetAppActions);
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
                return "exportDocumentType";
            }
        }

        public string Icon
        {
            get
            {
                return ".sprExportDocumentType";
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

	/// <summary>
	/// This action is invoked upon viewing audittrailing on a document
	/// </summary>
	public class ActionAudit : IAction
	{
        //create singleton
        private static readonly ActionAudit m_instance = new ActionAudit();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionAudit() { }

        public static ActionAudit Instance
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
				return string.Format("{0}.actionAudit()", ClientTools.Scripts.GetAppActions);
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
				return "auditTrail";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprAudit";
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
				return true;
			}
		}

		#endregion
	}

	/// <summary>
	/// This action is invoked upon creation of a document, media, member
	/// </summary>
	public class ActionPackage : IAction
	{
        //create singleton
        private static readonly ActionPackage m_instance = new ActionPackage();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionPackage() { }

        public static ActionPackage Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'X';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionPackage()", ClientTools.Scripts.GetAppActions);
			}
		}

		public string JsSource
		{
			get
			{
				return	"";
			}
		}

		public string Alias
		{
			get
			{
				return "importPackage";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprPackage2";
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
	
	/// <summary>
	/// This action is invoked upon creation of a document, media, member
	/// </summary>
	public class ActionPackageCreate : IAction
	{
        //create singleton
        private static readonly ActionPackageCreate m_instance = new ActionPackageCreate();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionPackageCreate() { }

        public static ActionPackageCreate Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'Y';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionPackageCreate()", ClientTools.Scripts.GetAppActions);
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
				return "createPackage";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprPackage2";
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
	
    /// <summary>
	/// This action is invoked when a document, media, member is deleted
	/// </summary>
	public class ActionDelete : IAction
	{
        //create singleton
        private static readonly ActionDelete m_instance = new ActionDelete();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionDelete() { }

        public static ActionDelete Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'D';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionDelete()", ClientTools.Scripts.GetAppActions);
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
				return "delete";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprDelete";
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

	/// <summary>
	/// This action is invoked when a document is disabled.
	/// </summary>
	public class ActionDisable : IAction
	{
        //create singleton
        private static readonly ActionDisable m_instance = new ActionDisable();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionDisable() { }

        public static ActionDisable Instance
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
				return string.Format("{0}.actionDisable()", ClientTools.Scripts.GetAppActions);
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
				
				return "disable";
			}
		}

		public string Icon
		{
			get
			{
				
				return ".sprDelete";
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
	
    /// <summary>
	/// This action is invoked upon creation of a document, media, member
	/// </summary>
	public class ActionMove : IAction
	{
        //create singleton
        private static readonly ActionMove m_instance = new ActionMove();

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
				
				return ".sprMove";
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

	/// <summary>
	/// This action is invoked when copying a document, media, member 
	/// </summary>
	public class ActionCopy : IAction
	{
        //create singleton
        private static readonly ActionCopy m_instance = new ActionCopy();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionCopy() { }

        public static ActionCopy Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				
				return 'O';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionCopy()", ClientTools.Scripts.GetAppActions);
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
				
				return "copy";
			}
		}

		public string Icon
		{
			get
			{
				
				return ".sprCopy";
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
	
	/// <summary>
	/// This action is invoked when children to a document, media, member is being sorted
	/// </summary>
	public class ActionSort : IAction
	{
        //create singleton
        private static readonly ActionSort m_instance = new ActionSort();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionSort() { }

        public static ActionSort Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				
				return 'S';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionSort()", ClientTools.Scripts.GetAppActions);
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
				
				return "sort";
			}
		}

		public string Icon
		{
			get
			{
				
				return ".sprSort";
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

	/// <summary>
	/// This action is invoked when rights are changed on a document
	/// </summary>
	public class ActionRights : IAction
	{
        //create singleton
        private static readonly ActionRights m_instance = new ActionRights();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionRights() { }

        public static ActionRights Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				
				return 'R';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionRights()", ClientTools.Scripts.GetAppActions);
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
				
				return "rights";
			}
		}

		public string Icon
		{
			get
			{
				
				return ".sprPermission";
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

	/// <summary>
	/// This action is invoked when a document is protected or unprotected
	/// </summary>
	public class ActionProtect : IAction
	{
        //create singleton
        private static readonly ActionProtect m_instance = new ActionProtect();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionProtect() { }

        public static ActionProtect Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				
				return 'P';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionProtect()", ClientTools.Scripts.GetAppActions);
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
				
				return "protect";
			}
		}

		public string Icon
		{
			get
			{
				
				return ".sprProtect";
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

	/// <summary>
	/// This action is invoked when copying a document is being rolled back
	/// </summary>
	public class ActionRollback : IAction
	{
        //create singleton
        private static readonly ActionRollback m_instance = new ActionRollback();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionRollback() { }

        public static ActionRollback Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				
				return 'K';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionRollback()", ClientTools.Scripts.GetAppActions);
			}
		}

		public string JsSource
		{
			get
			{
				return "";
			}
		}

		public string Alias
		{
			get
			{
				
				return "rollback";
			}
		}

		public string Icon
		{
			get
			{
				
				return ".sprRollback";
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

	/// <summary>
	/// This action is invoked when a node reloads its children
	/// Concerns only the tree itself and thus you should not handle
	/// this action from without umbraco.
	/// </summary>
	public class ActionRefresh : IAction
	{
        //create singleton
        private static readonly ActionRefresh m_instance = new ActionRefresh();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionRefresh() { }

        public static ActionRefresh Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				
				return 'L';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionRefresh()", ClientTools.Scripts.GetAppActions);
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
				
				return "refreshNode";
			}
		}

		public string Icon
		{
			get
			{
				
				return ".sprRefresh";
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

	/// <summary>
	/// This action is invoked when a notification is sent 
	/// </summary>
	public class ActionNotify : IAction
	{
        //create singleton
        private static readonly ActionNotify m_instance = new ActionNotify();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionNotify() { }

        public static ActionNotify Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				
				return 'T';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionNotify()", ClientTools.Scripts.GetAppActions);
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
				return "notify";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprNotify";
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

	/// <summary>
	/// This action is invoked when copying a document or media 
	/// </summary>
	public class ActionUpdate : IAction
	{
        //create singleton
        private static readonly ActionUpdate m_instance = new ActionUpdate();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionUpdate() { }

        public static ActionUpdate Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'A';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionUpdate()", ClientTools.Scripts.GetAppActions);
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
				return "update";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprUpdate";
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

	/// <summary>
	/// This action is invoked when a document is being published
	/// </summary>
	public class ActionPublish : IAction
	{
        //create singleton
        private static readonly ActionPublish m_instance = new ActionPublish();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionPublish() { }

        public static ActionPublish Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'U';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionPublish()", ClientTools.Scripts.GetAppActions);
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
				return "publish";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprPublish";
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

	/// <summary>
	/// This action is invoked when children to a document is being sent to published (by an editor without publishrights)
	/// </summary>
	public class ActionToPublish : IAction
	{
        //create singleton
        private static readonly ActionToPublish m_instance = new ActionToPublish();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionToPublish() { }

        public static ActionToPublish Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'H';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionToPublish()", ClientTools.Scripts.GetAppActions);
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
				return "sendtopublish";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprToPublish";
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
                //SD: Changed this to true so that any user may be able to perform this action, not just a writer
				return true;
			}
		}
		#endregion
	}

	/// <summary>
	/// This action is invoked when a user logs out
	/// </summary>
	public class ActionQuit : IAction
	{
        //create singleton
        private static readonly ActionQuit m_instance = new ActionQuit();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionQuit() { }

        public static ActionQuit Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'Q';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionQuit()", ClientTools.Scripts.GetAppActions);
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
				return "logout";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprLogout";
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

	/// <summary>
	/// This action is invoked when all documents are being republished
	/// </summary>
	public class ActionRePublish : IAction
	{
        //create singleton
        private static readonly ActionRePublish m_instance = new ActionRePublish();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionRePublish() { }

        public static ActionRePublish Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'B';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionRePublish()", ClientTools.Scripts.GetAppActions);
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
				return "republish";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprPublish";
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

	/// <summary>
	/// This action is invoked when a domain is being assigned to a document
	/// </summary>
	public class ActionAssignDomain : IAction
	{
        //create singleton
        private static readonly ActionAssignDomain m_instance = new ActionAssignDomain();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionAssignDomain() { }

        public static ActionAssignDomain Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'I';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionAssignDomain()", ClientTools.Scripts.GetAppActions);
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
				return "assignDomain";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprDomain";
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
				return true;
			}
		}
		#endregion
	}



	/// <summary>
	/// This action is invoked when a document is being unpublished
	/// </summary>
	public class ActionUnPublish : IAction
	{
        //create singleton
        private static readonly ActionUnPublish m_instance = new ActionUnPublish();

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance).
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionUnPublish() { }

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
				return ".sprDelete";
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
