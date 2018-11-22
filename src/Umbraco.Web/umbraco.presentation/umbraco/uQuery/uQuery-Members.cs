using System.Collections.Generic;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
	public static partial class uQuery
	{
		/// <summary>
		/// Get a collection of members from an XPath expression (note XML source is currently a flat structure)
		/// </summary>
		/// <param name="xPath">XPath expression</param>
		/// <returns>collection or empty collection</returns>
		public static IEnumerable<Member> GetMembersByXPath(string xPath)
		{
			var members = new List<Member>();
			var xmlDocument = uQuery.GetPublishedXml(UmbracoObjectType.Member);
			var xPathNavigator = xmlDocument.CreateNavigator();
			var xPathNodeIterator = xPathNavigator.Select(xPath);

			while (xPathNodeIterator.MoveNext())
			{
				var member = uQuery.GetMember(xPathNodeIterator.Current.Evaluate("string(@id)").ToString());
				if (member != null)
				{
					members.Add(member);
				}
			}

			return members;
		}

		/// <summary>
		/// Get collection of member objects from the supplied CSV of IDs
		/// </summary>
		/// <param name="csv">string csv of IDs</param>
		/// <returns>collection or emtpy collection</returns>
		public static IEnumerable<Member> GetMembersByCsv(string csv)
		{
			var members = new List<Member>();
			var ids = uQuery.GetCsvIds(csv);

			if (ids != null)
			{
				foreach (string id in ids)
				{
					var member = uQuery.GetMember(id);
					if (member != null)
					{
						members.Add(member);
					}
				}
			}

			return members;
		}

		/// <summary>
		/// Gets the members by XML.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static IEnumerable<Member> GetMembersByXml(string xml)
		{
			var members = new List<Member>();
			var ids = uQuery.GetXmlIds(xml);

			foreach (int id in ids)
			{
				var member = uQuery.GetMember(id);
				if (member != null)
				{
					members.Add(member);
				}
			}

			return members;
		}

		/// <summary>
		/// Get Members by member type alias
		/// </summary>
		/// <param name="memberTypeAlias">The member type alias</param>
		/// <returns>list of members, or empty list</returns>
		public static IEnumerable<Member> GetMembersByType(string memberTypeAlias)
		{
			// Both XML schema versions have this attribute
			return uQuery.GetMembersByXPath(string.Concat("descendant::*[@nodeTypeAlias='", memberTypeAlias, "']"));
		}

		// public static Member GetCurrentMember() { }

		/// <summary>
		/// Get member from an ID
		/// </summary>
		/// <param name="memberId">string representation of an umbraco.cms.businesslogic.member.Member object Id</param>
		/// <returns>member or null</returns>
		public static Member GetMember(string memberId)
		{
			int id;
			Member member = null;

			// suppress error if member with supplied id doesn't exist
			if (int.TryParse(memberId, out id))
			{
				member = uQuery.GetMember(id);
			}

			return member;
		}

		/// <summary>
		/// Get member from an ID
		/// </summary>
		/// <param name="id">ID of member item to get</param>
		/// <returns>member or null</returns>
		public static Member GetMember(int id)
		{
			// suppress error if member with supplied id doesn't exist
			try
			{
				return new Member(id);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Extension method on Member collection to return key value pairs of: member.Id / member.loginName
		/// </summary>
		/// <param name="members">generic list of Member objects</param>
		/// <returns>a collection of memberIDs and their login names</returns>
		public static Dictionary<int, string> ToNameIds(this IEnumerable<Member> members)
		{
			var dictionary = new Dictionary<int, string>();

			foreach (var member in members)
			{
				if (member != null)
				{
					dictionary.Add(member.Id, member.LoginName);
				}
			}

			return dictionary;
		}

		public static class Members
		{
			/// <summary>
			/// Adds the member to group.
			/// </summary>
			/// <param name="memberId">The member id.</param>
			/// <param name="groupId">The group id.</param>
			public static void AddMemberToGroup(int memberId, int groupId)
			{
				AddMembersToGroup(new int[] { memberId }, groupId);
			}

			/// <summary>
			/// Adds the members to group.
			/// </summary>
			/// <param name="memberIds">The member ids.</param>
			/// <param name="groupId">The group id.</param>
			public static void AddMembersToGroup(int[] memberIds, int groupId)
			{
				AddMembersToGroups(memberIds, new int[] { groupId });
			}

			/// <summary>
			/// Adds the members to groups.
			/// </summary>
			/// <param name="memberIds">The member ids.</param>
			/// <param name="groupIds">The group ids.</param>
			public static void AddMembersToGroups(int[] memberIds, int[] groupIds)
			{
				foreach (var memberId in memberIds)
				{
					if (memberId > 0)
					{
						var member = GetMember(memberId);

						if (member != null)
						{
							foreach (var groupId in groupIds)
							{
#pragma warning disable 0618
								member.AddGroup(groupId);
#pragma warning restore 0618
							}
						}
					}
				}
			}

			/// <summary>
			/// Adds the members to groups.
			/// </summary>
			/// <param name="memberIds">The member ids.</param>
			/// <param name="groupNames">The group names.</param>
			public static void AddMembersToGroups(int[] memberIds, string[] groupNames)
			{
				var groupIds = new List<int>();

				foreach (var groupName in groupNames)
				{
					var group = MemberGroup.GetByName(groupName);

					if (group != null)
					{
						groupIds.Add(group.Id);
					}
				}

				AddMembersToGroups(memberIds, groupIds.ToArray());
			}

			/// <summary>
			/// Removes the member from group.
			/// </summary>
			/// <param name="memberId">The member id.</param>
			/// <param name="groupId">The group id.</param>
			public static void RemoveMemberFromGroup(int memberId, int groupId)
			{
				RemoveMembersFromGroup(new int[] { memberId }, groupId);
			}

			/// <summary>
			/// Removes the members from group.
			/// </summary>
			/// <param name="memberIds">The member ids.</param>
			/// <param name="groupId">The group id.</param>
			public static void RemoveMembersFromGroup(int[] memberIds, int groupId)
			{
				RemoveMembersFromGroups(memberIds, new int[] { groupId });
			}

			/// <summary>
			/// Removes the members from groups.
			/// </summary>
			/// <param name="memberIds">The member ids.</param>
			/// <param name="groupIds">The group ids.</param>
			public static void RemoveMembersFromGroups(int[] memberIds, int[] groupIds)
			{
				foreach (var memberId in memberIds)
				{
					if (memberId > 0)
					{
						var member = GetMember(memberId);

						if (member != null)
						{
							foreach (var groupId in groupIds)
							{
#pragma warning disable 0618
								member.RemoveGroup(groupId);
#pragma warning restore 0618
							}
						}
					}
				}
			}

			/// <summary>
			/// Removes the members from groups.
			/// </summary>
			/// <param name="memberIds">The member ids.</param>
			/// <param name="groupNames">The group names.</param>
			public static void RemoveMembersFromGroups(int[] memberIds, string[] groupNames)
			{
				var groupIds = new List<int>();

				foreach (var groupName in groupNames)
				{
					var group = MemberGroup.GetByName(groupName);

					if (group != null)
					{
						groupIds.Add(group.Id);
					}
				}

				RemoveMembersFromGroups(memberIds, groupIds.ToArray());
			}
		}
	}
}