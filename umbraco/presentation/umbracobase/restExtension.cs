using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Reflection;
using System.Collections.Specialized;
using System.Xml;

using umbraco;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;

namespace umbraco.presentation.umbracobase
{
    public class restExtension
    {
        private Type _type;
        private MethodInfo _method;
        private Assembly _assembly;
        private string _alias;
        private bool _isAllowed;
        private bool _returnXml = true;

        public Type type
        {
            get { return _type; }
            set { _type = value; }
        }

        public MethodInfo method
        {
            get { return _method; }
            set { _method = value; }
        }

        public Assembly assembly
        {
            get { return _assembly; }
            set { _assembly = value; }
        }

        public string alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        public bool isAllowed
        {
            get { return _isAllowed; }
            set { _isAllowed = value; }
        }

        public bool returnXML {
            get { return _returnXml; }
            set { _returnXml = value; }
        }


        public restExtension()
        { }

        public restExtension(string extensionAlias, string methodName)
        {
            bool allowed = false;

            XmlDocument baseDoc = new XmlDocument(); //RESTExtension document...
            baseDoc.Load(System.Web.HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../config/restExtensions.config"));

            XmlNode baseExt = baseDoc.SelectSingleNode("/RestExtensions/ext [@alias='" + extensionAlias + "']/permission [@method='" + methodName + "']");

            //if not there.. it's not allowed... 
            if (baseExt != null)
            {
                //Access for all ?
                if (baseExt.Attributes["allowAll"] != null)
                {
                    if (baseExt.Attributes["allowAll"].Value.ToString().ToLower() == "true")
                        allowed = true;
                }

                if (!allowed)
                {
                    //Member Based permissions.. check for group, type and ID... 
                    Member currentMem = library.library.GetCurrentMember(); // <-- /TODO/ uses bases own member class as umbracos doesn't work yet... 

                    //not basic.. and not logged in? - out.. 
                    if (currentMem == null)
                    {
                        allowed = false;
                    }
                    else //do member authentication stuff... 
                        allowed = memberAuthentication(baseExt, currentMem);
                }
            }

            if (allowed)
            {
                XmlNode extNode = baseDoc.SelectSingleNode("/RestExtensions/ext [@alias='" + extensionAlias + "']");
                string asml = extNode.Attributes["assembly"].Value;
                string assemblyPath = System.Web.HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/.." + asml + ".dll");
                Assembly returnAssembly = System.Reflection.Assembly.LoadFrom(assemblyPath);

                string returnTypeName = extNode.Attributes["type"].Value;
                Type returnType = returnAssembly.GetType(returnTypeName);


                if (baseExt.Attributes["returnXml"] != null && baseExt.Attributes["returnXml"].Value.ToLower() == "false")
                    this.returnXML = false;
                
                this.isAllowed = true;
                this.alias = extensionAlias;
                this.assembly = returnAssembly;
                this.method = returnType.GetMethod(methodName);
                this.type = returnType;
            }
            else
            {
                this.isAllowed = false;
            }
        }


        private static bool memberAuthentication(XmlNode baseExt, Member currentMem)
        {
            //Check group, type and ID
            bool memberAccess = false;

            if (baseExt.Attributes["allowGroup"] != null)
            {
                if (baseExt.Attributes["allowGroup"].Value != "")
                {
                    //Groups array
                    string[] groupArray = baseExt.Attributes["allowGroup"].Value.Split(',');

                    foreach (MemberGroup mg in currentMem.Groups.Values)
                    {
                        foreach (string group in groupArray)
                        {
                            if (group == mg.Text)
                                memberAccess = true;
                        }
                    }
                }
            }

            //Membertype allowed?
            if (baseExt.Attributes["allowType"] != null && !memberAccess)
            {
                if (baseExt.Attributes["allowType"].Value != "")
                {
                    //Types array
                    string[] typeArray = baseExt.Attributes["allowType"].Value.Split(',');

                    foreach (string type in typeArray)
                    {
                        if (type == currentMem.ContentType.Alias)
                            memberAccess = true;
                    }
                }
            }


            //Member ID allowed? should this work with loginName instead? 
            if (baseExt.Attributes["allowMember"] != null && !memberAccess)
            {
                if (baseExt.Attributes["allowMember"].Value != "")
                {
                    if (int.Parse((string)baseExt.Attributes["allowMember"].Value.Trim()) == currentMem.Id)
                        memberAccess = true;
                }
            }
            return memberAccess;
        }
        
    }
}
