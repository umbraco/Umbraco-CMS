using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Reflection;
using System.Collections.Specialized;
using System.Xml;
using umbraco;
using umbraco.cms.businesslogic.member;

namespace presentation.umbracoBase
{
    public class requestModule : IHttpModule
    {
        #region IHttpModule Members
        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Init(HttpApplication httpApp)
        {
            httpApp.BeginRequest += new EventHandler(OnBeginRequest);
        }


        public void OnBeginRequest(Object sender, EventArgs e)
        {
            HttpApplication httpApp = (HttpApplication)sender;

            //remove extension and split the url
            object[] urlArray = httpApp.Context.Request.RawUrl.Replace(".aspx", "").Split('/');

            if (urlArray[1].ToString() == "base")
            {
                //There has to be minimum 4 parts in the url for this to work... /base/library/method/[parameter].aspx
                if (urlArray.Length >= 4) 
                {
                    string extensionAlias = urlArray[2].ToString();
                    string methodName = urlArray[3].ToString();

                    httpApp.Response.ContentType = "text/xml";
                    MyMethodStruct myMMS = getMethod(extensionAlias, methodName);

                    if (myMMS.isAllowed)
                    {
                        httpApp.Response.Output.Write(invokeMethod(myMMS, urlArray));
                    }
                    else
                    {
                        //Very static error msg...
                        httpApp.Response.Output.Write("<error>Extension not found or permission denied</error>"); 
                    }
                    //end the resposne
                    httpApp.Response.End();
                }
            }
        }




        private string invokeMethod(MyMethodStruct myMethod, object[] paras)
        {
            try
            {
                //So method is either not found or not valid... this should probably be moved... 
                if (!myMethod.method.IsPublic || !myMethod.method.IsStatic)
                    return "<error>Method has to be public and static</error>";
                else //And if it is lets continue trying to invoke it... 
                {
                    //lets check if we have parameters enough in the url..
                    if (myMethod.method.GetParameters().Length > (paras.Length - 4)) //Too few
                        return "<error>Not Enough parameters in url</error>";
                    else
                    {

                        //We have enough parameters... lets invoke..
                        //Create an instance of the type we need to invoke the method from.
                        Object obj = Activator.CreateInstance(myMethod.type);
                        Object response;

                        //umbracoBase.baseBinder bBinder = new baseBinder();

                        if (myMethod.method.GetParameters().Length == 0)
                        {
                            //response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, null, System.Globalization.CultureInfo.CurrentCulture);
                            response = myMethod.method.Invoke(obj, null); //Invoke with null as parameters as there are none
                        }

                        else
                        {
                            //We only need the parts of the url above the number 4 so we'll 
                            //recast those to objects and add them to the object[]

                            //Getting the right lenght.. 4 is the magic number dum di dum.. 
                            object[] methodParams = new object[(paras.Length - 4)];

                            int i = 0;

                            foreach (ParameterInfo pInfo in myMethod.method.GetParameters())
                            {
                                Type myType = Type.GetType(pInfo.ParameterType.ToString());
                                methodParams[(i)] = Convert.ChangeType(paras[i + 4], myType);
                                i++;
                            }

                            //Invoke with methodParams
                            //response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, methodParams, System.Globalization.CultureInfo.CurrentCulture);
                            response = myMethod.method.Invoke(obj, methodParams);
                        }

                        /*TODO - SOMETHING ALITTLE BETTER THEN ONLY CHECK FOR XPATHNODEITERATOR OR ELSE do ToString() */
                        if (response != null)
                        {
                            if (myMethod.method.ReturnType.ToString() == "System.Xml.XPath.XPathNodeIterator")
                                return ((System.Xml.XPath.XPathNodeIterator)response).Current.OuterXml;
                            else
                            {
                                string strResponse = ((string)response.ToString());

                                //do a quick "is this html?" check... if it is add CDATA... 
                                if (strResponse.Contains("<") || strResponse.Contains(">"))
                                    strResponse = "<![CDATA[" + strResponse + "]]>";
                                return "<value>" + strResponse + "</value>";
                            }
                        }
                        else
                        {
                            //Error msg... 
                            return "<error>Null value returned</error>";
                        }



                    }
                }
            }

            catch (Exception ex)
            {
                //Overall exception handling... 
                return "<error><![CDATA[MESSAGE:" + ex.Message + " STACKTRACE: " + ex.StackTrace + " ]]></error>";
            }

        }


        private struct MyMethodStruct
        {
            public Type type;
            public MethodInfo method;
            public Assembly assembly;
            public string alias;
            public bool isAllowed;
        }


        private static MyMethodStruct getMethod(string extensionAlias, string methodName)
        {

            bool allowed = false;

            XmlDocument baseDoc = new XmlDocument(); //RESTExtension document...
            baseDoc.Load(System.Web.HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../config/restExtensions.xml"));

            XmlNode baseExt = baseDoc.SelectSingleNode("/RestExtensions/ext [@alias='" + extensionAlias + "']/permission [@method='" + methodName + "']");

            //basicly - if not there.. it's not allowed... 
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


                MyMethodStruct mms = new MyMethodStruct();
                mms.alias = extensionAlias;
                mms.assembly = returnAssembly;
                mms.method = returnType.GetMethod(methodName);
                mms.type = returnType;
                mms.isAllowed = true;
                return mms;
            }
            else
            {
                MyMethodStruct noMms = new MyMethodStruct();
                noMms.isAllowed = false;
                return noMms;
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

        #endregion
    }





}
