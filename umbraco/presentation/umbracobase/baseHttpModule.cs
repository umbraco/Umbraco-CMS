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

namespace umbraco.presentation.umbracobase
{
    public class requestModule : IHttpModule
    {
        #region IHttpModule Members
        public void Dispose()
        {
        }

        public void Init(HttpApplication httpApp)
        {
            httpApp.BeginRequest += new EventHandler(OnBeginRequest);
        }


        public void OnBeginRequest(Object sender, EventArgs e)
        {
            HttpApplication httpApp = (HttpApplication)sender;

            //remove extension and split the url
            string url = httpApp.Context.Request.RawUrl;
            

            if (url.ToLower().StartsWith("/base/"))
            {
                if(url.ToLower().Contains(".aspx"))
                    url = url.Substring(0, url.IndexOf(".aspx"));

                object[] urlArray = url.Split('/');

                //There has to be minimum 4 parts in the url for this to work... /base/library/method/[parameter].aspx
                if (urlArray.Length >= 4)
                {
                    string extensionAlias = urlArray[2].ToString();
                    string methodName = urlArray[3].ToString();

                    httpApp.Response.ContentType = "text/xml";
                    restExtension myExtension = new restExtension(extensionAlias, methodName);

                    if (myExtension.isAllowed)
                    {
                        httpApp.Response.Output.Write(invokeMethod(myExtension, urlArray));
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




        private string invokeMethod(restExtension myExtension, object[] paras)
        {
            try
            {
                //So method is either not found or not valid... this should probably be moved... 
                if (!myExtension.method.IsPublic || !myExtension.method.IsStatic)
                    return "<error>Method has to be public and static</error>";
                else //And if it is lets continue trying to invoke it... 
                {
                    //lets check if we have parameters enough in the url..
                    if (myExtension.method.GetParameters().Length > (paras.Length - 4)) //Too few
                        return "<error>Not Enough parameters in url</error>";
                    else
                    {

                        //We have enough parameters... lets invoke..
                        //Create an instance of the type we need to invoke the method from.
                        Object obj = Activator.CreateInstance(myExtension.type);
                        Object response;

                        //umbracoBase.baseBinder bBinder = new baseBinder();

                        if (myExtension.method.GetParameters().Length == 0)
                        {
                            //response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, null, System.Globalization.CultureInfo.CurrentCulture);
                            response = myExtension.method.Invoke(obj, null); //Invoke with null as parameters as there are none
                        }

                        else
                        {
                            //We only need the parts of the url above the number 4 so we'll 
                            //recast those to objects and add them to the object[]

                            //Getting the right lenght.. 4 is the magic number dum di dum.. 
                            object[] methodParams = new object[(paras.Length - 4)];

                            int i = 0;

                            foreach (ParameterInfo pInfo in myExtension.method.GetParameters())
                            {
                                Type myType = Type.GetType(pInfo.ParameterType.ToString());
                                methodParams[(i)] = Convert.ChangeType(paras[i + 4], myType);
                                i++;
                            }

                            //Invoke with methodParams
                            //response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, methodParams, System.Globalization.CultureInfo.CurrentCulture);
                            response = myExtension.method.Invoke(obj, methodParams);
                        }

                        /*TODO - SOMETHING ALITTLE BETTER THEN ONLY CHECK FOR XPATHNODEITERATOR OR ELSE do ToString() */
                        if (response != null)
                        {
                            if (myExtension.method.ReturnType.ToString() == "System.Xml.XPath.XPathNodeIterator")
                                return ((System.Xml.XPath.XPathNodeIterator)response).Current.OuterXml;
                            else
                            {
                                string strResponse = ((string)response.ToString());

                                if (myExtension.returnXML) {
                                    //do a quick "is this html?" check... if it is add CDATA... 
                                    if (strResponse.Contains("<") || strResponse.Contains(">"))
                                        strResponse = "<![CDATA[" + strResponse + "]]>";
                                    return "<value>" + strResponse + "</value>";
                                } else {
                                    HttpContext.Current.Response.ContentType = "text/html";
                                    return strResponse;
                                }
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
                return "<error><![CDATA[MESSAGE:\n" + ex.Message + "\n\nSTACKTRACE:\n" + ex.StackTrace + "\n\nINNEREXCEPTION:\n" + ex.InnerException + "]]></error>";
            }

        }
    }
}
        #endregion