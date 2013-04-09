using System;
using System.Configuration;
using System.Data;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class nodetypeTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;

        public int UserId
        {
            set { _userID = value; }
        }
        public int TypeID
        {
            set { _typeID = value; }
            get { return _typeID; }
        }


        public string Alias
        {
            set { _alias = value; }
            get { return _alias; }
        }

        public int ParentID
        {
            set { _parentID = value; }
            get { return _parentID; }
        }

        public bool Save()
        {
            //NOTE: TypeID is the parent id!
            //NOTE: ParentID is aparently a flag to determine if we are to create a template! Hack much ?! :P
            var contentType = new ContentType(TypeID != 0 ? TypeID : -1)
                {
                    CreatorId = _userID,
                    Alias = Alias.Replace("'", "''"),
                    Icon = UmbracoSettings.IconPickerBehaviour == IconPickerBehaviour.HideFileDuplicates ? ".sprTreeFolder" : "folder.gif",
                    Name = Alias.Replace("'", "''")
                };

            // Create template?
            if (ParentID == 1)
            {
                var template = new Template(string.Empty, _alias, _alias);
                ApplicationContext.Current.Services.FileService.SaveTemplate(template, _userID);

                contentType.AllowedTemplates = new[] {template};
                contentType.DefaultTemplateId = template.Id;
            }
            ApplicationContext.Current.Services.ContentTypeService.Save(contentType);

            m_returnUrl = "settings/editNodeTypeNew.aspx?id=" + contentType.Id.ToString();

            return true;
        }

        public bool Delete()
        {
            var docType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(ParentID);
            if (docType != null)
            {
                ApplicationContext.Current.Services.ContentTypeService.Delete(docType);
            }
            return false;
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }
}
