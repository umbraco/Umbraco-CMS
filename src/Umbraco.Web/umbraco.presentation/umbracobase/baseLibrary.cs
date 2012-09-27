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
	[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.")]
    public class library
    {
		[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.GetCurrentMember().", false)]
		public static Member GetCurrentMember()
        {
			return Member.GetCurrentMember();
        }

		[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.GetCurrentMemberId().", false)]
        public static int CurrentMemberId()
        {
			return Member.CurrentMemberId();
        }
    }

	[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.")]
    public class member
    {
		[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.Login(login, password).", false)]
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

        [Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.Logout().", false)]
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

		[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.Logout().", false)]
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

		[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.GetCurrentMemberAsXml().", false)]
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

		[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.GetCurrentMemberId().", false)]
		public static int id()
        {
            return library.CurrentMemberId();
        }

		[Obsolete("Deprecated, use Umbraco.Web.BaseRest.MemberRest.SetProperty(alias, value).", false)]
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
