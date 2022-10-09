// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NPoco.DatabaseTypes;
using NPoco.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.Common.TestHelpers;

/// <summary>
///     An implementation of <see cref="IUmbracoDatabase" /> for tests.
/// </summary>
/// <remarks>
///     <para>Supports writing to the database, and logs Sql statements.</para>
///     <para>Cannot support reading from the database, and throws.</para>
///     <para>Tries to pretend it supports transactions, connections, etc. as best as possible.</para>
/// </remarks>
public class TestDatabase : IUmbracoDatabase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TestDatabase" /> class.
    /// </summary>
    /// <remarks>
    ///     <para>When both parameters are supplied, they should of course be consistent.</para>
    /// </remarks>
    public TestDatabase(DatabaseType databaseType = null, ISqlSyntaxProvider syntaxProvider = null)
    {
        DatabaseType = databaseType ?? new SqlServerDatabaseType();
        SqlContext = new SqlContext(syntaxProvider ?? new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())), DatabaseType, Mock.Of<IPocoDataFactory>());
    }

    /// <summary>
    ///     Gets the database operations.
    /// </summary>
    public List<Operation> Operations { get; } = new();

    public void Dispose()
    {
    }

    public IDatabase OpenSharedConnection() => this;

    public void CloseSharedConnection()
    {
    }

    (List<T1>, List<T2>, List<T3>, List<T4>) IDatabaseQuery.FetchMultiple<T1, T2, T3, T4>(Sql sql) =>
        throw new NotImplementedException();

    public int OneTimeCommandTimeout { get; set; }

    public MapperCollection Mappers { get; set; }

    public IPocoDataFactory PocoDataFactory { get; set; }

    public DatabaseType DatabaseType { get; }

    public List<IInterceptor> Interceptors { get; }

    public string ConnectionString { get; }

    public DbConnection Connection { get; }

    public DbTransaction Transaction { get; }

    public IDictionary<string, object> Data { get; }

    public ISqlContext SqlContext { get; }

    public string InstanceId { get; }

    public bool InTransaction { get; }

    public bool EnableSqlCount { get; set; }

    public int SqlCount { get; }

    public int BulkInsertRecords<T>(IEnumerable<T> records) => throw new NotImplementedException();
    public bool IsUmbracoInstalled() => true;

    public DatabaseSchemaResult ValidateSchema() => throw new NotImplementedException();

    public DbParameter CreateParameter() => throw new NotImplementedException();

    public void AddParameter(DbCommand cmd, object value) => throw new NotImplementedException();

    public DbCommand
        CreateCommand(DbConnection connection, CommandType commandType, string sql, params object[] args) =>
        throw new NotImplementedException();

    public ITransaction GetTransaction() => throw new NotImplementedException();

    public ITransaction GetTransaction(IsolationLevel isolationLevel) => throw new NotImplementedException();

    public void SetTransaction(DbTransaction tran) => throw new NotImplementedException();

    public void BeginTransaction() => Operations.Add(new Operation("BEGIN"));

    public void BeginTransaction(IsolationLevel isolationLevel) =>
        Operations.Add(new Operation("BEGIN " + isolationLevel));

    public void AbortTransaction() => Operations.Add(new Operation("ABORT"));

    public void CompleteTransaction() => Operations.Add(new Operation("COMMIT"));

    public int Execute(string sql, params object[] args)
    {
        Operations.Add(new Operation("EXECUTE", sql, args));
        return default;
    }

    public int Execute(Sql sql)
    {
        Operations.Add(new Operation("EXECUTE", sql.SQL, sql.Arguments));
        return default;
    }

    public int Execute(string sql, CommandType commandType, params object[] args)
    {
        Operations.Add(new Operation("EXECUTE", sql, args));
        return default;
    }

    public T ExecuteScalar<T>(string sql, params object[] args)
    {
        Operations.Add(new Operation("EXECUTE SCALAR", sql, args));
        return default;
    }

    public T ExecuteScalar<T>(Sql sql)
    {
        Operations.Add(new Operation("EXECUTE SCALAR", sql.SQL, sql.Arguments));
        return default;
    }

    public T ExecuteScalar<T>(string sql, CommandType commandType, params object[] args)
    {
        Operations.Add(new Operation("EXECUTE SCALAR", sql, args));
        return default;
    }

    public Task<T> ExecuteScalarAsync<T>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<T> ExecuteScalarAsync<T>(Sql sql) => throw new NotImplementedException();

    public Task<int> ExecuteAsync(string sql, params object[] args) => throw new NotImplementedException();

    public Task<int> ExecuteAsync(Sql sql) => throw new NotImplementedException();

    public Task<object> InsertAsync(string tableName, string primaryKeyName, object poco) =>
        throw new NotImplementedException();

    public object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco) =>
        throw new NotImplementedException();

    public object Insert<T>(string tableName, string primaryKeyName, T poco) => throw new NotImplementedException();

    public object Insert<T>(T poco) => throw new NotImplementedException();

    public void InsertBulk<T>(IEnumerable<T> pocos, InsertBulkOptions? options = null) =>
        throw new NotImplementedException();

    public Task<object> InsertAsync<T>(T poco) => throw new NotImplementedException();

    public Task InsertBulkAsync<T>(IEnumerable<T> pocos, InsertBulkOptions options = null) =>
        throw new NotImplementedException();

    public Task<int> InsertBatchAsync<T>(IEnumerable<T> pocos, BatchOptions options = null) =>
        throw new NotImplementedException();

    public Task<int> UpdateAsync(object poco) => throw new NotImplementedException();

    public Task<int> UpdateAsync(object poco, IEnumerable<string> columns) => throw new NotImplementedException();

    public Task<int> UpdateAsync<T>(T poco, Expression<Func<T, object>> fields) => throw new NotImplementedException();

    public Task<int> UpdateBatchAsync<T>(IEnumerable<UpdateBatch<T>> pocos, BatchOptions options = null) =>
        throw new NotImplementedException();

    public Task<int> DeleteAsync(object poco) => throw new NotImplementedException();

    public IAsyncUpdateQueryProvider<T> UpdateManyAsync<T>() => throw new NotImplementedException();

    public IAsyncDeleteQueryProvider<T> DeleteManyAsync<T>() => throw new NotImplementedException();
    public Task<bool> IsNewAsync<T>(T poco) => throw new NotImplementedException();

    public Task SaveAsync<T>(T poco) => throw new NotImplementedException();

    int IDatabase.InsertBatch<T>(IEnumerable<T> pocos, BatchOptions options) => throw new NotImplementedException();

    public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue) =>
        throw new NotImplementedException();

    public int Update(string tableName, string primaryKeyName, object poco) => throw new NotImplementedException();

    public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns) => throw new NotImplementedException();

    public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns) =>
        throw new NotImplementedException();

    public int Update(object poco, IEnumerable<string> columns) => throw new NotImplementedException();

    public int Update(object poco, object primaryKeyValue, IEnumerable<string> columns) =>
        throw new NotImplementedException();

    public int Update(object poco) => throw new NotImplementedException();

    public int Update<T>(T poco, Expression<Func<T, object>> fields) => throw new NotImplementedException();

    public int Update(object poco, object primaryKeyValue) => throw new NotImplementedException();

    public int Update<T>(string sql, params object[] args) => throw new NotImplementedException();

    public int Update<T>(Sql sql) => throw new NotImplementedException();

    public int UpdateBatch<T>(IEnumerable<UpdateBatch<T>> pocos, BatchOptions options = null) =>
        throw new NotImplementedException();

    public IUpdateQueryProvider<T> UpdateMany<T>() => throw new NotImplementedException();

    public int Delete(string tableName, string primaryKeyName, object poco) => throw new NotImplementedException();

    public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue) =>
        throw new NotImplementedException();

    public int Delete(object poco) => throw new NotImplementedException();

    public int Delete<T>(string sql, params object[] args) => throw new NotImplementedException();

    public int Delete<T>(Sql sql) => throw new NotImplementedException();

    public int Delete<T>(object pocoOrPrimaryKey) => throw new NotImplementedException();

    public IDeleteQueryProvider<T> DeleteMany<T>() => throw new NotImplementedException();

    public void Save<T>(T poco) => throw new NotImplementedException();

    public bool IsNew<T>(T poco) => throw new NotImplementedException();

    public List<object> Fetch(Type type, string sql, params object[] args) => throw new NotImplementedException();

    public List<object> Fetch(Type type, Sql sql) => throw new NotImplementedException();

    public IEnumerable<object> Query(Type type, string sql, params object[] args) =>
        throw new NotImplementedException();

    public IEnumerable<object> Query(Type type, Sql sql) => throw new NotImplementedException();

    public List<T> Fetch<T>() => throw new NotImplementedException();

    public List<T> Fetch<T>(string sql, params object[] args) => throw new NotImplementedException();

    public List<T> Fetch<T>(Sql sql) => throw new NotImplementedException();

    public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args) =>
        throw new NotImplementedException();

    public List<T> Fetch<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();

    public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args) =>
        throw new NotImplementedException();

    public Page<T> Page<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();

    public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args) =>
        throw new NotImplementedException();

    public List<T> SkipTake<T>(long skip, long take, Sql sql) => throw new NotImplementedException();

    public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, string sql, params object[] args) =>
        throw new NotImplementedException();

    public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Sql sql) => throw new NotImplementedException();

    public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Func<T, object> idFunc, string sql, params object[] args) => throw new NotImplementedException();

    public List<T> FetchOneToMany<T>(Expression<Func<T, IList>> many, Func<T, object> idFunc, Sql sql) =>
        throw new NotImplementedException();

    public IEnumerable<T> Query<T>(string sql, params object[] args) => throw new NotImplementedException();

    public IEnumerable<T> Query<T>(Sql sql) => throw new NotImplementedException();

    public IQueryProviderWithIncludes<T> Query<T>() => throw new NotImplementedException();

    public T SingleById<T>(object primaryKey) => throw new NotImplementedException();

    public T Single<T>(string sql, params object[] args) => throw new NotImplementedException();

    public T SingleInto<T>(T instance, string sql, params object[] args) => throw new NotImplementedException();

    public T SingleOrDefaultById<T>(object primaryKey) => throw new NotImplementedException();

    public T SingleOrDefault<T>(string sql, params object[] args) => throw new NotImplementedException();

    public T SingleOrDefaultInto<T>(T instance, string sql, params object[] args) =>
        throw new NotImplementedException();

    public T First<T>(string sql, params object[] args) => throw new NotImplementedException();

    public T FirstInto<T>(T instance, string sql, params object[] args) => throw new NotImplementedException();

    public T FirstOrDefault<T>(string sql, params object[] args) => throw new NotImplementedException();

    public T FirstOrDefaultInto<T>(T instance, string sql, params object[] args) => throw new NotImplementedException();

    public T Single<T>(Sql sql) => throw new NotImplementedException();

    public T SingleInto<T>(T instance, Sql sql) => throw new NotImplementedException();

    public T SingleOrDefault<T>(Sql sql) => throw new NotImplementedException();

    public T SingleOrDefaultInto<T>(T instance, Sql sql) => throw new NotImplementedException();

    public T First<T>(Sql sql) => throw new NotImplementedException();

    public T FirstInto<T>(T instance, Sql sql) => throw new NotImplementedException();

    public T FirstOrDefault<T>(Sql sql) => throw new NotImplementedException();

    public T FirstOrDefaultInto<T>(T instance, Sql sql) => throw new NotImplementedException();

    public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(Sql sql) => throw new NotImplementedException();

    public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(string sql, params object[] args) =>
        throw new NotImplementedException();

    public bool Exists<T>(object primaryKey) => throw new NotImplementedException();

    public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args) =>
        throw new NotImplementedException();

    public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();

    public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();

    public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql) =>
        throw new NotImplementedException();

    public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql) =>
        throw new NotImplementedException();

    public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql) =>
        throw new NotImplementedException();

    (List<T1>, List<T2>) IDatabaseQuery.FetchMultiple<T1, T2>(string sql, params object[] args) =>
        throw new NotImplementedException();

    (List<T1>, List<T2>, List<T3>) IDatabaseQuery.FetchMultiple<T1, T2, T3>(string sql, params object[] args) =>
        throw new NotImplementedException();

    (List<T1>, List<T2>, List<T3>, List<T4>) IDatabaseQuery.FetchMultiple<T1, T2, T3, T4>(string sql, params object[] args) => throw new NotImplementedException();

    (List<T1>, List<T2>) IDatabaseQuery.FetchMultiple<T1, T2>(Sql sql) => throw new NotImplementedException();

    (List<T1>, List<T2>, List<T3>) IDatabaseQuery.FetchMultiple<T1, T2, T3>(Sql sql) =>
        throw new NotImplementedException();

    public Task<T> SingleAsync<T>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<T> SingleAsync<T>(Sql sql) => throw new NotImplementedException();

    public Task<T> SingleOrDefaultAsync<T>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<T> SingleOrDefaultAsync<T>(Sql sql) => throw new NotImplementedException();

    public Task<T> SingleByIdAsync<T>(object primaryKey) => throw new NotImplementedException();

    public Task<T> SingleOrDefaultByIdAsync<T>(object primaryKey) => throw new NotImplementedException();

    public Task<T> FirstAsync<T>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<T> FirstAsync<T>(Sql sql) => throw new NotImplementedException();

    public Task<T> FirstOrDefaultAsync<T>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<T> FirstOrDefaultAsync<T>(Sql sql) => throw new NotImplementedException();

    IAsyncEnumerable<T> IAsyncQueryDatabase.QueryAsync<T>(string sql, params object[] args) =>
        throw new NotImplementedException();

    IAsyncEnumerable<T> IAsyncQueryDatabase.QueryAsync<T>(Sql sql) => throw new NotImplementedException();

    public IAsyncQueryProviderWithIncludes<T> QueryAsync<T>() => throw new NotImplementedException();

    public Task<List<T>> FetchAsync<T>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<List<T>> FetchAsync<T>(Sql sql) => throw new NotImplementedException();

    public Task<List<T>> FetchAsync<T>() => throw new NotImplementedException();

    public Task<Page<T>> PageAsync<T>(long page, long itemsPerPage, string sql, params object[] args) =>
        throw new NotImplementedException();

    public Task<Page<T>> PageAsync<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();

    public Task<List<T>> FetchAsync<T>(long page, long itemsPerPage, string sql, params object[] args) =>
        throw new NotImplementedException();

    public Task<List<T>> FetchAsync<T>(long page, long itemsPerPage, Sql sql) => throw new NotImplementedException();

    public Task<List<T>> SkipTakeAsync<T>(long skip, long take, string sql, params object[] args) =>
        throw new NotImplementedException();

    public Task<List<T>> SkipTakeAsync<T>(long skip, long take, Sql sql) => throw new NotImplementedException();

    public Task<TRet> FetchMultipleAsync<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();

    public Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();

    public Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args) => throw new NotImplementedException();

    public Task<TRet> FetchMultipleAsync<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql) =>
        throw new NotImplementedException();

    public Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql) =>
        throw new NotImplementedException();

    public Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql) => throw new NotImplementedException();

    public Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(string sql, params object[] args) =>
        throw new NotImplementedException();

    public Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(string sql, params object[] args) =>
        throw new NotImplementedException();

    public Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(Sql sql) => throw new NotImplementedException();

    public Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(Sql sql) =>
        throw new NotImplementedException();

    public Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(Sql sql) =>
        throw new NotImplementedException();

    public void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage) => throw new NotImplementedException();

    public void InsertBulk<T>(IEnumerable<T> pocos) => throw new NotImplementedException();

    public void InsertBatch<T>(IEnumerable<T> pocos, BatchOptions options = null) =>
        throw new NotImplementedException();

    public Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(string sql, params object[] args) =>
        throw new NotImplementedException();

    public Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(string sql, params object[] args) =>
        throw new NotImplementedException();

    public Tuple<List<T1>, List<T2>, List<T3>, List<T4>>
        FetchMultiple<T1, T2, T3, T4>(string sql, params object[] args) => throw new NotImplementedException();

    public Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(Sql sql) => throw new NotImplementedException();

    public Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(Sql sql) =>
        throw new NotImplementedException();

    public Tuple<List<T1>, List<T2>, List<T3>, List<T4>> FetchMultiple<T1, T2, T3, T4>(Sql sql) =>
        throw new NotImplementedException();

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] args) => throw new NotImplementedException();

    public Task<IEnumerable<T>> QueryAsync<T>(Sql sql) => throw new NotImplementedException();

    /// <summary>
    ///     Represents a database operation.
    /// </summary>
    public class Operation
    {
        public Operation(string text) => Text = text;

        public Operation(string text, string sql)
            : this(text) => Sql = sql;

        public Operation(string text, string sql, params object[] args)
            : this(text, sql) => Args = args;

        /// <summary>
        ///     Gets the operation text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        ///     Gets the operation Sql statement.
        /// </summary>
        public string Sql { get; }

        /// <summary>
        ///     Gets the operation Sql arguments.
        /// </summary>
        public object[] Args { get; }
    }
}
