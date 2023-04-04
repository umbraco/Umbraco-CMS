// using Umbraco.Cms.Persistence.EFCore.Entities;
//
// namespace Umbraco.Cms.Persistence.EFCore;
//
// public interface IUmbracoEfCoreDatabase : IDisposable
// {
//     /// <summary>
//     ///     Gets the Sql context.
//     /// </summary>
//     UmbracoEFContext UmbracoEFContext { get; }
//
//     /// <summary>
//     ///     Gets the database instance unique identifier as a string.
//     /// </summary>
//     /// <remarks>
//     ///     UmbracoDatabase returns the first eight digits of its unique Guid and, in some
//     ///     debug mode, the underlying database connection identifier (if any).
//     /// </remarks>
//     string InstanceId { get; }
//
//     /// <summary>
//     ///     Gets a value indicating whether the database is currently in a transaction.
//     /// </summary>
//     bool InTransaction { get; }
//     // TODO: Find out what these properties do
//     // bool EnableSqlCount { get; set; }
//     //
//     // int SqlCount { get; }
//
//     Task<bool> IsUmbracoInstalled();
// }
