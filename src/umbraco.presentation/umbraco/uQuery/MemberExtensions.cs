#pragma warning disable 0618
using umbraco.cms.businesslogic.member;

namespace umbraco
{
	/// <summary>
	/// uQuery Member extensions.
	/// </summary>
	public static class MemberExtensions
	{
		/// <summary>
		/// Adds a member the group (by group name).
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="groupName">Name of the group.</param>
		public static void AddGroup(this Member member, string groupName)
		{
			if (string.IsNullOrWhiteSpace(groupName))
				return;

			member.AddGroup(MemberGroup.GetByName(groupName));
		}

		/// <summary>
		/// Adds a member the group.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="group">The group.</param>
		public static void AddGroup(this Member member, MemberGroup group)
		{
			if (group != null)
			{
				member.AddGroup(group.Id);
			}
		}

		/// <summary>
		/// Determines whether [is in group] [the specified member].
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="groupId">The group id.</param>
		/// <returns>
		/// 	<c>true</c> if [is in group] [the specified member]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsInGroup(this Member member, int groupId)
		{
			return member.Groups.ContainsKey(groupId);
		}

		/// <summary>
		/// Determines whether [is in group] [the specified member].
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="groupName">Name of the group.</param>
		/// <returns>
		/// 	<c>true</c> if [is in group] [the specified member]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsInGroup(this Member member, string groupName)
		{
			if (string.IsNullOrWhiteSpace(groupName))
				return false;

			return member.IsInGroup(MemberGroup.GetByName(groupName));
		}

		/// <summary>
		/// Determines whether [is in group] [the specified member].
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="group">The group.</param>
		/// <returns>
		/// 	<c>true</c> if [is in group] [the specified member]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsInGroup(this Member member, MemberGroup group)
		{
			if (group != null)
			{
				return member.IsInGroup(group.Id);
			}

			return false;
		}

		/// <summary>
		/// Removes a member the group (by group name).
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="groupName">Name of the group.</param>
		public static void RemoveGroup(this Member member, string groupName)
		{
			if (string.IsNullOrWhiteSpace(groupName))
				return;

			member.RemoveGroup(MemberGroup.GetByName(groupName));
		}

		/// <summary>
		/// Removes a member the group.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="group">The group.</param>
		public static void RemoveGroup(this Member member, MemberGroup group)
		{
			if (group != null)
			{
				member.RemoveGroup(group.Id);
			}
		}
	}
}
#pragma warning restore 0618