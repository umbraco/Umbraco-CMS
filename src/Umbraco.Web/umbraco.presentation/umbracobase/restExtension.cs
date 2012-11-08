using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.businesslogic.member;
using umbraco.IO;

namespace umbraco.presentation.umbracobase
{
	[Obsolete]
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

        public bool returnXML
        {
            get { return _returnXml; }
            set { _returnXml = value; }
        }


        public restExtension()
        { }

        public restExtension(string extensionAlias, string methodName)
        {
            bool allowed = false;
            bool fromFile = true;

            XmlDocument baseDoc = new XmlDocument(); //RESTExtension document...
            baseDoc.Load(IOHelper.MapPath(SystemFiles.RestextensionsConfig));

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
                    Member currentMem = Member.GetCurrentMember();

                    //not basic.. and not logged in? - out.. 
                    if (currentMem == null)
                    {
                        allowed = false;
                    }
                    else //do member authentication stuff... 
                        allowed = memberAuthentication(baseExt, currentMem);
                }
            }
            else
            {
                //check for RestExtensionAttribute

            	var restExtensions = PluginManager.Current.ResolveRestExtensions();

				foreach (var t in restExtensions)
                {

                    var temp = t.GetCustomAttributes(typeof(RestExtension), false).OfType<RestExtension>();

                    if (temp.Any(x => x.GetAlias() == extensionAlias))
                    {

                        MethodInfo mi = t.GetMethod(methodName);

                        if (mi != null)
                        {
                            //check allowed
                        	var attributes = mi.GetCustomAttributes(typeof (RestExtensionMethod), false)
                        		.OfType<RestExtensionMethod>()
                        		.ToArray();

                            //check to make sure the method was decorated properly
                            if (attributes.Any())
                            {
                                fromFile = false;

                                var attribute = attributes.First();
                                allowed = attribute.allowAll;

                                if (!allowed)
                                {
                                    //Member Based permissions.. check for group, type and ID... 
                                    Member currentMem = Member.GetCurrentMember();

                                    //not basic.. and not logged in? - out.. 
                                    if (currentMem == null)
                                    {
                                        allowed = false;
                                    }
                                    else
                                    {
                                        //do member authentication stuff... 
                                        allowed = memberAuthentication(attribute, currentMem);
                                    }
                                }

                                if (allowed)
                                {
									this.method = t.GetMethod(methodName);
									this.isAllowed = this.method != null;
                                    this.alias = extensionAlias;
                                    this.assembly = t.Assembly;
                                    this.type = t;
                                    this.returnXML = attribute.returnXml;
                                } 
                            }
                        }
                    }
                }
            }

            if (allowed)
            {
                if (fromFile)
                {
                    XmlNode extNode = baseDoc.SelectSingleNode("/RestExtensions/ext [@alias='" + extensionAlias + "']");
                    string asml = extNode.Attributes["assembly"].Value;
                    string assemblyPath = IOHelper.MapPath(string.Format("{0}/{1}.dll", SystemDirectories.Bin, asml.TrimStart('/')));
                    Assembly returnAssembly = System.Reflection.Assembly.LoadFrom(assemblyPath);

                    string returnTypeName = extNode.Attributes["type"].Value;
                    Type returnType = returnAssembly.GetType(returnTypeName);


                    if (baseExt.Attributes["returnXml"] != null && baseExt.Attributes["returnXml"].Value.ToLower() == "false")
                        this.returnXML = false;

					this.method = returnType.GetMethod(methodName);
					this.isAllowed = this.method != null;
                    this.alias = extensionAlias;
                    this.assembly = returnAssembly;
                    this.type = returnType;
                }
            }
            else
            {
                this.isAllowed = false;
            }
        }

        private static bool memberAuthentication(RestExtensionMethod baseExt, Member currentMem)
        {
            //Check group, type and ID
            bool memberAccess = false;

            if (!string.IsNullOrEmpty(baseExt.GetAllowGroup()))
            {

                //Groups array
                string[] groupArray = baseExt.GetAllowGroup().Split(',');

                foreach (MemberGroup mg in currentMem.Groups.Values)
                {
                    foreach (string group in groupArray)
                    {
                        if (group == mg.Text)
                            memberAccess = true;
                    }
                }

            }

            //Membertype allowed?
            if (!string.IsNullOrEmpty(baseExt.GetAllowType()) && !memberAccess)
            {

                //Types array
                string[] typeArray = baseExt.GetAllowType().Split(',');

                foreach (string type in typeArray)
                {
                    if (type == currentMem.ContentType.Alias)
                        memberAccess = true;
                }

            }


            //Member ID allowed? should this work with loginName instead? 
            if (!string.IsNullOrEmpty(baseExt.GetAllowMember()) && !memberAccess)
            {

                if (int.Parse((string)baseExt.GetAllowMember().Trim()) == currentMem.Id)
                    memberAccess = true;

            }
            return memberAccess;
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
