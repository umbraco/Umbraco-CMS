using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;

namespace Umbraco.Web.BaseRest
{
	public class MemberRest
	{
		public static int GetCurrentMemberId()
		{
			return Member.CurrentMemberId();
		}

		public static Member GetCurrentMember()
		{
			return Member.GetCurrentMember();
		}

		public static int Login(string login, string password)
		{
			Member m = Member.GetMemberFromLoginNameAndPassword(login, password);
			var id = GetCurrentMemberId();
			if (id == 0)
			{
				if (m == null)
				{
					return 0;
				}
				else
				{
					Member.AddMemberToCache(m);
					return m.Id;
				}
			}
			else
			{
				return id;
			}
		}

		[Obsolete("Use Login(login, password).", false)]
		public static int login(string loginname, string password)
		{
			return Login(loginname, password);
		}

		public static int Logout()
		{
			var currentId = GetCurrentMemberId();
			if (currentId > 0)
			{
				Member.RemoveMemberFromCache(currentId);
				Member.ClearMemberFromClient(currentId);
				return currentId;
			}
			else
			{
				return 0;
			}
		}

		[Obsolete("Use Logout().", false)]
		public static int logout(int NodeId)
		{
			return Logout();
		}

		[Obsolete("Use Logout().", false)]
		public static int logout()
		{
			return Logout();
		}

		public static XPathNodeIterator Data()
		{
			var m = GetCurrentMember();
			if (m != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(m.ToXml(doc, false).OuterXml);
				XPathNavigator nav = doc.CreateNavigator();
				return nav.Select("/node");
			}
			else
			{
				return null;
			}
		}

		[Obsolete("Use Data().", false)]
		public static XPathNodeIterator data()
		{
			return Data();
		}

		[Obsolete("Use GetCurrentMemberId().", false)]
		public static int id()
		{
			return GetCurrentMemberId();
		}

		public static string SetProperty(string alias, object value)
		{
			string ret = "False";

			try
			{
				var m = GetCurrentMember();

				if (m != null)
				{
					var prop = m.getProperty(alias);

					if (m.ContentType != null && MemberType.GetByAlias(m.ContentType.Alias).MemberCanEdit(prop.PropertyType))
					{

						if (prop.PropertyType.ValidationRegExp.Trim() != "")
						{
							Regex regex = new Regex(m.getProperty(alias).PropertyType.ValidationRegExp);

							if (regex.IsMatch(value.ToString()))
							{
								prop.Value = value;
								ret = "True";
								m.XmlGenerate(new XmlDocument());
							}
						}
						else
						{
							prop.Value = value;
							ret = "True";
							m.XmlGenerate(new XmlDocument());
						}

					}
				}
			}
			catch (Exception e)
			{
				ret = e.Message;
			}

			return ret;
		}

		[Obsolete("Use SetProperty(alias, value).", false)]
		public static string setProperty(string alias, object value)
		{
			return SetProperty(alias, value);
		}
	}
}
