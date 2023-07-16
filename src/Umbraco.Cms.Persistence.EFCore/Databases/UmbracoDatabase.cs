using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using NPoco;
using NPoco.Linq;
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

        public UmbracoDatabase(UmbracoDbContext umbracoDbContext, DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
        {
            _umbracoDbContext = umbracoDbContext;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
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
        }

        #region NPOCO specific (Remove when EF Core has replaced NPOCO - All is not implemented)
        public ISqlContext SqlContext => throw new NotImplementedException();
        public string InstanceId => throw new NotImplementedException();
        public bool InTransaction => throw new NotImplementedException();
        public bool EnableSqlCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SqlCount => throw new NotImplementedException();
        public DbConnection Connection => throw new NotImplementedException();
        public DbTransaction Transaction => throw new NotImplementedException();
        public IDictionary<string, object> Data => throw new NotImplementedException();
        public int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int OneTimeCommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public MapperCollection Mappers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPocoDataFactory PocoDataFactory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DatabaseType DatabaseType => throw new NotImplementedException();
        public List<IInterceptor> Interceptors => throw new NotImplementedException();
        public string ConnectionString => throw new NotImplementedException();
        public void AbortTransaction() => throw new NotImplementedException();
        public void AddParameter(DbCommand cmd, object value) => throw new NotImplementedException();
        public void BeginTransaction() => throw new NotImplementedException();
        public void BeginTransaction(IsolationLevel isolationLevel) => throw new NotImplementedException();
        public void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage) => throw new NotImplementedException();
        public int BulkInsertRecords<T>(IEnumerable<T> records) => throw new NotImplementedException();
        public void CloseSharedConnection() => throw new NotImplementedException();
        public void CompleteTransaction() => throw new NotImplementedException();
        public DbCommand CreateCommand(DbConnection connection, CommandType commandType, string sql, params object[] args) => throw new NotImplementedException();
        public DbParameter CreateParameter() => throw new NotImplementedException();
        public int Delete(string tableName, string primaryKeyName, object poco) => throw new NotImplementedException();
        public int Delete(string tableName, string primaryKeyName, object? poco, object? primaryKeyValue) => throw new NotImplementedException();
        public int Delete(object poco) => throw new NotImplementedException();
        public int Delete<T>(string sql, params object[] args) => throw new NotImplementedException();
        public int Delete<T>(Sql sql) => throw new NotImplementedException();
        public int Delete<T>(object pocoOrPrimaryKey) => throw new NotImplementedException();
        public Task<int> DeleteAsync(object poco) => throw new NotImplementedException();
        public IDeleteQueryProvider<T> DeleteMany<T>() => throw new NotImplementedException();
        public IAsyncDeleteQueryProvider<T> DeleteManyAsync<T>() => throw new NotImplementedException();
        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(Sql Sql) where TKey : notnull => throw new NotImplementedException();
        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(string sql, params object[] args) where TKey : notnull => throw new NotImplementedException();
        public int Execute(string sql, params object[] args) => throw new NotImplementedException();
        public int Execute(Sql sql) => throw new NotImplementedException();
        public int Execute(string sql, CommandType commandType, params object[] args) => throw new NotImplementedException();
        public Task<int> ExecuteAsync(string sql, params object[] args) => throw new NotImplementedException();
        public Task<int> ExecuteAsync(Sql sql) => throw new NotImplementedException();
        public T ExecuteScalar<T>(string sql, params object[] args) => throw new NotImplementedException();
        public T ExecuteScalar<T>(Sql sql) => throw new NotImplementedException();
        public T ExecuteScalar<T>(string sql, CommandType commandType, params object[] args) => throw new NotImplementedException();
        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<T> ExecuteScalarAsync<T>(Sql sql) => throw new NotImplementedException();
        public bool Exists<T>(object primaryKey) => throw new NotImplementedException();
        public List<object> Fetch(Type type, string sql, params object[] args) => throw new NotImplementedException();
        public List<object> Fetch(Type type, Sql Sql) => throw new NotImplementedException();
        public List<T> Fetch<T>() => throw new NotImplementedException();
        public List<T> Fetch<T>(string sql, params object[] args) => throw new NotImplementedException();
        public List<T> Fetch<T>(Sql sql) => throw new NotImplementedException();
        public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args) => throw new NotImplementedException();
        public List<T> Fetch<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();
        public Task<List<T>> FetchAsync<T>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<List<T>> FetchAsync<T>(Sql sql) => throw new NotImplementedException();
        public Task<List<T>> FetchAsync<T>() => throw new NotImplementedException();
        public Task<List<T>> FetchAsync<T>(long page, long itemsPerPage, string sql, params object[] args) => throw new NotImplementedException();
        public Task<List<T>> FetchAsync<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();
        public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();
        public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();
        public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();
        public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql) => throw new NotImplementedException();
        public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql) => throw new NotImplementedException();
        public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql) => throw new NotImplementedException();
        public (List<T1>, List<T2>) FetchMultiple<T1, T2>(string sql, params object[] args) => throw new NotImplementedException();
        public (List<T1>, List<T2>, List<T3>) FetchMultiple<T1, T2, T3>(string sql, params object[] args) => throw new NotImplementedException();
        public (List<T1>, List<T2>, List<T3>, List<T4>) FetchMultiple<T1, T2, T3, T4>(string sql, params object[] args) => throw new NotImplementedException();
        public (List<T1>, List<T2>) FetchMultiple<T1, T2>(Sql sql) => throw new NotImplementedException();
        public (List<T1>, List<T2>, List<T3>) FetchMultiple<T1, T2, T3>(Sql sql) => throw new NotImplementedException();
        public (List<T1>, List<T2>, List<T3>, List<T4>) FetchMultiple<T1, T2, T3, T4>(Sql sql) => throw new NotImplementedException();
        public Task<TRet> FetchMultipleAsync<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();
        public Task<TRet> FetchMultipleAsync<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql) => throw new NotImplementedException();
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql) => throw new NotImplementedException();
        public Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql) => throw new NotImplementedException();
        public Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(Sql sql) => throw new NotImplementedException();
        public Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(Sql sql) => throw new NotImplementedException();
        public Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(Sql sql) => throw new NotImplementedException();
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, string sql, params object[] args) => throw new NotImplementedException();
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Sql sql) => throw new NotImplementedException();
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Func<T, object> idFunc, string sql, params object[] args) => throw new NotImplementedException();
        public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Func<T, object> idFunc, Sql sql) => throw new NotImplementedException();
        public T First<T>(string sql, params object[] args) => throw new NotImplementedException();
        public T First<T>(Sql sql) => throw new NotImplementedException();
        public Task<T> FirstAsync<T>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<T> FirstAsync<T>(Sql sql) => throw new NotImplementedException();
        public T FirstInto<T>(T instance, string sql, params object[] args) => throw new NotImplementedException();
        public T FirstInto<T>(T instance, Sql sql) => throw new NotImplementedException();
        public T FirstOrDefault<T>(string sql, params object[] args) => throw new NotImplementedException();
        public T FirstOrDefault<T>(Sql sql) => throw new NotImplementedException();
        public Task<T> FirstOrDefaultAsync<T>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<T> FirstOrDefaultAsync<T>(Sql sql) => throw new NotImplementedException();
        public T FirstOrDefaultInto<T>(T instance, string sql, params object[] args) => throw new NotImplementedException();
        public T FirstOrDefaultInto<T>(T instance, Sql sql) => throw new NotImplementedException();
        public ITransaction GetTransaction() => throw new NotImplementedException();
        public ITransaction GetTransaction(IsolationLevel isolationLevel) => throw new NotImplementedException();
        public object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco) => throw new NotImplementedException();
        public object Insert<T>(string tableName, string primaryKeyName, T poco) => throw new NotImplementedException();
        public object Insert<T>(T poco) => throw new NotImplementedException();
        public Task<object> InsertAsync(string tableName, string primaryKeyName, object poco) => throw new NotImplementedException();
        public Task<object> InsertAsync<T>(T poco) => throw new NotImplementedException();
        public int InsertBatch<T>(IEnumerable<T> pocos, BatchOptions? options = null) => throw new NotImplementedException();
        public Task<int> InsertBatchAsync<T>(IEnumerable<T> pocos, BatchOptions? options = null) => throw new NotImplementedException();
        public void InsertBulk<T>(IEnumerable<T> pocos, InsertBulkOptions? options = null) => throw new NotImplementedException();
        public Task InsertBulkAsync<T>(IEnumerable<T> pocos, InsertBulkOptions? options = null) => throw new NotImplementedException();
        public bool IsNew<T>(T poco) => throw new NotImplementedException();
        public Task<bool> IsNewAsync<T>(T poco) => throw new NotImplementedException();
        public IDatabase OpenSharedConnection() => throw new NotImplementedException();
        public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args) => throw new NotImplementedException();
        public Page<T> Page<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();
        public Task<Page<T>> PageAsync<T>(long page, long itemsPerPage, string sql, params object[] args) => throw new NotImplementedException();
        public Task<Page<T>> PageAsync<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();
        public IEnumerable<object> Query(Type type, string sql, params object[] args) => throw new NotImplementedException();
        public IEnumerable<object> Query(Type type, Sql Sql) => throw new NotImplementedException();
        public IEnumerable<T> Query<T>(string sql, params object[] args) => throw new NotImplementedException();
        public IEnumerable<T> Query<T>(Sql sql) => throw new NotImplementedException();
        public IQueryProviderWithIncludes<T> Query<T>() => throw new NotImplementedException();
        public IAsyncEnumerable<T> QueryAsync<T>(string sql, params object[] args) => throw new NotImplementedException();
        public IAsyncEnumerable<T> QueryAsync<T>(Sql sql) => throw new NotImplementedException();
        public IAsyncQueryProviderWithIncludes<T> QueryAsync<T>() => throw new NotImplementedException();
        public void Save<T>(T poco) => throw new NotImplementedException();
        public Task SaveAsync<T>(T poco) => throw new NotImplementedException();
        public void SetTransaction(DbTransaction tran) => throw new NotImplementedException();
        public T Single<T>(string sql, params object[] args) => throw new NotImplementedException();
        public T Single<T>(Sql sql) => throw new NotImplementedException();
        public Task<T> SingleAsync<T>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<T> SingleAsync<T>(Sql sql) => throw new NotImplementedException();
        public T SingleById<T>(object primaryKey) => throw new NotImplementedException();
        public Task<T> SingleByIdAsync<T>(object primaryKey) => throw new NotImplementedException();
        public T SingleInto<T>(T instance, string sql, params object[] args) => throw new NotImplementedException();
        public T SingleInto<T>(T instance, Sql sql) => throw new NotImplementedException();
        public T SingleOrDefault<T>(string sql, params object[] args) => throw new NotImplementedException();
        public T SingleOrDefault<T>(Sql sql) => throw new NotImplementedException();
        public Task<T> SingleOrDefaultAsync<T>(string sql, params object[] args) => throw new NotImplementedException();
        public Task<T> SingleOrDefaultAsync<T>(Sql sql) => throw new NotImplementedException();
        public T SingleOrDefaultById<T>(object primaryKey) => throw new NotImplementedException();
        public Task<T> SingleOrDefaultByIdAsync<T>(object primaryKey) => throw new NotImplementedException();
        public T SingleOrDefaultInto<T>(T instance, string sql, params object[] args) => throw new NotImplementedException();
        public T SingleOrDefaultInto<T>(T instance, Sql sql) => throw new NotImplementedException();
        public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args) => throw new NotImplementedException();
        public List<T> SkipTake<T>(long skip, long take, Sql sql) => throw new NotImplementedException();
        public Task<List<T>> SkipTakeAsync<T>(long skip, long take, string sql, params object[] args) => throw new NotImplementedException();
        public Task<List<T>> SkipTakeAsync<T>(long skip, long take, Sql sql) => throw new NotImplementedException();
        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue) => throw new NotImplementedException();
        public int Update(string tableName, string primaryKeyName, object poco) => throw new NotImplementedException();
        public int Update(string tableName, string primaryKeyName, object poco, object? primaryKeyValue, IEnumerable<string>? columns) => throw new NotImplementedException();
        public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string>? columns) => throw new NotImplementedException();
        public int Update(object poco, IEnumerable<string> columns) => throw new NotImplementedException();
        public int Update(object poco, object primaryKeyValue, IEnumerable<string>? columns) => throw new NotImplementedException();
        public int Update<T>(T poco, Expression<Func<T, object>> fields) => throw new NotImplementedException();
        public int Update(object poco, object primaryKeyValue) => throw new NotImplementedException();
        public int Update<T>(string sql, params object[] args) => throw new NotImplementedException();
        public int Update<T>(Sql sql) => throw new NotImplementedException();
        public Task<int> UpdateAsync(object poco) => throw new NotImplementedException();
        public Task<int> UpdateAsync(object poco, IEnumerable<string> columns) => throw new NotImplementedException();
        public Task<int> UpdateAsync<T>(T poco, Expression<Func<T, object>> fields) => throw new NotImplementedException();
        public int UpdateBatch<T>(IEnumerable<UpdateBatch<T>> pocos, BatchOptions? options = null) => throw new NotImplementedException();
        public Task<int> UpdateBatchAsync<T>(IEnumerable<UpdateBatch<T>> pocos, BatchOptions? options = null) => throw new NotImplementedException();
        public IUpdateQueryProvider<T> UpdateMany<T>() => throw new NotImplementedException();
        public IAsyncUpdateQueryProvider<T> UpdateManyAsync<T>() => throw new NotImplementedException();
        int IDatabase.Update(object poco) => throw new NotImplementedException();
        #endregion
    }
}
