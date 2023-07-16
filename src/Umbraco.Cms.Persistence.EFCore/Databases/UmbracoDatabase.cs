using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using NPoco;
using NPoco.Linq;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Models;
using Umbraco.Cms.Persistence.EFCore.DbContexts;

namespace Umbraco.Cms.Persistence.EFCore.Databases
{
    /// <summary>
    /// EF Core implementation of <see cref="IUmbracoDatabase"/>.
    /// </summary>
    public sealed class UmbracoDatabase : IUmbracoDatabase
    {
        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
        private readonly UmbracoDbContext _umbracoDbContext;

        [Obsolete("This is only used untill the NPOCO implementation can be removed")]
        public Infrastructure.Persistence.UmbracoDatabase LegacyUmbracoDatabase { get; }

        public UmbracoDatabase(UmbracoDbContext umbracoDbContext, DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
        {
            _umbracoDbContext = umbracoDbContext;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;

            // TODO: Remove this when it's time to remove the old UmbracoDatabase implementation - StaticServiceProvider is only used to avoid breaking changes
            var legacyUmbracoDatabaseFactory = (Infrastructure.Persistence.UmbracoDatabaseFactory)StaticServiceProvider.Instance.GetService(typeof(Infrastructure.Persistence.UmbracoDatabaseFactory))!;
            LegacyUmbracoDatabase = (Infrastructure.Persistence.UmbracoDatabase)legacyUmbracoDatabaseFactory.CreateDatabase();
        }

        /// <inheritdoc/>
        public IQueryable<CmsContentNu> CmsContentNus => _umbracoDbContext.CmsContentNus;

        /// <inheritdoc/>
        public IQueryable<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypes => _umbracoDbContext.CmsContentTypeAllowedContentTypes;

        /// <inheritdoc/>
        public IQueryable<CmsContentType> CmsContentTypes => _umbracoDbContext.CmsContentTypes;

        /// <inheritdoc/>
        public IQueryable<CmsDictionary> CmsDictionaries => _umbracoDbContext.CmsDictionaries;

        /// <inheritdoc/>
        public IQueryable<CmsDocumentType> CmsDocumentTypes => _umbracoDbContext.CmsDocumentTypes;

        /// <inheritdoc/>
        public IQueryable<CmsLanguageText> CmsLanguageTexts => _umbracoDbContext.CmsLanguageTexts;

        /// <inheritdoc/>
        public IQueryable<CmsMacroProperty> CmsMacroProperties => _umbracoDbContext.CmsMacroProperties;

        /// <inheritdoc/>
        public IQueryable<CmsMacro> CmsMacros => _umbracoDbContext.CmsMacros;

        /// <inheritdoc/>
        public IQueryable<CmsMember> CmsMembers => _umbracoDbContext.CmsMembers;

        /// <inheritdoc/>
        public IQueryable<CmsMemberType> CmsMemberTypes => _umbracoDbContext.CmsMemberTypes;

        /// <inheritdoc/>
        public IQueryable<CmsPropertyTypeGroup> CmsPropertyTypeGroups => _umbracoDbContext.CmsPropertyTypeGroups;

        /// <inheritdoc/>
        public IQueryable<CmsPropertyType> CmsPropertyTypes => _umbracoDbContext.CmsPropertyTypes;

        /// <inheritdoc/>
        public IQueryable<CmsTagRelationship> CmsTagRelationships => _umbracoDbContext.CmsTagRelationships;

        /// <inheritdoc/>
        public IQueryable<CmsTag> CmsTags => _umbracoDbContext.CmsTags;

        /// <inheritdoc/>
        public IQueryable<CmsTemplate> CmsTemplates => _umbracoDbContext.CmsTemplates;

        /// <inheritdoc/>
        public IQueryable<UmbracoAccess> UmbracoAccesses => _umbracoDbContext.UmbracoAccesses;

        /// <inheritdoc/>
        public IQueryable<UmbracoAccessRule> UmbracoAccessRules => _umbracoDbContext.UmbracoAccessRules;

        /// <inheritdoc/>
        public IQueryable<UmbracoAudit> UmbracoAudits => _umbracoDbContext.UmbracoAudits;

        /// <inheritdoc/>
        public IQueryable<UmbracoCacheInstruction> UmbracoCacheInstructions => _umbracoDbContext.UmbracoCacheInstructions;

        /// <inheritdoc/>
        public IQueryable<UmbracoConsent> UmbracoConsents => _umbracoDbContext.UmbracoConsents;

        /// <inheritdoc/>
        public IQueryable<UmbracoContent> UmbracoContents => _umbracoDbContext.UmbracoContents;

        /// <inheritdoc/>
        public IQueryable<UmbracoContentSchedule> UmbracoContentSchedules => _umbracoDbContext.UmbracoContentSchedules;

        /// <inheritdoc/>
        public IQueryable<UmbracoContentVersionCleanupPolicy> UmbracoContentVersionCleanupPolicies => _umbracoDbContext.UmbracoContentVersionCleanupPolicies;

        /// <inheritdoc/>
        public IQueryable<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations => _umbracoDbContext.UmbracoContentVersionCultureVariations;

        /// <inheritdoc/>
        public IQueryable<UmbracoContentVersion> UmbracoContentVersions => _umbracoDbContext.UmbracoContentVersions;

        /// <inheritdoc/>
        public IQueryable<UmbracoCreatedPackageSchema> UmbracoCreatedPackageSchemas => _umbracoDbContext.UmbracoCreatedPackageSchemas;

        /// <inheritdoc/>
        public IQueryable<UmbracoDataType> UmbracoDataTypes => _umbracoDbContext.UmbracoDataTypes;

        /// <inheritdoc/>
        public IQueryable<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations => _umbracoDbContext.UmbracoDocumentCultureVariations;

        /// <inheritdoc/>
        public IQueryable<UmbracoDocument> UmbracoDocuments => _umbracoDbContext.UmbracoDocuments;

        /// <inheritdoc/>
        public IQueryable<UmbracoDocumentVersion> UmbracoDocumentVersions => _umbracoDbContext.UmbracoDocumentVersions;

        /// <inheritdoc/>
        public IQueryable<UmbracoDomain> UmbracoDomains => _umbracoDbContext.UmbracoDomains;

        /// <inheritdoc/>
        public IQueryable<UmbracoExternalLogin> UmbracoExternalLogins => _umbracoDbContext.UmbracoExternalLogins;

        /// <inheritdoc/>
        public IQueryable<UmbracoExternalLoginToken> UmbracoExternalLoginTokens => _umbracoDbContext.UmbracoExternalLoginTokens;

        /// <inheritdoc/>
        public IQueryable<UmbracoKeyValue> UmbracoKeyValues => _umbracoDbContext.UmbracoKeyValues;

        /// <inheritdoc/>
        public IQueryable<UmbracoLanguage> UmbracoLanguages => _umbracoDbContext.UmbracoLanguages;

        /// <inheritdoc/>
        public IQueryable<UmbracoLock> UmbracoLocks => _umbracoDbContext.UmbracoLocks;

        /// <inheritdoc/>
        public IQueryable<UmbracoLog> UmbracoLogs => _umbracoDbContext.UmbracoLogs;

        /// <inheritdoc/>
        public IQueryable<UmbracoLogViewerQuery> UmbracoLogViewerQueries => _umbracoDbContext.UmbracoLogViewerQueries;

        /// <inheritdoc/>
        public IQueryable<UmbracoMediaVersion> UmbracoMediaVersions => _umbracoDbContext.UmbracoMediaVersions;

        /// <inheritdoc/>
        public IQueryable<UmbracoNode> UmbracoNodes => _umbracoDbContext.UmbracoNodes;

        /// <inheritdoc/>
        public IQueryable<UmbracoPropertyDatum> UmbracoPropertyData => _umbracoDbContext.UmbracoPropertyData;

        /// <inheritdoc/>
        public IQueryable<UmbracoRedirectUrl> UmbracoRedirectUrls => _umbracoDbContext.UmbracoRedirectUrls;

        /// <inheritdoc/>
        public IQueryable<UmbracoRelation> UmbracoRelations => _umbracoDbContext.UmbracoRelations;

        /// <inheritdoc/>
        public IQueryable<UmbracoRelationType> UmbracoRelationTypes => _umbracoDbContext.UmbracoRelationTypes;

        /// <inheritdoc/>
        public IQueryable<UmbracoServer> UmbracoServers => _umbracoDbContext.UmbracoServers;

        /// <inheritdoc/>
        public IQueryable<UmbracoTwoFactorLogin> UmbracoTwoFactorLogins => _umbracoDbContext.UmbracoTwoFactorLogins;

        /// <inheritdoc/>
        public IQueryable<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies => _umbracoDbContext.UmbracoUser2NodeNotifies;

        /// <inheritdoc/>
        public IQueryable<UmbracoUserGroup2App> UmbracoUserGroup2Apps => _umbracoDbContext.UmbracoUserGroup2Apps;

        /// <inheritdoc/>
        public IQueryable<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions => _umbracoDbContext.UmbracoUserGroup2NodePermissions;

        /// <inheritdoc/>
        public IQueryable<UmbracoUserGroup> UmbracoUserGroups => _umbracoDbContext.UmbracoUserGroups;

        /// <inheritdoc/>
        public IQueryable<UmbracoUserLogin> UmbracoUserLogins => _umbracoDbContext.UmbracoUserLogins;

        /// <inheritdoc/>
        public IQueryable<UmbracoUser> UmbracoUsers => _umbracoDbContext.UmbracoUsers;

        /// <inheritdoc/>
        public IQueryable<UmbracoUserStartNode> UmbracoUserStartNodes => _umbracoDbContext.UmbracoUserStartNodes;

        /// <inheritdoc/>
        public bool IsUmbracoInstalled() => ValidateSchema().DetermineHasInstalledVersion();

        /// <inheritdoc/>
        public DatabaseSchemaResult ValidateSchema()
        {
            DatabaseSchemaCreator? dbSchema = _databaseSchemaCreatorFactory?.Create(this);
            DatabaseSchemaResult? databaseSchemaValidationResult = dbSchema?.ValidateSchema();

            return databaseSchemaValidationResult ?? new DatabaseSchemaResult();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // TODO: Remove IDisposable from this class as it's not needed.

            LegacyUmbracoDatabase.Dispose(); // TODO: Remove this when it's time to remove the old UmbracoDatabase implementation
        }

        #region NPOCO specific (Remove when EF Core has replaced NPOCO)
        public ISqlContext SqlContext => LegacyUmbracoDatabase.SqlContext;
        public string InstanceId => LegacyUmbracoDatabase.InstanceId;
        public bool InTransaction => LegacyUmbracoDatabase.InTransaction;
        public bool EnableSqlCount { get => LegacyUmbracoDatabase.EnableSqlCount; set => LegacyUmbracoDatabase.EnableSqlCount = value; }
        public int SqlCount => LegacyUmbracoDatabase.SqlCount;
        public DbConnection? Connection => LegacyUmbracoDatabase.Connection;
        public DbTransaction? Transaction => LegacyUmbracoDatabase.Transaction;
        public IDictionary<string, object> Data => LegacyUmbracoDatabase.Data;
        public int CommandTimeout { get => LegacyUmbracoDatabase.CommandTimeout; set => LegacyUmbracoDatabase.CommandTimeout = value; }
        public int OneTimeCommandTimeout { get => LegacyUmbracoDatabase.OneTimeCommandTimeout; set => LegacyUmbracoDatabase.OneTimeCommandTimeout = value; }
        public MapperCollection Mappers { get => LegacyUmbracoDatabase.Mappers; set => LegacyUmbracoDatabase.Mappers = value; }
        public IPocoDataFactory PocoDataFactory { get => LegacyUmbracoDatabase.PocoDataFactory; set => LegacyUmbracoDatabase.PocoDataFactory = value; }
        public DatabaseType DatabaseType => LegacyUmbracoDatabase.DatabaseType;
        public List<IInterceptor> Interceptors => LegacyUmbracoDatabase.Interceptors;
        public string ConnectionString => LegacyUmbracoDatabase.ConnectionString;
        public void AbortTransaction() => LegacyUmbracoDatabase.AbortTransaction();
        public void AddParameter(DbCommand cmd, object value) => LegacyUmbracoDatabase.AddParameter(cmd, value);
        public void BeginTransaction() => LegacyUmbracoDatabase.BeginTransaction();
        public void BeginTransaction(IsolationLevel isolationLevel) => LegacyUmbracoDatabase.BeginTransaction(isolationLevel);
        public void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage) => LegacyUmbracoDatabase.BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);
        public int BulkInsertRecords<T>(IEnumerable<T> records) => LegacyUmbracoDatabase.BulkInsertRecords<T>(records);
        public void CloseSharedConnection() => LegacyUmbracoDatabase.CloseSharedConnection();
        public void CompleteTransaction() => LegacyUmbracoDatabase.CompleteTransaction();
        public DbCommand CreateCommand(DbConnection connection, CommandType commandType, string sql, params object[] args) => LegacyUmbracoDatabase.CreateCommand(connection, commandType, sql, args);
        public DbParameter CreateParameter() => LegacyUmbracoDatabase.CreateParameter();
        public int Delete(string tableName, string primaryKeyName, object poco) => LegacyUmbracoDatabase.Delete(tableName, primaryKeyName, poco);
        public int Delete(string tableName, string primaryKeyName, object? poco, object? primaryKeyValue) => LegacyUmbracoDatabase.Delete(tableName, primaryKeyName, poco, primaryKeyValue);
        public int Delete(object poco) => LegacyUmbracoDatabase.Delete(poco);
        public int Delete<T>(string sql, params object[] args) => LegacyUmbracoDatabase.Delete<T>(sql, args);
        public int Delete<T>(Sql sql) => LegacyUmbracoDatabase.Delete<T>(sql);
        public int Delete<T>(object pocoOrPrimaryKey) => LegacyUmbracoDatabase.Delete<T>(pocoOrPrimaryKey);
        public Task<int> DeleteAsync(object poco) => LegacyUmbracoDatabase.DeleteAsync(poco);
        public IDeleteQueryProvider<T> DeleteMany<T>() => LegacyUmbracoDatabase.DeleteMany<T>();
        public IAsyncDeleteQueryProvider<T> DeleteManyAsync<T>() => LegacyUmbracoDatabase.DeleteManyAsync<T>();
        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(Sql Sql) where TKey : notnull => LegacyUmbracoDatabase.Dictionary<TKey, TValue>(Sql);
        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(string sql, params object[] args) where TKey : notnull => LegacyUmbracoDatabase.Dictionary<TKey, TValue>(sql, args);
        public int Execute(string sql, params object[] args) => LegacyUmbracoDatabase.Execute(sql, args);
        public int Execute(Sql sql) => LegacyUmbracoDatabase.Execute(sql);
        public int Execute(string sql, CommandType commandType, params object[] args) => LegacyUmbracoDatabase.Execute(sql, commandType, args);
        public Task<int> ExecuteAsync(string sql, params object[] args) => LegacyUmbracoDatabase.ExecuteAsync(sql, args);
        public Task<int> ExecuteAsync(Sql sql) => LegacyUmbracoDatabase.ExecuteAsync(sql);
        public T ExecuteScalar<T>(string sql, params object[] args) => LegacyUmbracoDatabase.ExecuteScalar<T>(sql, args);
        public T ExecuteScalar<T>(Sql sql) => LegacyUmbracoDatabase.ExecuteScalar<T>(sql);
        public T ExecuteScalar<T>(string sql, CommandType commandType, params object[] args) => LegacyUmbracoDatabase.ExecuteScalar<T>(sql, commandType, args);
        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] args) => LegacyUmbracoDatabase.ExecuteScalarAsync<T>(sql, args);
        public Task<T> ExecuteScalarAsync<T>(Sql sql) => LegacyUmbracoDatabase.ExecuteScalarAsync<T>(sql);
        public bool Exists<T>(object primaryKey) => LegacyUmbracoDatabase.Exists<T>(primaryKey);
        public List<object> Fetch(Type type, string sql, params object[] args) => LegacyUmbracoDatabase.Fetch(type, sql, args);
        public List<object> Fetch(Type type, Sql Sql) => LegacyUmbracoDatabase.Fetch(type, Sql);
        public List<T> Fetch<T>() => LegacyUmbracoDatabase.Fetch<T>();
        public List<T> Fetch<T>(string sql, params object[] args) => LegacyUmbracoDatabase.Fetch<T>(sql, args);
        public List<T> Fetch<T>(Sql sql) => LegacyUmbracoDatabase.Fetch<T>(sql);
        public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args) => LegacyUmbracoDatabase.Fetch<T>(page, itemsPerPage, sql, args);
        public List<T> Fetch<T>(long page, long itemsPerPage, Sql sql) => LegacyUmbracoDatabase.Fetch<T>(page, itemsPerPage, sql);
        public Task<List<T>> FetchAsync<T>(string sql, params object[] args) => LegacyUmbracoDatabase.FetchAsync<T>(sql, args);
        public Task<List<T>> FetchAsync<T>(Sql sql) => LegacyUmbracoDatabase.FetchAsync<T>(sql);
        public Task<List<T>> FetchAsync<T>() => LegacyUmbracoDatabase.FetchAsync<T>();
        public Task<List<T>> FetchAsync<T>(long page, long itemsPerPage, string sql, params object[] args) => LegacyUmbracoDatabase.FetchAsync<T>(page, itemsPerPage, sql, args);
        public Task<List<T>> FetchAsync<T>(long page, long itemsPerPage, Sql sql) => LegacyUmbracoDatabase.FetchAsync<T>(page, itemsPerPage, sql);
        public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultiple(cb, sql, args);
        public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultiple(cb, sql, args);
        public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultiple(cb, sql, args);
        public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql) => LegacyUmbracoDatabase.FetchMultiple(cb, sql);
        public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql) => LegacyUmbracoDatabase.FetchMultiple(cb, sql);
        public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql) => LegacyUmbracoDatabase.FetchMultiple(cb, sql);
        public (List<T1>, List<T2>) FetchMultiple<T1, T2>(string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultiple<T1, T2>(sql, args);
        public (List<T1>, List<T2>, List<T3>) FetchMultiple<T1, T2, T3>(string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultiple<T1, T2, T3>(sql, args);
        public (List<T1>, List<T2>, List<T3>, List<T4>) FetchMultiple<T1, T2, T3, T4>(string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultiple<T1, T2, T3, T4>(sql, args);
        public (List<T1>, List<T2>) FetchMultiple<T1, T2>(Sql sql) => LegacyUmbracoDatabase.FetchMultiple<T1, T2>(sql);
        public (List<T1>, List<T2>, List<T3>) FetchMultiple<T1, T2, T3>(Sql sql) => LegacyUmbracoDatabase.FetchMultiple<T1, T2, T3>(sql);
        public (List<T1>, List<T2>, List<T3>, List<T4>) FetchMultiple<T1, T2, T3, T4>(Sql sql) => LegacyUmbracoDatabase.FetchMultiple<T1, T2, T3, T4>(sql);
        public Task<TRet> FetchMultipleAsync<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultipleAsync(cb, sql, args);
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultipleAsync(cb, sql, args);
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultipleAsync(cb, sql, args);
        public Task<TRet> FetchMultipleAsync<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql) => LegacyUmbracoDatabase.FetchMultipleAsync(cb, sql);
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql) => LegacyUmbracoDatabase.FetchMultipleAsync(cb, sql);
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql) => LegacyUmbracoDatabase.FetchMultipleAsync(cb, sql);
        public Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultipleAsync<T1, T2>(sql, args);
        public Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultipleAsync<T1, T2, T3>(sql, args);
        public Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(string sql, params object[] args) => LegacyUmbracoDatabase.FetchMultipleAsync<T1, T2, T3, T4>(sql, args);
        public Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(Sql sql) => LegacyUmbracoDatabase.FetchMultipleAsync<T1, T2>(sql);
        public Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(Sql sql) => LegacyUmbracoDatabase.FetchMultipleAsync<T1, T2, T3>(sql);
        public Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(Sql sql) => LegacyUmbracoDatabase.FetchMultipleAsync<T1, T2, T3, T4>(sql);
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, string sql, params object[] args) => LegacyUmbracoDatabase.FetchOneToMany<T>(many, sql, args);
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Sql sql) => LegacyUmbracoDatabase.FetchOneToMany<T>(many, sql);
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Func<T, object> idFunc, string sql, params object[] args) => LegacyUmbracoDatabase.FetchOneToMany<T>(many, idFunc, sql, args);
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Func<T, object> idFunc, Sql sql) => LegacyUmbracoDatabase.FetchOneToMany<T>(many, idFunc, sql);
        public T First<T>(string sql, params object[] args) => LegacyUmbracoDatabase.First<T>(sql, args);
        public T First<T>(Sql sql) => LegacyUmbracoDatabase.First<T>(sql);
        public Task<T> FirstAsync<T>(string sql, params object[] args) => LegacyUmbracoDatabase.FirstAsync<T>(sql, args);
        public Task<T> FirstAsync<T>(Sql sql) => LegacyUmbracoDatabase.FirstAsync<T>(sql);
        public T FirstInto<T>(T instance, string sql, params object[] args) => LegacyUmbracoDatabase.FirstInto<T>(instance, sql, args);
        public T FirstInto<T>(T instance, Sql sql) => LegacyUmbracoDatabase.FirstInto<T>(instance, sql);
        public T FirstOrDefault<T>(string sql, params object[] args) => LegacyUmbracoDatabase.FirstOrDefault<T>(sql, args);
        public T FirstOrDefault<T>(Sql sql) => LegacyUmbracoDatabase.FirstOrDefault<T>(sql);
        public Task<T> FirstOrDefaultAsync<T>(string sql, params object[] args) => LegacyUmbracoDatabase.FirstOrDefaultAsync<T>(sql, args);
        public Task<T> FirstOrDefaultAsync<T>(Sql sql) => LegacyUmbracoDatabase.FirstOrDefaultAsync<T>(sql);
        public T FirstOrDefaultInto<T>(T instance, string sql, params object[] args) => LegacyUmbracoDatabase.FirstOrDefaultInto<T>(instance, sql, args);
        public T FirstOrDefaultInto<T>(T instance, Sql sql) => LegacyUmbracoDatabase.FirstOrDefaultInto<T>(instance, sql);
        public ITransaction GetTransaction() => LegacyUmbracoDatabase.GetTransaction();
        public ITransaction GetTransaction(IsolationLevel isolationLevel) => LegacyUmbracoDatabase.GetTransaction(isolationLevel);
        public object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco) => LegacyUmbracoDatabase.Insert<T>(tableName, primaryKeyName, autoIncrement, poco);
        public object Insert<T>(string tableName, string primaryKeyName, T poco) => LegacyUmbracoDatabase.Insert<T>(tableName, primaryKeyName, poco);
        public object Insert<T>(T poco) => LegacyUmbracoDatabase.Insert<T>(poco);
        public Task<object> InsertAsync(string tableName, string primaryKeyName, object poco) => LegacyUmbracoDatabase.InsertAsync(tableName, primaryKeyName, poco);
        public Task<object> InsertAsync<T>(T poco) => LegacyUmbracoDatabase.InsertAsync<T>(poco);
        public int InsertBatch<T>(IEnumerable<T> pocos, BatchOptions? options = null) => LegacyUmbracoDatabase.InsertBatch<T>(pocos, options);
        public Task<int> InsertBatchAsync<T>(IEnumerable<T> pocos, BatchOptions? options = null) => LegacyUmbracoDatabase.InsertBatchAsync<T>(pocos, options);
        public void InsertBulk<T>(IEnumerable<T> pocos, InsertBulkOptions? options = null) => LegacyUmbracoDatabase.InsertBulk<T>(pocos, options);
        public Task InsertBulkAsync<T>(IEnumerable<T> pocos, InsertBulkOptions? options = null) => LegacyUmbracoDatabase.InsertBulkAsync<T>(pocos, options);
        public bool IsNew<T>(T poco) => LegacyUmbracoDatabase.IsNew<T>(poco);
        public Task<bool> IsNewAsync<T>(T poco) => LegacyUmbracoDatabase.IsNewAsync<T>(poco);
        public IDatabase OpenSharedConnection() => LegacyUmbracoDatabase.OpenSharedConnection();
        public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args) => LegacyUmbracoDatabase.Page<T>(page, itemsPerPage, sql, args);
        public Page<T> Page<T>(long page, long itemsPerPage, Sql sql) => LegacyUmbracoDatabase.Page<T>(page, itemsPerPage, sql);
        public Task<Page<T>> PageAsync<T>(long page, long itemsPerPage, string sql, params object[] args) => LegacyUmbracoDatabase.PageAsync<T>(page, itemsPerPage, sql, args);
        public Task<Page<T>> PageAsync<T>(long page, long itemsPerPage, Sql sql) => LegacyUmbracoDatabase.PageAsync<T>(page, itemsPerPage, sql);
        public IEnumerable<object> Query(Type type, string sql, params object[] args) => LegacyUmbracoDatabase.Query(type, sql, args);
        public IEnumerable<object> Query(Type type, Sql Sql) => LegacyUmbracoDatabase.Query(type, Sql);
        public IEnumerable<T> Query<T>(string sql, params object[] args) => LegacyUmbracoDatabase.Query<T>(sql, args);
        public IEnumerable<T> Query<T>(Sql sql) => LegacyUmbracoDatabase.Query<T>(sql);
        public IQueryProviderWithIncludes<T> Query<T>() => LegacyUmbracoDatabase.Query<T>();
        public IAsyncEnumerable<T> QueryAsync<T>(string sql, params object[] args) => LegacyUmbracoDatabase.QueryAsync<T>(sql, args);
        public IAsyncEnumerable<T> QueryAsync<T>(Sql sql) => LegacyUmbracoDatabase.QueryAsync<T>(sql);
        public IAsyncQueryProviderWithIncludes<T> QueryAsync<T>() => LegacyUmbracoDatabase.QueryAsync<T>();
        public void Save<T>(T poco) => LegacyUmbracoDatabase.Save<T>(poco);
        public Task SaveAsync<T>(T poco) => LegacyUmbracoDatabase.SaveAsync<T>(poco);
        public void SetTransaction(DbTransaction tran) => LegacyUmbracoDatabase.SetTransaction(tran);
        public T Single<T>(string sql, params object[] args) => LegacyUmbracoDatabase.Single<T>(sql, args);
        public T Single<T>(Sql sql) => LegacyUmbracoDatabase.Single<T>(sql);
        public Task<T> SingleAsync<T>(string sql, params object[] args) => LegacyUmbracoDatabase.SingleAsync<T>(sql, args);
        public Task<T> SingleAsync<T>(Sql sql) => LegacyUmbracoDatabase.SingleAsync<T>(sql);
        public T SingleById<T>(object primaryKey) => LegacyUmbracoDatabase.SingleById<T>(primaryKey);
        public Task<T> SingleByIdAsync<T>(object primaryKey) => LegacyUmbracoDatabase.SingleByIdAsync<T>(primaryKey);
        public T SingleInto<T>(T instance, string sql, params object[] args) => LegacyUmbracoDatabase.SingleInto<T>(instance, sql, args);
        public T SingleInto<T>(T instance, Sql sql) => LegacyUmbracoDatabase.SingleInto<T>(instance, sql);
        public T SingleOrDefault<T>(string sql, params object[] args) => LegacyUmbracoDatabase.SingleOrDefault<T>(sql, args);
        public T SingleOrDefault<T>(Sql sql) => LegacyUmbracoDatabase.SingleOrDefault<T>(sql);
        public Task<T> SingleOrDefaultAsync<T>(string sql, params object[] args) => LegacyUmbracoDatabase.SingleOrDefaultAsync<T>(sql, args);
        public Task<T> SingleOrDefaultAsync<T>(Sql sql) => LegacyUmbracoDatabase.SingleOrDefaultAsync<T>(sql);
        public T SingleOrDefaultById<T>(object primaryKey) => LegacyUmbracoDatabase.SingleOrDefaultById<T>(primaryKey);
        public Task<T> SingleOrDefaultByIdAsync<T>(object primaryKey) => LegacyUmbracoDatabase.SingleOrDefaultByIdAsync<T>(primaryKey);
        public T SingleOrDefaultInto<T>(T instance, string sql, params object[] args) => LegacyUmbracoDatabase.SingleOrDefaultInto<T>(instance, sql, args);
        public T SingleOrDefaultInto<T>(T instance, Sql sql) => LegacyUmbracoDatabase.SingleOrDefaultInto<T>(instance, sql);
        public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args) => LegacyUmbracoDatabase.SkipTake<T>(skip, take, sql, args);
        public List<T> SkipTake<T>(long skip, long take, Sql sql) => LegacyUmbracoDatabase.SkipTake<T>(skip, take, sql);
        public Task<List<T>> SkipTakeAsync<T>(long skip, long take, string sql, params object[] args) => LegacyUmbracoDatabase.SkipTakeAsync<T>(skip, take, sql, args);
        public Task<List<T>> SkipTakeAsync<T>(long skip, long take, Sql sql) => LegacyUmbracoDatabase.SkipTakeAsync<T>(skip, take, sql);
        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue) => LegacyUmbracoDatabase.Update(tableName, primaryKeyName, poco, primaryKeyValue);
        public int Update(string tableName, string primaryKeyName, object poco) => LegacyUmbracoDatabase.Update(tableName, primaryKeyName, poco);
        public int Update(string tableName, string primaryKeyName, object poco, object? primaryKeyValue, IEnumerable<string>? columns) => LegacyUmbracoDatabase.Update(tableName, primaryKeyName, poco, primaryKeyValue, columns);
        public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string>? columns) => LegacyUmbracoDatabase.Update(tableName, primaryKeyName, poco, columns);
        public int Update(object poco, IEnumerable<string> columns) => LegacyUmbracoDatabase.Update(poco, columns);
        public int Update(object poco, object primaryKeyValue, IEnumerable<string>? columns) => LegacyUmbracoDatabase.Update(poco, primaryKeyValue, columns);
        public int Update<T>(T poco, Expression<Func<T, object>> fields) => LegacyUmbracoDatabase.Update<T>(poco, fields);
        public int Update(object poco, object primaryKeyValue) => LegacyUmbracoDatabase.Update(poco, primaryKeyValue);
        public int Update<T>(string sql, params object[] args) => LegacyUmbracoDatabase.Update<T>(sql, args);
        public int Update<T>(Sql sql) => LegacyUmbracoDatabase.Update<T>(sql);
        public Task<int> UpdateAsync(object poco) => LegacyUmbracoDatabase.UpdateAsync(poco);
        public Task<int> UpdateAsync(object poco, IEnumerable<string> columns) => LegacyUmbracoDatabase.UpdateAsync(poco, columns);
        public Task<int> UpdateAsync<T>(T poco, Expression<Func<T, object>> fields) => LegacyUmbracoDatabase.UpdateAsync<T>(poco, fields);
        public int UpdateBatch<T>(IEnumerable<UpdateBatch<T>> pocos, BatchOptions? options = null) => LegacyUmbracoDatabase.UpdateBatch<T>(pocos, options);
        public Task<int> UpdateBatchAsync<T>(IEnumerable<UpdateBatch<T>> pocos, BatchOptions? options = null) => LegacyUmbracoDatabase.UpdateBatchAsync<T>(pocos, options);
        public IUpdateQueryProvider<T> UpdateMany<T>() => LegacyUmbracoDatabase.UpdateMany<T>();
        public IAsyncUpdateQueryProvider<T> UpdateManyAsync<T>() => LegacyUmbracoDatabase.UpdateManyAsync<T>();
        int IDatabase.Update(object poco) => LegacyUmbracoDatabase.Update(poco);
        #endregion
    }
}
