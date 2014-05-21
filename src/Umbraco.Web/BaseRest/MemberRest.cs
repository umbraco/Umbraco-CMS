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
    [Obsolete("Umbraco /base is obsoleted, use WebApi (UmbracoApiController) instead for all REST based logic")]
	public static class MemberRest
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

		public static XPathNodeIterator GetCurrentMemberAsXml()
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
							    m.Save();
							}
						}
						else
						{
							prop.Value = value;
							ret = "True";
                            m.Save();
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
	}
}
