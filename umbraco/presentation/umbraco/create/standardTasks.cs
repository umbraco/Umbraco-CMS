using System;
using System.Data;

using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    /// <summary>
    /// Summary description for standardTasks.
    /// </summary>
    ///

    public class XsltTasks : interfaces.ITaskReturnUrl
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
            string template = _alias.Substring(0, _alias.IndexOf("|||"));
            string fileName = _alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3).Replace(" ", "");
            string xsltTemplateSource = IOHelper.MapPath( SystemDirectories.Umbraco + "/xslt/templates/" + template);
            string xsltNewFilename = IOHelper.MapPath( SystemDirectories.Xslt + "/" + fileName + ".xslt");

            if (fileName.Contains("/")) //if there's a / create the folder structure for it
            {
                string[] folders = fileName.Split("/".ToCharArray());
                string xsltBasePath = IOHelper.MapPath(SystemDirectories.Xslt);
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    xsltBasePath = System.IO.Path.Combine(xsltBasePath, folders[i]);
                    System.IO.Directory.CreateDirectory(xsltBasePath);
                }
            }

//            System.IO.File.Copy(xsltTemplateSource, xsltNewFilename, false);

            // update with xslt references
            string xslt = "";
            System.IO.StreamReader xsltFile = System.IO.File.OpenText(xsltTemplateSource);
            xslt = xsltFile.ReadToEnd();
            xsltFile.Close();

            // prepare support for XSLT extensions
            xslt = macro.AddXsltExtensionsToHeader(xslt);
            System.IO.StreamWriter xsltWriter = System.IO.File.CreateText(xsltNewFilename);
            xsltWriter.Write(xslt);
            xsltWriter.Flush();
            xsltWriter.Close();

            // Create macro?
            if (ParentID == 1)
            {
                cms.businesslogic.macro.Macro m =
                    cms.businesslogic.macro.Macro.MakeNew(
                    helper.SpaceCamelCasing(_alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3)));
                m.Xslt = fileName + ".xslt";
            }

            m_returnUrl = string.Format( SystemDirectories.Umbraco + "/developer/xslt/editXslt.aspx?file={0}.xslt", fileName);

            return true;
        }

        public bool Delete()
        {
            string path = IOHelper.MapPath(SystemDirectories.Xslt + "/" + Alias.TrimStart('/'));

            System.Web.HttpContext.Current.Trace.Warn("", "*" + path + "*");
            
            try
            {
                System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, UmbracoEnsuredPage.CurrentUser, -1, "Could not remove XSLT file " + Alias + ". ERROR: " + ex.Message);
            }
            return true;
        }

        public XsltTasks()
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

    public class DLRScriptingTasks : interfaces.ITaskReturnUrl
    {
        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;

        public int UserId
        {
            set
            {
                _userID = value;
            }
        }

        public int TypeID
        {
            get
            {
                return _typeID;
            }
            set
            {
                _typeID = value;
            }
        }

        public string Alias
        {
            get
            {
                return _alias;
            }
            set
            {
                _alias = value;
            }
        }

        public int ParentID
        {
            get
            {
                return _parentID;
            }
            set
            {
                _parentID = value;
            }
        }

        public bool Save()
        {

            string template = _alias.Substring(0, _alias.IndexOf("|||")).Trim();
            string fileName = _alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3).Replace(" ", "");

            if (!fileName.Contains("."))
                fileName = _alias + ".py";

            string scriptContent = "";
            if (!string.IsNullOrEmpty(template))
            {
                System.IO.StreamReader templateFile = System.IO.File.OpenText( IOHelper.MapPath( IO.SystemDirectories.Umbraco + "/scripting/templates/" + template));
                scriptContent = templateFile.ReadToEnd();
                templateFile.Close();
            }
            
            
            if (fileName.Contains("/")) //if there's a / create the folder structure for it
            {
                string[] folders = fileName.Split("/".ToCharArray());
                string basePath = IOHelper.MapPath(SystemDirectories.Python);
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    basePath = System.IO.Path.Combine(basePath, folders[i]);
                    System.IO.Directory.CreateDirectory(basePath);
                }
            }

            string abFileName = IOHelper.MapPath(SystemDirectories.Python + "/" + fileName);

            System.IO.StreamWriter scriptWriter = System.IO.File.CreateText(abFileName);
            scriptWriter.Write(scriptContent);
            scriptWriter.Flush();
            scriptWriter.Close();


            if (ParentID == 1)
            {
                cms.businesslogic.macro.Macro m = cms.businesslogic.macro.Macro.MakeNew(
                    helper.SpaceCamelCasing(fileName.Substring(0, (fileName.LastIndexOf('.') + 1)).Trim('.')));
                m.ScriptingFile = fileName;
            }

            m_returnUrl = string.Format(SystemDirectories.Umbraco + "/developer/python/editPython.aspx?file={0}", fileName);
            return true;
        }

        public bool Delete()
        {

            string path = IOHelper.MapPath(SystemDirectories.Python + "/" + Alias.TrimStart('/'));

            System.Web.HttpContext.Current.Trace.Warn("", "*" + path + "*");
            try
            {
                System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, UmbracoEnsuredPage.CurrentUser, -1, "Could not remove XSLT file " + Alias + ". ERROR: " + ex.Message);
            }
            return true;
        }

        public DLRScriptingTasks()
        {
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }
    public class PythonTasks : DLRScriptingTasks { }
    
    public class ScriptTasks : interfaces.ITaskReturnUrl
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
            string[] scriptFileAr = _alias.Split('¤');
            
            

            string relPath = scriptFileAr[0];
            string fileName = scriptFileAr[1];
            string fileType = scriptFileAr[2];
            
            int createFolder = ParentID;

            string basePath = IOHelper.MapPath(SystemDirectories.Scripts + "/" + relPath + fileName);
            
            if (createFolder == 1)
            {
                System.IO.Directory.CreateDirectory(basePath);
            }
            else
            {
                System.IO.File.Create(basePath + "." + fileType).Close();
                m_returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}{1}.{2}", relPath, fileName, fileType);
            }
            return true;
        }

        public bool Delete()
        {
            string path = IOHelper.MapPath( SystemDirectories.Scripts + "/" + _alias.TrimStart('/'));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            else if(System.IO.Directory.Exists(path))
               System.IO.Directory.Delete(path, true);
            
            BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Delete, umbraco.BasePages.UmbracoEnsuredPage.CurrentUser, -1, _alias + " Deleted");
            return true;
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }

    public class CreatedPackageTasks : interfaces.ITaskReturnUrl
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

            umbraco.BusinessLogic.User myUser = new umbraco.BusinessLogic.User(0);
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Delete, myUser, 0, "Xml save started");
            int id = cms.businesslogic.packager.CreatedPackage.MakeNew(Alias).Data.Id;
            m_returnUrl = string.Format("developer/packages/editPackage.aspx?id={0}", id);
            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.packager.CreatedPackage.GetById(ParentID).Delete();
            return true;
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }

    public class macroTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

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
            int id = umbraco.cms.businesslogic.macro.Macro.MakeNew(_alias).Id;
            m_returnUrl = string.Format("developer/Macros/editMacro.aspx?macroID={0}", id);
            return true;
        }

        public bool Delete()
        {
            // Release cache
            System.Web.Caching.Cache macroCache = System.Web.HttpRuntime.Cache;
            if (macroCache["umbMacro" + ParentID.ToString()] != null)
            {
                macroCache.Remove("umbMacro" + ParentID.ToString());
            }

            // Clear cache!
            macro.ClearAliasCache();
            new cms.businesslogic.macro.Macro(ParentID).Delete();
            return true;
        }

        public macroTasks()
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
            int id = cms.businesslogic.media.MediaType.MakeNew(BusinessLogic.User.GetUser(_userID), Alias.Replace("'", "''")).Id;
            m_returnUrl = string.Format("settings/editMediaType.aspx?id={0}", id);
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
            cms.businesslogic.web.DocumentType dt = cms.businesslogic.web.DocumentType.MakeNew(BusinessLogic.User.GetUser(_userID), Alias.Replace("'", "''"));
            dt.IconUrl = "folder.gif";

            // Create template?
            if (ParentID == 1)
            {
                cms.businesslogic.template.Template[] t = { cms.businesslogic.template.Template.MakeNew(_alias, BusinessLogic.User.GetUser(_userID)) };
                dt.allowedTemplates = t;
                dt.DefaultTemplate = t[0].Id;
            }

            // Master Content Type?
            if (TypeID != 0)
            {
                dt.MasterContentType = TypeID;
            }

            m_returnUrl = "settings/editNodeTypeNew.aspx?id=" + dt.Id.ToString();

            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.web.DocumentType(ParentID).delete();

            //after a document type is deleted, we clear the cache, as some content will now have disappeared.
            library.RefreshContent();

            return false;
        }

        public nodetypeTasks()
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

    public class templateTasks : interfaces.ITaskReturnUrl
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
            int masterId = ParentID;
            BusinessLogic.Log.Add(LogTypes.Debug, -1, "tp id:" + masterId.ToString());
            if (masterId > 0)
            {
                int id = cms.businesslogic.template.Template.MakeNew(Alias, BusinessLogic.User.GetUser(_userID), new cms.businesslogic.template.Template(masterId)).Id;
                m_returnUrl = string.Format("settings/editTemplate.aspx?templateID={0}", id);
            }
            else
            {
                int id = cms.businesslogic.template.Template.MakeNew(Alias, BusinessLogic.User.GetUser(_userID)).Id;
                m_returnUrl = string.Format("settings/editTemplate.aspx?templateID={0}", id);

            }
            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.template.Template(_parentID).delete();
            return false;
        }

        public templateTasks()
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

    public class mediaTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

        public int UserId
        {
            set { _userID = value; }
        }

        public string ReturnUrl
        {
            get { return _returnUrl; }
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
            set
            {
                _parentID = value;
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            cms.businesslogic.media.MediaType dt = new cms.businesslogic.media.MediaType(TypeID);
            cms.businesslogic.media.Media m = cms.businesslogic.media.Media.MakeNew(Alias, dt, BusinessLogic.User.GetUser(_userID), ParentID);
            _returnUrl = "editMedia.aspx?id=" + m.Id.ToString() + "&isNew=true";

            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.media.Media d = new cms.businesslogic.media.Media(ParentID);

            // Log
            BasePages.UmbracoEnsuredPage bp = new BasePages.UmbracoEnsuredPage();
            BusinessLogic.Log.Add(BusinessLogic.LogTypes.Delete, bp.getUser(), d.Id, "");

            d.delete();
            return true;

        }

        public bool Sort()
        {
            return false;
        }

        public mediaTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

    public class contentTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

        public int UserId
        {
            set { _userID = value; }
        }

        public string ReturnUrl
        {
            get { return _returnUrl; }
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
            set
            {
                _parentID = value;
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            cms.businesslogic.web.DocumentType dt = new cms.businesslogic.web.DocumentType(TypeID);
            cms.businesslogic.web.Document d = cms.businesslogic.web.Document.MakeNew(Alias, dt, BusinessLogic.User.GetUser(_userID), ParentID);
            if (d == null)
            {
                //TODO: Slace - Fix this to use the language files
                BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Document Creation", "Document creation was canceled");
                return false;
            }
            else
            {
                _returnUrl = "editContent.aspx?id=" + d.Id.ToString() + "&isNew=true";
                return true; 
            }
        }

        public bool Delete()
        {
            cms.businesslogic.web.Document d = new cms.businesslogic.web.Document(ParentID);

            // Log
            BasePages.UmbracoEnsuredPage bp = new BasePages.UmbracoEnsuredPage();
            BusinessLogic.Log.Add(BusinessLogic.LogTypes.Delete, bp.getUser(), d.Id, "");

            library.UnPublishSingleNode(d.Id);

            d.delete();

            return true;

        }

        public bool Sort()
        {
            return false;
        }

        public contentTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

    public class userTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

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

        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public bool Save()
        {
            // Hot damn HACK > user is allways UserType with id  = 1  = administrator ???
            // temp password deleted by NH
            //BusinessLogic.User.MakeNew(Alias, Alias, "", BusinessLogic.UserType.GetUserType(1));
            //return true;

            MembershipCreateStatus status = MembershipCreateStatus.ProviderError;
            try
            {
                // Password is auto-generated. They are they required to change the password by editing the user information.
                MembershipUser u = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].CreateUser(Alias,
                    Membership.GeneratePassword(
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].MinRequiredPasswordLength,
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].MinRequiredNonAlphanumericCharacters),
                    "", "", "", true, null, out status);

                _returnUrl = string.Format("users/EditUser.aspx?id={0}", u.ProviderUserKey.ToString());

                return status == MembershipCreateStatus.Success;
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, ParentID, String.Format("Failed to create the user. Error from provider: {0}", status.ToString()));
                Log.Add(LogTypes.Debug, ParentID, ex.Message);
                return false;
            }
        }

        public bool Delete()
        {
            BusinessLogic.User u = BusinessLogic.User.GetUser(ParentID);
            u.disable();
            return true;
        }

        public userTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

    public class DataTypeTasks : interfaces.ITaskReturnUrl
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

            int id = cms.businesslogic.datatype.DataTypeDefinition.MakeNew(BusinessLogic.User.GetUser(_userID), Alias).Id;
            m_returnUrl = string.Format("developer/datatypes/editDataType.aspx?id={0}", id);
            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(ParentID).delete();
            return true;
        }

        public DataTypeTasks()
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

    public class contentItemTypeTasks : interfaces.ITask
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

            cms.businesslogic.contentitem.ContentItemType.MakeNew(BusinessLogic.User.GetUser(_userID), Alias);
            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.contentitem.ContentItemType(_parentID).delete();
            return true;
        }

        public contentItemTypeTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

    public class MemberGroupTasks : interfaces.ITaskReturnUrl
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
            System.Web.Security.Roles.CreateRole(Alias);
            //			int id = cms.businesslogic.member.MemberGroup.MakeNew(Alias, BusinessLogic.User.GetUser(_userID)).Id;
            m_returnUrl = string.Format("members/EditMemberGroup.aspx?id={0}", System.Web.HttpContext.Current.Server.UrlEncode(Alias));
            return true;
        }

        public bool Delete()
        {
            // only build-in roles can be deleted
            if (cms.businesslogic.member.Member.IsUsingUmbracoRoles())
            {
                cms.businesslogic.member.MemberGroup.GetByName(Alias).delete();
                return true;
            }
            else
            {
                return false;
            }
            //try
            //{

            //    MembershipUser u = Membership.GetUser(_parentID);
            //    Membership.DeleteUser(u.UserName);
            //    return true;

            //}catch
            //{
            //    Log.Add(LogTypes.Error, _parentID, "Member cannot be deleted.");
            //    return false;
            //}
        }

        public MemberGroupTasks()
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

    public class MemberTypeTasks : interfaces.ITaskReturnUrl
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

            int id = cms.businesslogic.member.MemberType.MakeNew(BusinessLogic.User.GetUser(_userID), Alias).Id;
            m_returnUrl = string.Format("members/EditMemberType.aspx?id={0}", id);
            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.member.MemberType(_parentID).delete();
            return true;
        }

        public MemberTypeTasks()
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

    public class StylesheetTasks : interfaces.ITaskReturnUrl
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

            int id = cms.businesslogic.web.StyleSheet.MakeNew(BusinessLogic.User.GetUser(_userID), Alias, "", "").Id;
            m_returnUrl = string.Format("settings/stylesheet/editStylesheet.aspx?id={0}", id);
            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.web.StyleSheet s = new cms.businesslogic.web.StyleSheet(ParentID);
            s.delete();
            return true;
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }

    public class stylesheetPropertyTasks : interfaces.ITaskReturnUrl
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
            try
            {
                cms.businesslogic.web.StyleSheet s = new cms.businesslogic.web.StyleSheet(ParentID);
                int id = s.AddProperty(Alias, BusinessLogic.User.GetUser(_userID)).Id;
                m_returnUrl = string.Format("settings/stylesheet/property/EditStyleSheetProperty.aspx?id={0}", id);
            }
            catch
            {
                throw new ArgumentException("DER ER SKET EN FEJL MED AT OPRETTE NOGET MED ET PARENT ID : " + ParentID);
            }
            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.web.StylesheetProperty sp = new cms.businesslogic.web.StylesheetProperty(ParentID);
            cms.businesslogic.web.StyleSheet s = sp.StyleSheet();
            s.saveCssToFile();
            sp.delete();

            return true;
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }
    public class NewMemberUIEventArgs : System.ComponentModel.CancelEventArgs
    {
    }

    public class memberTasks : interfaces.ITaskReturnUrl
    {
        /// <summary>
        /// The new event handler
        /// </summary>
        new public delegate void NewUIMemberEventHandler(Member sender, string unencryptedPassword, NewMemberUIEventArgs e);

        new public static event NewUIMemberEventHandler NewMember;
        new protected virtual void OnNewMember(NewMemberUIEventArgs e, string unencryptedPassword, Member m)
        {
            if (NewMember != null)
            {
                NewMember(m, unencryptedPassword, e);
            }
        }


        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

        public int UserId
        {
            set { _userID = value; }
        }

        public int TypeID
        {
            set { _typeID = value; }
            get { return _typeID; }
        }

        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public string Alias
        {
            set { _alias = value; }
            get { return _alias; }
        }

        public int ParentID
        {
            set
            {
                _parentID = value;
                if (_parentID == 1) _parentID = -1;
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            string[] nameAndMail = Alias.Split("|".ToCharArray());
            string name = nameAndMail[0];
            string email = nameAndMail.Length > 0 ? nameAndMail[1] : "";
            string password = nameAndMail.Length > 1 ? nameAndMail[2] : "";
            if (cms.businesslogic.member.Member.InUmbracoMemberMode() && TypeID != -1)
            {
                cms.businesslogic.member.MemberType dt = new cms.businesslogic.member.MemberType(TypeID);
                cms.businesslogic.member.Member m = cms.businesslogic.member.Member.MakeNew(name, dt, BusinessLogic.User.GetUser(_userID));
                m.Password = password;
                m.Email = email;
                m.LoginName = name.Replace(" ", "").ToLower();

                NewMemberUIEventArgs e = new NewMemberUIEventArgs();
                this.OnNewMember(e, password, m);

                _returnUrl = "members/editMember.aspx?id=" + m.Id.ToString();
            }
            else
            {
                MembershipCreateStatus mc = new MembershipCreateStatus();
                Membership.CreateUser(name, password, email, "empty", "empty", true, out mc);
                if (mc != MembershipCreateStatus.Success)
                {
                    throw new Exception("Error creating Member: " + mc.ToString());
                }
                _returnUrl = "members/editMember.aspx?id=" + System.Web.HttpContext.Current.Server.UrlEncode(name);
            }

            return true;
        }

        public bool Delete()
        {
            //cms.businesslogic.member.Member d = new cms.businesslogic.member.Member(ParentID);
            //d.delete();
            //return true;
            MembershipUser u = Membership.GetUser(Alias);
            Membership.DeleteUser(u.UserName, true);
            return true;


        }

        public bool Sort()
        {
            return false;
        }

        public memberTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

    public class contentItemTasks : interfaces.ITask
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
            set
            {
                _parentID = value;
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            // TODO : fix it!!
            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.contentitem.ContentItem d = new cms.businesslogic.contentitem.ContentItem(ParentID);

            // Version3.0 - moving to recycle bin instead of deletion
            //d.delete();
            d.Move(-20);
            return true;

        }

        public bool Sort()
        {
            return false;
        }

        public contentItemTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

    public class dictionaryTasks : interfaces.ITaskReturnUrl
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
            set
            {
                _parentID = value;
                // NASTY HACK ON NASTY HACK§!!
                // if (_parentID == 1) _parentID = -1;
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            //check to see if key is already there
            if (cms.businesslogic.Dictionary.DictionaryItem.hasKey(Alias))
                return false;

            // Create new dictionary item if name no already exist
            if (ParentID > 0)
            {
                int id = cms.businesslogic.Dictionary.DictionaryItem.addKey(Alias, "", new cms.businesslogic.Dictionary.DictionaryItem(ParentID).key);
                m_returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", id);
            }
            else
            {
                int id = cms.businesslogic.Dictionary.DictionaryItem.addKey(Alias, "");
                m_returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", id);
            }
            return true;
        }

        public bool Delete()
        {
            BusinessLogic.Log.Add(LogTypes.Debug, ParentID, _typeID.ToString() + " " + _parentID.ToString() + " deleting " + Alias);

            new cms.businesslogic.Dictionary.DictionaryItem(ParentID).delete();
            return true;
        }

        public bool Sort()
        {
            return false;
        }

        public dictionaryTasks()
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

    public class languageTasks : interfaces.ITask
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
            cms.businesslogic.language.Language.MakeNew(Alias);
            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.language.Language(ParentID).Delete();
            return false;
        }

        public languageTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

}
