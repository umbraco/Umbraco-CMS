using System;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;

namespace presentation.umbracoBase.library
{
    //This should be replaced with umbracos own methods...
    public class library
    {
        public static Member GetCurrentMember()
        {
            try
            {
                if (CurrentMemberId() != 0)
                {
                    // return member from cache
                    Member m = Member.GetMemberFromCache(CurrentMemberId());
                    if (m == null)
                        m = new Member(CurrentMemberId());

                    if (m != null)
                        if (m.UniqueId == new Guid(getCookieValue("umbracoMemberGuid")) && m.LoginName == getCookieValue("umbracoMemberLogin"))
                            return m;

                    return null;
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }

        }

        public static int CurrentMemberId()
        {
            int _currentMemberId = 0;

            if (getCookieValue("umbracoMemberId") != "" && getCookieValue("umbracoMemberGuid") != "" && getCookieValue("umbracoMemberLogin") != "")
                _currentMemberId = int.Parse(getCookieValue("umbracoMemberId"));

            return _currentMemberId;
        }

        private static string getCookieValue(string Name)
        {
            string tempValue = "";
            if (System.Web.HttpContext.Current.Request.Cookies[Name] != null)
                if (System.Web.HttpContext.Current.Request.Cookies[Name].Value != "")
                {
                    tempValue = System.Web.HttpContext.Current.Request.Cookies[Name].Value;
                }

            return tempValue;
        }
    }

    public class member
    {
        public static int login(string loginname, string password)
        {
            Member m = Member.GetMemberFromLoginNameAndPassword(loginname, password);

            if (library.CurrentMemberId() == 0)
            {
                // If null, login not correct... 
                if (m != null)
                {
                    Member.AddMemberToCache(m);
                    return m.Id;
                }
                else
                    return 0;
            }
            else
                return library.CurrentMemberId();
        }

        public static int logout()
        {
            Member m = library.GetCurrentMember();
            if (m != null)
            {
                Member.RemoveMemberFromCache(m);
                Member.ClearMemberFromClient(m);
                return m.Id;
            }
            else
                return 0;
        }

        public static XPathNodeIterator data()
        {
            if (library.GetCurrentMember() != null)
            {
                XmlDocument mXml = new XmlDocument();
                mXml.LoadXml(library.GetCurrentMember().ToXml(mXml, false).OuterXml);
                XPathNavigator xp = mXml.CreateNavigator();
                return xp.Select("/node");
            }
            else
                return null;
        }

        public static int id()
        {
            return library.CurrentMemberId();
        }

        public static string setProperty(string alias, object value)
        {
            string retVal = "False";
            try
            {
                Member myMember = library.GetCurrentMember();

                if (myMember != null)
                {
                    Property myProperty = myMember.getProperty(alias);

                    if (MemberType.GetByAlias(myMember.ContentType.Alias).MemberCanEdit(myProperty.PropertyType))
                    {

                        if (myProperty.PropertyType.ValidationRegExp.Trim() != "")
                        {
                            Regex regexPattern = new Regex(myMember.getProperty(alias).PropertyType.ValidationRegExp);

                            if (regexPattern.IsMatch(value.ToString()))
                            {
                                myProperty.Value = value;
                                retVal = "True";
                                myMember.XmlGenerate(new XmlDocument());
                            }
                        }
                        else
                        {
                            myProperty.Value = value;
                            retVal = "True";
                            myMember.XmlGenerate(new XmlDocument());
                        }

                    }
                }
            }
            catch (Exception x)
            { retVal = x.Message; }

            return retVal;
        }
    }

}
