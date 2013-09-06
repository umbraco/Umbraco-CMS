using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using umbraco.DataLayer.Utility.Table;

namespace umbraco.DataLayer.SqlHelpers.SqlServer
{
    /// <summary>
    /// SQL Server implementation of <see cref="DefaultTableUtility&lt;S&gt;"/>.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class SqlServerTableUtility : DefaultTableUtility<SqlServerHelper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerTableUtility"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public SqlServerTableUtility(SqlServerHelper sqlHelper)
            : base(sqlHelper)
        { }

        #region DefaultTableUtility<SqlServerHelper> members

        /// <summary>
        /// Gets the table with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The table, or <c>null</c> if no table with that name exists.</returns>
        public override ITable GetTable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            ITable table = null;

            // get name in correct casing
            name = SqlHelper.ExecuteScalar<string>("SELECT name FROM sys.tables WHERE name=@name",
                                                          SqlHelper.CreateParameter("name", name));
            if (name != null)
            {
                table = new DefaultTable(name);

                using (IRecordsReader reader = SqlHelper.ExecuteReader(
                        @"SELECT c.name AS Name, st.name AS DataType, c.max_length, c.is_nullable, c.is_identity
                          FROM sys.tables AS t
                            JOIN sys.columns AS c ON t.object_id = c.object_id
                            JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                            JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
                            JOIN sys.types st ON ty.system_type_id = st.user_type_id
                          WHERE t.name = @name
                          ORDER BY c.column_id", SqlHelper.CreateParameter("name", name)))
                {
                    while (reader.Read())
                    {
                        table.AddField(table.CreateField(reader.GetString("Name"), GetType(reader)));
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Saves or updates the table.
        /// </summary>
        /// <param name="table">The table to be saved.</param>
        public override void SaveTable(ITable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");

            ITable oldTable = GetTable(table.Name);

            // create the table if it didn't exist, update fields otherwise
            if (oldTable == null)
            {
                CreateTable(table);
            }
            else
            {
                foreach (IField field in table)
                {
                    // create the field if it did't exist in the old table
                    if (oldTable.FindField(field.Name)==null)
                    {
                        CreateColumn(table, field);
                    }
                }
            }
        }

        #endregion

        #region Protected Helper Methods

        /// <summary>
        /// Creates the table in the data source.
        /// </summary>
        /// <param name="table">The table.</param>
        protected virtual void CreateTable(ITable table)
        {
            Debug.Assert(table != null);

            // create enumerator and check for first field
            IEnumerator<IField> fieldEnumerator = table.GetEnumerator();
            bool hasNext = fieldEnumerator.MoveNext();
            if(!hasNext)
                throw new ArgumentException("The table must contain at least one field.");

            // create query
            StringBuilder createTableQuery = new StringBuilder();
            createTableQuery.AppendFormat("CREATE TABLE [{0}] (", SqlHelper.EscapeString(table.Name));

            // add fields
            while (hasNext)
            {
                // add field name and type
                IField field = fieldEnumerator.Current;
                createTableQuery.Append('[').Append(field.Name).Append(']');
                createTableQuery.Append(' ').Append(GetDatabaseType(field));

                // append comma if a following field exists
                hasNext = fieldEnumerator.MoveNext();
                if (hasNext)
                {
                    createTableQuery.Append(',');
                }
            }

            // close CREATE TABLE x (...) bracket
            createTableQuery.Append(')');

            // execute query
            try
            {
                SqlHelper.ExecuteNonQuery(createTableQuery.ToString());
            }
            catch (Exception executeException)
            {
                throw new UmbracoException(String.Format("Could not create table '{0}'.", table), executeException);
            }
        }

        /// <summary>
        /// Creates a new column in the table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="field">The field used to create the column.</param>
        protected virtual void CreateColumn(ITable table, IField field)
        {
            Debug.Assert(table != null && field != null);

            StringBuilder addColumnQuery = new StringBuilder();
            addColumnQuery.AppendFormat("ALTER TABLE [{0}] ADD [{1}] {2}",
                                        SqlHelper.EscapeString(table.Name),
                                        SqlHelper.EscapeString(field.Name),
                                        SqlHelper.EscapeString(GetDatabaseType(field)));
            try
            {
                SqlHelper.ExecuteNonQuery(addColumnQuery.ToString());
            }
            catch (Exception executeException)
            {
                throw new UmbracoException(String.Format("Could not create column '{0}' in table '{1}'.",
                                                         field, table.Name), executeException);
            }
        }

        /// <summary>
        /// Gets the .Net type corresponding to the specified database data type.
        /// </summary>
        /// <param name="dataTypeReader">The data type reader.</param>
        /// <returns>The .Net type</returns>
        protected virtual Type GetType(IRecordsReader dataTypeReader)
        {
            string typeName = dataTypeReader.GetString("DataType");

            switch (typeName)
            {
                case "bit":                 return typeof(bool);
                case "tinyint":             return typeof(byte);
                case "datetime":            return typeof(DateTime);
                // TODO: return different decimal type according to field precision
                case "decimal":             return typeof(decimal);
                case "uniqueidentifier":    return typeof(Guid);
                case "smallint":            return typeof(Int16);
                case "int":                 return typeof(Int32);
                case "bigint":              return typeof(Int64);
                case "nvarchar":            return typeof(string);
                default:
                    throw new ArgumentException(String.Format("Cannot convert database type '{0}' to a .Net type.",
                                                              typeName));
            }
        }

        /// <summary>
        /// Gets the database type corresponding to the field, complete with field properties.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The database type.</returns>
        protected virtual string GetDatabaseType(IField field)
        {
            StringBuilder fieldBuilder = new StringBuilder();

            fieldBuilder.Append(GetDatabaseTypeName(field));
            fieldBuilder.Append(field.HasProperty(FieldProperties.Identity) ? " IDENTITY(1,1)" : String.Empty);
            fieldBuilder.Append(field.HasProperty(FieldProperties.NotNull) ? " NOT NULL" : " NULL");

            return fieldBuilder.ToString();
        }

        /// <summary>
        /// Gets the name of the database type, without field properties.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        protected virtual string GetDatabaseTypeName(IField field)
        {
            switch (field.DataType.FullName)
            {
                case "System.Boolean":  return "bit";
                case "System.Byte":     return "tinyint";
                case "System.DateTime": return "datetime";
                case "System.Decimal":  return "decimal(28)";
                case "System.Double":   return "decimal(15)";
                case "System.Single":   return "decimal(7)";
                case "System.Guid":     return "uniqueidentifier";
                case "System.Int16":    return "smallint";
                case "System.Int32":    return "int";
                case "System.Int64":    return "bigint";;
                case "System.String":
                    string type = "nvarchar";
                    return field.Size == 0 ? type : String.Format("{0}({1})", type, field.Size);
                default:
                    throw new ArgumentException(String.Format("Cannot convert '{0}' to a database type.", field));
            }
        }

        #endregion
    }
}
