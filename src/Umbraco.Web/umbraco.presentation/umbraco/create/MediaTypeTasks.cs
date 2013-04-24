using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class MediaTypeTasks : interfaces.ITaskReturnUrl
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
            var mediaType = cms.businesslogic.media.MediaType.MakeNew(User.GetUser(_userID), Alias.Replace("'", "''"));
            mediaType.IconUrl = UmbracoSettings.IconPickerBehaviour == IconPickerBehaviour.HideFileDuplicates ? ".sprTreeFolder" : "folder.gif";
            
            m_returnUrl = string.Format("settings/editMediaType.aspx?id={0}", mediaType.Id);
            var mediaType = cms.businesslogic.media.MediaType.MakeNew(User.GetUser(_userID), Alias.Replace("'", "''"),
                                                                      ParentID);

            if (ParentID != -1)
            {
                mediaType.MasterContentType = ParentID;
                mediaType.Save();
            }

            m_returnUrl = string.Format("settings/editMediaType.aspx?id={0}", mediaType.Id);
            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.media.MediaType(_parentID).delete();
            return false;
        }

        public MediaTypeTasks()
        {
            //
            // TODO: Add constructor logic here
            //
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
