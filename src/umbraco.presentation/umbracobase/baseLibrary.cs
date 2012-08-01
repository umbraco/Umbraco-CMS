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

namespace umbraco.presentation.umbracobase.library
{
    //This should be replaced with umbracos own methods...
    public class library
    {
        public static Member GetCurrentMember()
        {
			// zb-00035 #29931 : remove duplicate code?
			return Member.GetCurrentMember();
        }

        public static int CurrentMemberId()
        {
			// zb-00035 #29931 : remove duplicate code?
			return Member.CurrentMemberId();
        }
    }

    public class test
    {
        public static int member()
        {
            return Member.CurrentMemberId();
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

        [Obsolete("Deprecated, use logout(int NodeId) instead", false)]
        public static int logout()
        {
            Member m = library.GetCurrentMember();
            if (m != null)
            {
                Member.RemoveMemberFromCache(m.Id);
                Member.ClearMemberFromClient(m.Id);
                return m.Id;
            }
            else
                return 0;
        }

        public static int logout(int NodeId)
        {
            int _currentMemberId = library.CurrentMemberId();
            if (_currentMemberId > 0)
            {
                Member.RemoveMemberFromCache(library.CurrentMemberId());
                Member.ClearMemberFromClient(library.CurrentMemberId());
                return _currentMemberId;
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

					if (myMember.ContentType != null && MemberType.GetByAlias(myMember.ContentType.Alias).MemberCanEdit(myProperty.PropertyType))
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
