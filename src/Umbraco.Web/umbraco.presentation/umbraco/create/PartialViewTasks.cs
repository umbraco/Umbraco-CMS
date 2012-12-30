using System.IO;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using umbraco.BasePages;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// The UI 'tasks' for the create dialog and delete processes
	/// </summary>
	[UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
	public class PartialViewTasks : interfaces.ITaskReturnUrl
	{
		private string _alias;
		private int _parentId;
		private int _typeId;
		private int _userId;

		private const string EditViewFile = "Settings/Views/EditView.aspx";
		private readonly string _basePath = SystemDirectories.MvcViews + "/Partials/";

		public int UserId
		{
			set { _userId = value; }
		}
		public int TypeID
		{
			set { _typeId = value; }
			get { return _typeId; }
		}


		public string Alias
		{
			set { _alias = value; }
			get { return _alias; }
		}

		public int ParentID
		{
			set { _parentId = value; }
			get { return _parentId; }
		}

		public bool Save()
		{
			var fileName = _alias + ".cshtml";
			var fullFilePath = IOHelper.MapPath(_basePath + fileName);
			
			//return the link to edit the file if it already exists
			if (File.Exists(fullFilePath))
			{
				_returnUrl = string.Format(EditViewFile + "?file={0}", fileName);
				return true;
			}	

			//create the file
			using (var sw = File.CreateText(fullFilePath))
			{
				//write out the template header
				sw.Write("@inherits ");
				sw.Write(typeof(UmbracoViewPage<>).FullName.TrimEnd("`1"));
				sw.Write("<dynamic>");
			}
			_returnUrl = string.Format(EditViewFile + "?file={0}", fileName);
			return true;
		}

		public bool Delete()
		{
			var path = IOHelper.MapPath(_basePath + _alias.TrimStart('/'));

			if (File.Exists(path))
				File.Delete(path);
			else if (Directory.Exists(path))
				Directory.Delete(path, true);

			LogHelper.Info<PartialViewTasks>(string.Format("{0} Deleted by user {1}", _alias, UmbracoEnsuredPage.CurrentUser.Id));

			return true;
		}

		#region ITaskReturnUrl Members
		private string _returnUrl = "";
		public string ReturnUrl
		{
			get { return _returnUrl; }
		}

		#endregion
	}
}