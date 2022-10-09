using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines part of the MemberService, which is specific to methods used by the membership provider.
/// </summary>
/// <remarks>
///     Idea is to have this as an isolated interface so that it can be easily 'replaced' in the membership provider
///     implementation.
/// </remarks>
public interface IMembershipMemberService : IMembershipMemberService<IMember>, IMembershipRoleService<IMember>
{
    /// <summary>
    ///     Creates and persists a new Member
    /// </summary>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="memberType"><see cref="IMemberType" /> which the Member should be based on</param>
    /// <returns>
    ///     <see cref="IMember" />
    /// </returns>
    IMember CreateMemberWithIdentity(string username, string email, IMemberType memberType);
}

/// <summary>
///     Defines part of the UserService/MemberService, which is specific to methods used by the membership provider.
///     The generic type is restricted to <see cref="IMembershipUser" />. The implementation of this interface  uses
///     either <see cref="IMember" /> for the MemberService or <see cref="IUser" /> for the UserService.
/// </summary>
/// <remarks>
///     Idea is to have this as an isolated interface so that it can be easily 'replaced' in the membership provider
///     implementation.
/// </remarks>
public interface IMembershipMemberService<T> : IService
    where T : class, IMembershipUser
{
    /// <summary>
    ///     Gets the total number of Members or Users based on the count type
    /// </summary>
    /// <remarks>
    ///     The way the Online count is done is the same way that it is done in the MS SqlMembershipProvider - We query for any
    ///     members
    ///     that have their last active date within the Membership.UserIsOnlineTimeWindow (which is in minutes). It isn't exact
    ///     science
    ///     but that is how MS have made theirs so we'll follow that principal.
    /// </remarks>
    /// <param name="countType"><see cref="MemberCountType" /> to count by</param>
    /// <returns><see cref="System.int" /> with number of Members or Users for passed in type</returns>
    int GetCount(MemberCountType countType);

    /// <summary>
    ///     Checks if a Member with the username exists
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns><c>True</c> if the Member exists otherwise <c>False</c></returns>
    bool Exists(string username);

    /// <summary>
    ///     Creates and persists a new <see cref="IMembershipUser" />
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="username">Username of the <see cref="IMembershipUser" /> to create</param>
    /// <param name="email">Email of the <see cref="IMembershipUser" /> to create</param>
    /// <param name="passwordValue">
    ///     This value should be the encoded/encrypted/hashed value for the password that will be
    ///     stored in the database
    /// </param>
    /// <param name="memberTypeAlias">Alias of the Type</param>
    /// <returns>
    ///     <see cref="IMembershipUser" />
    /// </returns>
    T CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias);

    /// <summary>
    ///     Creates and persists a new <see cref="IMembershipUser" />
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="username">Username of the <see cref="IMembershipUser" /> to create</param>
    /// <param name="email">Email of the <see cref="IMembershipUser" /> to create</param>
    /// <param name="passwordValue">
    ///     This value should be the encoded/encrypted/hashed value for the password that will be
    ///     stored in the database
    /// </param>
    /// <param name="memberTypeAlias">Alias of the Type</param>
    /// <param name="isApproved">IsApproved of the <see cref="IMembershipUser" /> to create</param>
    /// <returns>
    ///     <see cref="IMembershipUser" />
    /// </returns>
    T CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias, bool isApproved);

    /// <summary>
    ///     Gets an <see cref="IMembershipUser" /> by its provider key
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="id">Id to use for retrieval</param>
    /// <returns>
    ///     <see cref="IMembershipUser" />
    /// </returns>
    T? GetByProviderKey(object id);

    /// <summary>
    ///     Get an <see cref="IMembershipUser" /> by email
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="email">Email to use for retrieval</param>
    /// <returns>
    ///     <see cref="IMembershipUser" />
    /// </returns>
    T? GetByEmail(string email);

    /// <summary>
    ///     Get an <see cref="IMembershipUser" /> by username
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="username">Username to use for retrieval</param>
    /// <returns>
    ///     <see cref="IMembershipUser" />
    /// </returns>
    T? GetByUsername(string? username);

    /// <summary>
    ///     Deletes an <see cref="IMembershipUser" />
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="membershipUser"><see cref="IMember" /> or <see cref="IUser" /> to Delete</param>
    void Delete(T membershipUser);

    /// <summary>
    ///     Sets the last login date for the member if they are found by username
    /// </summary>
    /// <param name="username"></param>
    /// <param name="date"></param>
    /// <remarks>
    ///     This is a specialized method because whenever a member logs in, the membership provider requires us to set the
    ///     'online' which requires
    ///     updating their login date. This operation must be fast and cannot use database locks which is fine if we are only
    ///     executing a single query
    ///     for this data since there won't be any other data contention issues.
    /// </remarks>
    void SetLastLogin(string username, DateTime date);

    /// <summary>
    ///     Saves an <see cref="IMembershipUser" />
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="entity"><see cref="IMember" /> or <see cref="IUser" /> to Save</param>
    void Save(T entity);

    /// <summary>
    ///     Saves a list of <see cref="IMembershipUser" /> objects
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="entities"><see cref="IEnumerable{T}" /> to save</param>
    void Save(IEnumerable<T> entities);

    /// <summary>
    ///     Finds a list of <see cref="IMembershipUser" /> objects by a partial email string
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="emailStringToMatch">Partial email string to match</param>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.StartsWith" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    IEnumerable<T> FindByEmail(string emailStringToMatch, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

    /// <summary>
    ///     Finds a list of <see cref="IMembershipUser" /> objects by a partial username
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="login">Partial username to match</param>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.StartsWith" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    IEnumerable<T> FindByUsername(string login, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

    /// <summary>
    ///     Gets a list of paged <see cref="IMembershipUser" /> objects
    /// </summary>
    /// <remarks>An <see cref="IMembershipUser" /> can be of type <see cref="IMember" /> or <see cref="IUser" /></remarks>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    IEnumerable<T> GetAll(long pageIndex, int pageSize, out long totalRecords);
}
