using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the MemberService, which is an easy access to operations involving (umbraco) members.
/// </summary>
public interface IMemberService : IMembershipMemberService
{
    /// <summary>
    ///     Gets a list of paged <see cref="IMember" /> objects
    /// </summary>
    /// <remarks>An <see cref="IMember" /> can be of type <see cref="IMember" /> </remarks>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <param name="orderBy">Field to order by</param>
    /// <param name="orderDirection">Direction to order by</param>
    /// <param name="memberTypeAlias"></param>
    /// <param name="filter">Search text filter</param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    IEnumerable<IMember> GetAll(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string orderBy,
        Direction orderDirection,
        string? memberTypeAlias = null,
        string filter = "");

    /// <summary>
    ///     Gets a list of paged <see cref="IMember" /> objects
    /// </summary>
    /// <remarks>An <see cref="IMember" /> can be of type <see cref="IMember" /> </remarks>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <param name="orderBy">Field to order by</param>
    /// <param name="orderDirection">Direction to order by</param>
    /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
    /// <param name="memberTypeAlias"></param>
    /// <param name="filter">Search text filter</param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    IEnumerable<IMember> GetAll(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string orderBy,
        Direction orderDirection,
        bool orderBySystemField,
        string? memberTypeAlias,
        string filter);

    /// <summary>
    ///     Creates an <see cref="IMember" /> object without persisting it
    /// </summary>
    /// <remarks>
    ///     This method is convenient for when you need to add properties to a new Member
    ///     before persisting it in order to limit the amount of times its saved.
    ///     Also note that the returned <see cref="IMember" /> will not have an Id until its saved.
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="name">Name of the Member to create</param>
    /// <param name="memberTypeAlias">Alias of the MemberType the Member should be based on</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMember(string username, string email, string name, string memberTypeAlias);

    /// <summary>
    ///     Creates an <see cref="IMember" /> object without persisting it
    /// </summary>
    /// <remarks>
    ///     This method is convenient for when you need to add properties to a new Member
    ///     before persisting it in order to limit the amount of times its saved.
    ///     Also note that the returned <see cref="IMember" /> will not have an Id until its saved.
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="name">Name of the Member to create</param>
    /// <param name="memberType">MemberType the Member should be based on</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMember(string username, string email, string name, IMemberType memberType);

    /// <summary>
    ///     Creates and persists a Member
    /// </summary>
    /// <remarks>
    ///     Using this method will persist the Member object before its returned
    ///     meaning that it will have an Id available (unlike the CreateMember method)
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="memberTypeAlias">Alias of the MemberType the Member should be based on</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMemberWithIdentity(string username, string email, string memberTypeAlias) =>
        throw new NotImplementedException();

    /// <summary>
    ///     Creates and persists a Member
    /// </summary>
    /// <remarks>
    ///     Using this method will persist the Member object before its returned
    ///     meaning that it will have an Id available (unlike the CreateMember method)
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="memberTypeAlias">Alias of the MemberType the Member should be based on</param>
    /// <param name="isApproved">Whether the member is approved or not</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMemberWithIdentity(string username, string email, string memberTypeAlias, bool isApproved) =>
        throw new NotImplementedException();

    /// <summary>
    ///     Creates and persists a Member
    /// </summary>
    /// <remarks>
    ///     Using this method will persist the Member object before its returned
    ///     meaning that it will have an Id available (unlike the CreateMember method)
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="name">Name of the Member to create</param>
    /// <param name="memberTypeAlias">Alias of the MemberType the Member should be based on</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMemberWithIdentity(string username, string email, string name, string memberTypeAlias);

    /// <summary>
    ///     Creates and persists a Member
    /// </summary>
    /// <remarks>
    ///     Using this method will persist the Member object before its returned
    ///     meaning that it will have an Id available (unlike the CreateMember method)
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="name">Name of the Member to create</param>
    /// <param name="memberTypeAlias">Alias of the MemberType the Member should be based on</param>
    /// <param name="isApproved">Whether the member is approved or not</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMemberWithIdentity(string username, string email, string name, string memberTypeAlias, bool isApproved)
        => throw new NotImplementedException();

    /// <summary>
    ///     Creates and persists a Member
    /// </summary>
    /// <remarks>
    ///     Using this method will persist the Member object before its returned
    ///     meaning that it will have an Id available (unlike the CreateMember method)
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="name">Name of the Member to create</param>
    /// <param name="memberType">MemberType the Member should be based on</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMemberWithIdentity(string username, string email, string name, IMemberType memberType);

    /// <summary>
    ///     Gets the count of Members by an optional MemberType alias
    /// </summary>
    /// <remarks>If no alias is supplied then the count for all Member will be returned</remarks>
    /// <param name="memberTypeAlias">Optional alias for the MemberType when counting number of Members</param>
    /// <returns><see cref="System.int" /> with number of Members</returns>
    int Count(string? memberTypeAlias = null);

    /// <summary>
    ///     Checks if a Member with the id exists
    /// </summary>
    /// <param name="id">Id of the Member</param>
    /// <returns><c>True</c> if the Member exists otherwise <c>False</c></returns>
    bool Exists(int id);

    /// <summary>
    ///     Gets a Member by the unique key
    /// </summary>
    /// <remarks>
    ///     The guid key corresponds to the unique id in the database
    ///     and the user id in the membership provider.
    /// </remarks>
    /// <param name="id"><see cref="Guid" /> Id</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember? GetByKey(Guid id);

    /// <summary>
    ///     Gets a Member by its integer id
    /// </summary>
    /// <param name="id"><see cref="System.int" /> Id</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember? GetById(int id);

    /// <summary>
    ///     Gets all Members for the specified MemberType alias
    /// </summary>
    /// <param name="memberTypeAlias">Alias of the MemberType</param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember> GetMembersByMemberType(string memberTypeAlias);

    /// <summary>
    ///     Gets all Members for the MemberType id
    /// </summary>
    /// <param name="memberTypeId">Id of the MemberType</param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember> GetMembersByMemberType(int memberTypeId);

    /// <summary>
    ///     Gets all Members within the specified MemberGroup name
    /// </summary>
    /// <param name="memberGroupName">Name of the MemberGroup</param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember> GetMembersByGroup(string memberGroupName);

    /// <summary>
    ///     Gets all Members with the ids specified
    /// </summary>
    /// <remarks>If no Ids are specified all Members will be retrieved</remarks>
    /// <param name="ids">Optional list of Member Ids</param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember> GetAllMembers(params int[] ids);

    /// <summary>
    ///     Delete Members of the specified MemberType id
    /// </summary>
    /// <param name="memberTypeId">Id of the MemberType</param>
    void DeleteMembersOfType(int memberTypeId);

    /// <summary>
    ///     Finds Members based on their display name
    /// </summary>
    /// <param name="displayNameToMatch">Display name to match</param>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.StartsWith" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember> FindMembersByDisplayName(
        string displayNameToMatch,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

    /// <summary>
    ///     Gets a list of Members based on a property search
    /// </summary>
    /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
    /// <param name="value"><see cref="System.string" /> Value to match</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.Exact" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember>? GetMembersByPropertyValue(
        string propertyTypeAlias,
        string value,
        StringPropertyMatchType matchType = StringPropertyMatchType.Exact);

    /// <summary>
    ///     Gets a list of Members based on a property search
    /// </summary>
    /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
    /// <param name="value"><see cref="System.int" /> Value to match</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.Exact" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember>? GetMembersByPropertyValue(string propertyTypeAlias, int value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact);

    /// <summary>
    ///     Gets a list of Members based on a property search
    /// </summary>
    /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
    /// <param name="value"><see cref="System.bool" /> Value to match</param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember>? GetMembersByPropertyValue(string propertyTypeAlias, bool value);

    /// <summary>
    ///     Gets a list of Members based on a property search
    /// </summary>
    /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
    /// <param name="value"><see cref="System.DateTime" /> Value to match</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.Exact" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    IEnumerable<IMember>? GetMembersByPropertyValue(string propertyTypeAlias, DateTime value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact);
}
