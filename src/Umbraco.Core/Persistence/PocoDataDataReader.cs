﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// A data reader used for reading collections of PocoData entity types
    /// </summary>
    /// <remarks>
    /// We are using a custom data reader so that tons of memory is not consumed when rebuilding this table, previously
    /// we'd generate SQL insert statements, but we'd have to put all of the XML structures into memory first. Alternatively
    /// we can use .net's DataTable, but this also requires putting everything into memory. By using a DataReader we don't have to
    /// store every content item and it's XML structure in memory to get it into the DB, we can stream it into the db with this
    /// reader.
    /// </remarks>
    internal class PocoDataDataReader<T, TSyntax> : BulkDataReader
        where TSyntax : ISqlSyntaxProvider
    {
        private readonly MicrosoftSqlSyntaxProviderBase<TSyntax> _sqlSyntaxProvider;
        private readonly TableDefinition _tableDefinition;
        private readonly PocoColumn[] _readerColumns;
        private readonly IEnumerator<T> _enumerator;
        private readonly ColumnDefinition[] _columnDefinitions;
        private int _recordsAffected = -1;

        public PocoDataDataReader(
            IEnumerable<T> dataSource,
            PocoData pd,
            MicrosoftSqlSyntaxProviderBase<TSyntax> sqlSyntaxProvider)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (sqlSyntaxProvider == null) throw new ArgumentNullException(nameof(sqlSyntaxProvider));

            _tableDefinition = DefinitionFactory.GetTableDefinition(pd.Type, sqlSyntaxProvider);
            if (_tableDefinition == null) throw new InvalidOperationException("No table definition found for type " + pd.Type);

            // only real columns, exclude result columns
            _readerColumns = pd.Columns
                .Where(x => x.Value.ResultColumn == false)
                .Select(x => x.Value)
                .ToArray();

            _sqlSyntaxProvider = sqlSyntaxProvider;
            _enumerator = dataSource.GetEnumerator();
            _columnDefinitions = _tableDefinition.Columns.ToArray();

        }

        protected override string SchemaName => _tableDefinition.SchemaName;

        protected override string TableName => _tableDefinition.Name;

        public override int RecordsAffected => _recordsAffected <= 0 ? -1 : _recordsAffected;

        /// <summary>
        /// This will automatically add the schema rows based on the Poco table definition and the columns passed in
        /// </summary>
        protected override void AddSchemaTableRows()
        {
            //var colNames = _readerColumns.Select(x => x.ColumnName).ToArray();
            //foreach (var col in _columnDefinitions.Where(x => colNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
            foreach (var col in _columnDefinitions)
            {
                SqlDbType sqlDbType;
                if (col.HasSpecialDbType)
                {
                    //get the SqlDbType from the 'special type'
                    switch (col.DbType)
                    {
                        case SpecialDbTypes.NTEXT:
                            sqlDbType = SqlDbType.NText;
                            break;
                        case SpecialDbTypes.NCHAR:
                            sqlDbType = SqlDbType.NChar;
                            break;
                        case SpecialDbTypes.NVARCHARMAX:
                            sqlDbType = SqlDbType.NVarChar;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (col.Type.HasValue)
                {
                    //get the SqlDbType from the DbType
                    sqlDbType = _sqlSyntaxProvider.GetSqlDbType(col.Type.Value);
                }
                else
                {
                    //get the SqlDbType from the CLR type
                    sqlDbType = _sqlSyntaxProvider.GetSqlDbType(col.PropertyType);
                }

                AddSchemaTableRow(
                    col.Name,
                    col.Size > 0 ? (int?)col.Size : null,
                    col.Precision > 0  ? (short?)col.Precision : null,
                    null, col.IsUnique, col.IsIdentity, col.IsNullable, sqlDbType,
                    null, null, null, null, null);
            }
        }

        /// <summary>
        /// Get the value from the column index for the current object
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override object GetValue(int i)
        {
            return _enumerator.Current == null ? null : _readerColumns[i].GetValue(_enumerator.Current);
        }

        /// <summary>
        /// Advance the cursor
        /// </summary>
        /// <returns></returns>
        public override bool Read()
        {
            var result = _enumerator.MoveNext();
            if (result)
            {
                if (_recordsAffected == -1)
                    _recordsAffected = 0;
                _recordsAffected++;
            }
            return result;
        }

        /// <summary>
        /// Ensure the enumerator is disposed
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                _enumerator.Dispose();
        }
    }
}
