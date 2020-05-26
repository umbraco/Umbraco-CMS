using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Persistence
{
    /// <summary>
    ///  Unit tests for <see cref="BulkDataReader"/>.
    /// </summary>
    /// <remarks>
    /// Borrowed from Microsoft:
    /// See: https://blogs.msdn.microsoft.com/anthonybloesch/2013/01/23/bulk-loading-data-with-idatareader-and-sqlbulkcopy/
    /// </remarks>
    [TestFixture]
    public class BulkDataReaderTest
    {

        #region Test constants

        /// <summary>
        /// The <see cref="BulkDataReaderSubclass"/> schema name.
        /// </summary>
        private const string TestSchemaName = "TestSchema";

        /// <summary>
        /// The <see cref="BulkDataReaderSubclass"/> table name.
        /// </summary>
        private const string TestTableName = "TestTable";

        /// <summary>
        /// The test UDT schema name.
        /// </summary>
        private const string TestUdtSchemaName = "UdtSchema";

        /// <summary>
        /// The test UDT name.
        /// </summary>
        private const string TestUdtName = "TestUdt";

        /// <summary>
        /// The test XML schema collection database name.
        /// </summary>
        private const string TestXmlSchemaCollectionDatabaseName = "XmlDatabase";

        /// <summary>
        /// The test XML schema collection owning schema name.
        /// </summary>
        private const string TestXmlSchemaCollectionSchemaName = "XmlSchema";

        /// <summary>
        /// The test XML schema collection name.
        /// </summary>
        private const string TestXmlSchemaCollectionName = "Xml";

        #endregion

        #region Schema tests

        /// <summary>
        /// Test that <see cref="BulkDataReader.ColumnMappings"/> is functioning correctly.
        /// </summary>
        /// <seealso cref="BulkDataReader.ColumnMappings"/>
        [Test]
        public void ColumnMappingsTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                var columnMappings = testReader.ColumnMappings;

                Assert.IsTrue(columnMappings.Count > 0);
                Assert.AreEqual(columnMappings.Count, testReader.FieldCount);

                foreach (var columnMapping in columnMappings)
                {
                    Assert.AreEqual(columnMapping.SourceColumn, columnMapping.DestinationColumn);
                }
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.GetDataTypeName(Int32)"/> is functioning correctly.
        /// </summary>
        /// <seealso cref="BulkDataReader.GetDataTypeName(Int32)"/>
        [Test]
        public void GetDataTypeNameTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.FieldCount > 0);

                for (var currentColumn = 0; currentColumn < testReader.FieldCount; currentColumn++)
                {
                    var schemaTable = testReader.GetSchemaTable();
                    Assert.IsNotNull(schemaTable);
                    Assert.AreEqual(testReader.GetDataTypeName(currentColumn), ((Type) schemaTable.Rows[currentColumn][SchemaTableColumn.DataType]).Name);
                }
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.GetFieldType(Int32)"/> is functioning correctly.
        /// </summary>
        /// <seealso cref="BulkDataReader.GetFieldType(Int32)"/>
        [Test]
        public void GetFieldTypeTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.FieldCount > 0);

                for (var currentColumn = 0; currentColumn < testReader.FieldCount; currentColumn++)
                {
                    var schemaTable = testReader.GetSchemaTable();
                    Assert.IsNotNull(schemaTable);
                    Assert.AreEqual(testReader.GetFieldType(currentColumn), schemaTable.Rows[currentColumn][SchemaTableColumn.DataType]);
                }
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.GetOrdinal(String)"/> is functioning correctly.
        /// </summary>
        /// <seealso cref="BulkDataReader.GetOrdinal(String)"/>
        [Test]
        public void GetOrdinalTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.FieldCount > 0);

                for (var currentColumn = 0; currentColumn < testReader.FieldCount; currentColumn++)
                {
                    Assert.AreEqual(testReader.GetOrdinal(testReader.GetName(currentColumn)), currentColumn);
                    Assert.AreEqual(testReader.GetOrdinal(testReader.GetName(currentColumn).ToUpperInvariant()), currentColumn);
                }
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.GetSchemaTable()"/> functions correctly.
        /// </summary>
        /// <remarks>
        /// uses <see cref="BulkDataReaderSubclass"/> to test legal schema combinations.
        /// </remarks>
        /// <seealso cref="BulkDataReader.GetSchemaTable()"/>
        [Test]
        public void GetSchemaTableTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                var schemaTable = testReader.GetSchemaTable();

                Assert.IsNotNull(schemaTable);
                Assert.IsTrue(schemaTable.Rows.Count > 0);
                Assert.AreEqual(schemaTable.Rows.Count, BulkDataReaderSubclass.ExpectedResultSet.Count);
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for null column names.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowNullColumnNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = null;
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.BigInt;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for empty column names.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowEmptyColumnNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = string.Empty;
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.BigInt;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for nonpositive column sizes.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowNonpositiveColumnSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 0;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.NVarChar;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for nonpositive numeric precision.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowNonpositiveNumericPrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 0;
                testReader.NumericScale = 0;
                testReader.ProviderType = SqlDbType.Decimal;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for negative numeric scale.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowNegativeNumericScaleTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 5;
                testReader.NumericScale = -1;
                testReader.ProviderType = SqlDbType.Decimal;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for binary column without a column size.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowBinaryWithoutSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Binary;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for binary column with a column size that is too large (>8000).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowBinaryWithTooLargeSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 8001;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Binary;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for char column without a column size.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowCharWithoutSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Char;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for char column with a column size that is too large (>8000).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowCharWithTooLargeSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 8001;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Char;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for decimal column without a column precision.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowDecimalWithoutPrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = 5;
                testReader.ProviderType = SqlDbType.Decimal;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for decimal column without a column scale.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowDecimalWithoutScaleTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 20;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Decimal;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for decimal column with a column precision that is too large (>38).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowDecimalWithTooLargePrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 39;
                testReader.NumericScale = 5;
                testReader.ProviderType = SqlDbType.Decimal;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for decimal column with a column scale that is larger than the column precision.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowDecimalWithTooLargeScaleTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 20;
                testReader.NumericScale = 21;
                testReader.ProviderType = SqlDbType.Decimal;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for datetime2 column with a column size that has a precision that is too large (>7).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowDateTime2WithTooLargePrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 8;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.DateTime2;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for datetimeoffset column with a column size that has a precision that is too large (>7).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowDateTimeOffsetWithTooLargePrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 8;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.DateTimeOffset;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for nchar column without a precision.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowFloatWithoutPrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Float;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for float column with a column precision that is too large (>53).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowFloatWithTooLargePrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 54;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Float;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for nchar column without a column size.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowNCharWithoutSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.NChar;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for nchar column with a column size that is too large (>4000).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowNCharWithTooLargeSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 4001;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.NChar;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for nvarchar column with a column size that is too large (>4000).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowNVarCharWithTooLargeSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 4001;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.NVarChar;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for time column with a column precision that is too large (>7).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowTimeWithTooLargePrecisionTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 8;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Time;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for missing UDT schema name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowUdtMissingSchemaNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Udt;
                testReader.UdtSchema = null;
                testReader.UdtType = "Type";
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for empty UDT schema name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowUdtEmptySchemaNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Udt;
                testReader.UdtSchema = string.Empty;
                testReader.UdtType = "Type";
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for missing UDT name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowUdtMissingNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Udt;
                testReader.UdtSchema = "Schema";
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for empty UDT name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowUdtEmptyNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Udt;
                testReader.UdtSchema = "Schema";
                testReader.UdtType = string.Empty;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for varbinary column with a column size that is too large (>8000).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowVarBinaryWithTooLargeSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 8001;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.VarBinary;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for varchar column with a column size that is too large (>8000).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowVarCharWithTooLargeSizeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 8001;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.VarChar;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for null xml collection name but with a name for the database.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowXmlNullNameWithDatabaseNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Xml;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = "Database";
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for null xml collection name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowXmlNullNameWithOwningSchemaNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Xml;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = "Schema";
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for empty xml collection database name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowXmlEmptyDatabaseNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Xml;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = string.Empty;
                testReader.XmlSchemaCollectionOwningSchema = "Schema";
                testReader.XmlSchemaCollectionName = "Xml";

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for empty xml collection owning schema name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowXmlEmptyOwningSchemaNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Xml;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = "Database";
                testReader.XmlSchemaCollectionOwningSchema = string.Empty;
                testReader.XmlSchemaCollectionName = "Xml";

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentException"/> for empty xml collection name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowXmlEmptyNameTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Xml;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = "Database";
                testReader.XmlSchemaCollectionOwningSchema = "Schema";
                testReader.XmlSchemaCollectionName = string.Empty;

                Assert.Throws<ArgumentException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for a structured column (which is illegal).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowStructuredTypeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Structured;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> for a timestamp column (which is illegal).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowTimestampTypeTest()
        {
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.ProviderType = SqlDbType.Timestamp;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetSchemaTable();
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        /// throws an <see cref="ArgumentException"/> for a column with an unallowed optional column set.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSchemaTest"/> to test the illegal schema combination.
        /// </remarks>
        /// <seealso cref="BulkDataReader.AddSchemaTableRow(String,Nullable{Int32},Nullable{Int16},Nullable{Int16},Boolean,Boolean,Boolean,SqlDbType,String,String,String,String,String)"/>
        [Test]
        public void AddSchemaTableRowUnallowedOptionalColumnTest()
        {

            // Column size set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = 5;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Bit, SqlDbType.Date, SqlDbType.DateTime, SqlDbType.DateTime2,
                                                                   SqlDbType.DateTimeOffset, SqlDbType.Image, SqlDbType.Int, SqlDbType.Money, SqlDbType.Real,
                                                                   SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney, SqlDbType.Structured, SqlDbType.Text,
                                                                   SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt, SqlDbType.Udt, SqlDbType.UniqueIdentifier,
                                                                   SqlDbType.Variant, SqlDbType.Xml })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // Numeric precision set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 5;
                testReader.NumericScale = null;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.Image, SqlDbType.Int, SqlDbType.Money, SqlDbType.NChar,
                                                                   SqlDbType.NText, SqlDbType.NVarChar, SqlDbType.Real, SqlDbType.SmallDateTime, SqlDbType.SmallInt,
                                                                   SqlDbType.SmallMoney, SqlDbType.Structured, SqlDbType.Text, SqlDbType.Timestamp, SqlDbType.TinyInt,
                                                                   SqlDbType.Udt, SqlDbType.UniqueIdentifier, SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant,
                                                                   SqlDbType.Xml })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // Numeric scale set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 5;
                testReader.NumericScale = 3;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset, SqlDbType.Image, SqlDbType.Int,
                                                                   SqlDbType.Money, SqlDbType.NChar, SqlDbType.NText, SqlDbType.NVarChar, SqlDbType.Real,
                                                                   SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney, SqlDbType.Structured, SqlDbType.Text,
                                                                   SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt, SqlDbType.Udt, SqlDbType.UniqueIdentifier,
                                                                   SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant, SqlDbType.Xml })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // Numeric scale set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = 5;
                testReader.NumericScale = 3;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset, SqlDbType.Image, SqlDbType.Int,
                                                                   SqlDbType.Money, SqlDbType.NChar, SqlDbType.NText, SqlDbType.NVarChar, SqlDbType.Real,
                                                                   SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney, SqlDbType.Structured, SqlDbType.Text,
                                                                   SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt, SqlDbType.Udt, SqlDbType.UniqueIdentifier,
                                                                   SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant, SqlDbType.Xml })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // UDT type name set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.UdtSchema = null;
                testReader.UdtType = "Type";
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset, SqlDbType.Decimal, SqlDbType.Float,
                                                                   SqlDbType.Image, SqlDbType.Int, SqlDbType.Money, SqlDbType.NChar, SqlDbType.NText,
                                                                   SqlDbType.NVarChar, SqlDbType.Real, SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney,
                                                                   SqlDbType.Structured, SqlDbType.Text, SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt,
                                                                   SqlDbType.UniqueIdentifier, SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant, SqlDbType.Xml })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // UDT schema and type name set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.UdtSchema = "Schema";
                testReader.UdtType = "Type";
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = null;

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset, SqlDbType.Decimal, SqlDbType.Float,
                                                                   SqlDbType.Image, SqlDbType.Int, SqlDbType.Money, SqlDbType.NChar, SqlDbType.NText,
                                                                   SqlDbType.NVarChar, SqlDbType.Real, SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney,
                                                                   SqlDbType.Structured, SqlDbType.Text, SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt,
                                                                   SqlDbType.UniqueIdentifier, SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant, SqlDbType.Xml })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // XML type name set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = null;
                testReader.XmlSchemaCollectionName = "Name";

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset, SqlDbType.Decimal, SqlDbType.Float,
                                                                   SqlDbType.Image, SqlDbType.Int, SqlDbType.Money, SqlDbType.NChar, SqlDbType.NText,
                                                                   SqlDbType.NVarChar, SqlDbType.Real, SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney,
                                                                   SqlDbType.Structured, SqlDbType.Text, SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt,
                                                                   SqlDbType.UniqueIdentifier, SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant, SqlDbType.Udt })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // XML owning schema and type name set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = null;
                testReader.XmlSchemaCollectionOwningSchema = "Schema";
                testReader.XmlSchemaCollectionName = "Name";

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset, SqlDbType.Decimal, SqlDbType.Float,
                                                                   SqlDbType.Image, SqlDbType.Int, SqlDbType.Money, SqlDbType.NChar, SqlDbType.NText,
                                                                   SqlDbType.NVarChar, SqlDbType.Real, SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney,
                                                                   SqlDbType.Structured, SqlDbType.Text, SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt,
                                                                   SqlDbType.UniqueIdentifier, SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant, SqlDbType.Udt })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // XML database, owning schema and type name set
            using (var testReader = new BulkDataReaderSchemaTest())
            {
                testReader.AllowDBNull = false;
                testReader.ColumnName = "Name";
                testReader.ColumnSize = null;
                testReader.IsKey = false;
                testReader.IsUnique = false;
                testReader.NumericPrecision = null;
                testReader.NumericScale = null;
                testReader.UdtSchema = null;
                testReader.UdtType = null;
                testReader.XmlSchemaCollectionDatabase = "Database";
                testReader.XmlSchemaCollectionOwningSchema = "Schema";
                testReader.XmlSchemaCollectionName = "Name";

                foreach (var dbtype in new List<SqlDbType> { SqlDbType.BigInt, SqlDbType.Binary, SqlDbType.Bit, SqlDbType.Char, SqlDbType.Date,
                                                                   SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset, SqlDbType.Decimal, SqlDbType.Float,
                                                                   SqlDbType.Image, SqlDbType.Int, SqlDbType.Money, SqlDbType.NChar, SqlDbType.NText,
                                                                   SqlDbType.NVarChar, SqlDbType.Real, SqlDbType.SmallDateTime, SqlDbType.SmallInt, SqlDbType.SmallMoney,
                                                                   SqlDbType.Structured, SqlDbType.Text, SqlDbType.Time, SqlDbType.Timestamp, SqlDbType.TinyInt,
                                                                   SqlDbType.UniqueIdentifier, SqlDbType.VarBinary, SqlDbType.VarChar, SqlDbType.Variant, SqlDbType.Udt })
                {
                    testReader.ProviderType = dbtype;

                    try
                    {
                        var unused = testReader.GetSchemaTable();

                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        #endregion;

        #region Rowset tests

        /// <summary>
        /// Test that <see cref="BulkDataReader.Close()"/> is functioning correctly.
        /// </summary>
        /// <seealso cref="BulkDataReader.Close()"/>
        [Test]
        public void CloseTest()
        {
            var testReader = new BulkDataReaderSubclass();

            testReader.Close();

            Assert.IsTrue(testReader.IsClosed);
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.Depth"/> is functioning correctly.
        /// </summary>
        /// <remarks>
        /// Because nested row sets are not supported, this should always return 0;
        /// </remarks>
        /// <seealso cref="BulkDataReader.Depth"/>
        [Test]
        public void DepthTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                Assert.AreEqual(0, testReader.Depth);
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.GetData(Int32)"/> is functioning correctly.
        /// </summary>
        /// <remarks>
        /// Because nested row sets are not supported, this should always return null;
        /// </remarks>
        /// <seealso cref="BulkDataReader.GetData(Int32)"/>
        [Test]
        public void GetDataTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                Assert.IsTrue(testReader.FieldCount > 0);

                Assert.IsNull(testReader.GetData(0));
            }
        }

        /// <summary>
        /// Test <see cref="BulkDataReader.GetValue(Int32)"/> and related functions.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSubclass"/> to test legal schema combinations.
        /// </remarks>
        [Test]
        public void GetValueTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                // this[int]
                for (var column = 0; column < BulkDataReaderSubclass.ExpectedResultSet.Count; column++)
                {
                    Assert.AreEqual(testReader[column], BulkDataReaderSubclass.ExpectedResultSet[column]);
                }

                // this[string]
                for (var column = 0; column < BulkDataReaderSubclass.ExpectedResultSet.Count; column++)
                {
                    Assert.AreEqual(testReader[testReader.GetName(column)], BulkDataReaderSubclass.ExpectedResultSet[column]);

                    Assert.AreEqual(testReader[testReader.GetName(column).ToUpperInvariant()], BulkDataReaderSubclass.ExpectedResultSet[column]);
                }

                // GetValues
                {
                    var values = new object[BulkDataReaderSubclass.ExpectedResultSet.Count];
                    var expectedValues = new object[BulkDataReaderSubclass.ExpectedResultSet.Count];

                    Assert.AreEqual(testReader.GetValues(values), values.Length);

                    BulkDataReaderSubclass.ExpectedResultSet.CopyTo(expectedValues, 0);

                    Assert.IsTrue(ArraysMatch(values, expectedValues));
                }

                // Typed getters
                {
                    var currentColumn = 0;

                    Assert.AreEqual(testReader.GetInt64(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    {
                        var expectedResult = (byte[])BulkDataReaderSubclass.ExpectedResultSet[currentColumn];
                        var expectedLength = expectedResult.Length;
                        var buffer = new byte[expectedLength];

                        Assert.AreEqual(testReader.GetBytes(currentColumn, 0, buffer, 0, expectedLength), expectedLength);

                        Assert.IsTrue(ArraysMatch(buffer, expectedResult));
                    }
                    currentColumn++;

                    Assert.AreEqual(testReader.GetBoolean(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.IsDBNull(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn] == null);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetChar(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetChar(currentColumn), ((char[])BulkDataReaderSubclass.ExpectedResultSet[currentColumn])[0]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetChar(currentColumn), ((string)BulkDataReaderSubclass.ExpectedResultSet[currentColumn])[0]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);

                    {
                        var expectedResult = ((string)BulkDataReaderSubclass.ExpectedResultSet[currentColumn]).ToCharArray();
                        var expectedLength = expectedResult.Length;
                        var buffer = new char[expectedLength];

                        Assert.AreEqual(testReader.GetChars(currentColumn, 0, buffer, 0, expectedLength), expectedLength);

                        Assert.IsTrue(ArraysMatch(buffer, expectedResult));
                    }

                    currentColumn++;

                    Assert.AreEqual(testReader.GetDateTime(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDateTime(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDateTime(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDateTime(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDateTimeOffset(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDateTimeOffset(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDecimal(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDouble(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    {
                        byte[] expectedResult = (byte[])BulkDataReaderSubclass.ExpectedResultSet[currentColumn];
                        int expectedLength = expectedResult.Length;
                        byte[] buffer = new byte[expectedLength];

                        Assert.AreEqual(testReader.GetBytes(currentColumn, 0, buffer, 0, expectedLength), expectedLength);

                        Assert.IsTrue(ArraysMatch(buffer, expectedResult));
                    }
                    currentColumn++;

                    Assert.AreEqual(testReader.GetInt32(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDecimal(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetFloat(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDateTime(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetInt16(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetDecimal(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetTimeSpan(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetTimeSpan(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetByte(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetValue(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetGuid(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    {
                        var expectedResult = (byte[])BulkDataReaderSubclass.ExpectedResultSet[currentColumn];
                        var expectedLength = expectedResult.Length;
                        var buffer = new byte[expectedLength];

                        Assert.AreEqual(testReader.GetBytes(currentColumn, 0, buffer, 0, expectedLength), expectedLength);

                        Assert.IsTrue(ArraysMatch(buffer, expectedResult));
                    }
                    currentColumn++;

                    {
                        var expectedResult = (byte[])BulkDataReaderSubclass.ExpectedResultSet[currentColumn];
                        int expectedLength = expectedResult.Length;
                        byte[] buffer = new byte[expectedLength];

                        Assert.AreEqual(testReader.GetBytes(currentColumn, 0, buffer, 0, expectedLength), expectedLength);

                        Assert.IsTrue(ArraysMatch(buffer, expectedResult));
                    }
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetValue(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                    currentColumn++;

                    Assert.AreEqual(testReader.GetString(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn]);
                }
            }
        }

        /// <summary>
        /// Test <see cref="BulkDataReader.GetValue(Int32)"/> throws a <see cref="ArgumentOutOfRangeException"/> when
        /// the index is too small.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSubclass"/> to test the method.
        /// </remarks>
        /// <seealso cref="BulkDataReader.GetValue(Int32)"/>
        [Test]
        public void GetValueIndexTooSmallTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetValue(-1);
                });
            }
        }

        /// <summary>
        /// Test <see cref="BulkDataReader.GetValue(Int32)"/> throws a <see cref="ArgumentOutOfRangeException"/> when
        /// the index is too large.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSubclass"/> to test the method.
        /// </remarks>
        /// <seealso cref="BulkDataReader.GetValue(Int32)"/>
        [Test]
        public void GetValueIndexTooLargeTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetValue(testReader.FieldCount);
                });
            }
        }

        /// <summary>
        /// Test <see cref="BulkDataReader.GetData(Int32)"/> throws a <see cref="ArgumentOutOfRangeException"/> when
        /// the index is too small.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSubclass"/> to test the method.
        /// </remarks>
        /// <seealso cref="BulkDataReader.GetValue(Int32)"/>
        [Test]
        public void GetDataIndexTooSmallTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetData(-1);
                });
            }
        }

        /// <summary>
        /// Test <see cref="BulkDataReader.GetValue(Int32)"/> throws a <see cref="ArgumentOutOfRangeException"/> when
        /// the index is too large.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="BulkDataReaderSubclass"/> to test the method.
        /// </remarks>
        [Test]
        public void GetDataIndexTooLargeTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var unused = testReader.GetData(testReader.FieldCount);
                });
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.IsDBNull(Int32)"/> functions correctly.
        /// </summary>
        /// <seealso cref="BulkDataReader.IsDBNull(Int32)"/>
        [Test]
        public void IsDbNullTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                for (var currentColumn = 0; currentColumn < testReader.FieldCount; currentColumn++)
                {
                    Assert.AreEqual(testReader.IsDBNull(currentColumn), BulkDataReaderSubclass.ExpectedResultSet[currentColumn] == null);
                }
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.NextResult()"/> is functioning correctly.
        /// </summary>
        /// <remarks>
        /// Because this is a single row set, this should always return false;
        /// </remarks>
        /// <seealso cref="BulkDataReader.NextResult()"/>
        [Test]
        public void NextResultTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsFalse(testReader.NextResult());
            }
        }

        /// <summary>
        /// Test that <see cref="BulkDataReader.RecordsAffected"/> is functioning correctly.
        /// </summary>
        /// <remarks>
        /// Because this row set represents a data source, this should always return -1;
        /// </remarks>
        /// <seealso cref="BulkDataReader.RecordsAffected"/>
        [Test]
        public void RecordsAffectedTest()
        {
            using (var testReader = new BulkDataReaderSubclass())
            {
                Assert.IsTrue(testReader.Read());

                Assert.AreEqual(-1, testReader.RecordsAffected);
            }
        }

        #endregion

        #region Test IDisposable

        /// <summary>
        /// Test that the <see cref="BulkDataReader"/> <see cref="IDisposable"/> interface is functioning correctly.
        /// </summary>
        /// <seealso cref="BulkDataReader.Dispose()"/>
        /// <seealso cref="IDisposable"/>
        [Test]
        public void DisposableTest()
        {
            // Test the Dispose method
            {
                var testReader = new BulkDataReaderSubclass();

                testReader.Dispose();

                Assert.IsTrue(testReader.IsClosed);
            }

            // Test the finalizer method
            {
                var testReader = new BulkDataReaderSubclass();

                testReader = null;

                GC.Collect();

                GC.WaitForPendingFinalizers();
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Do the two <typeparamref name="ElementType"/> arrays match exactly?
        /// </summary>
        /// <typeparam name="ElementType">
        /// The type of the array elements.
        /// </typeparam>
        /// <param name="left">
        /// The first <typeparamref name="ElementType"/> array.
        /// </param>
        /// <param name="right">
        /// The second <typeparamref name="ElementType"/> array.
        /// </param>
        /// <returns>
        /// True if the <typeparamref name="ElementType"/> arrays have the same length and contents.
        /// </returns>
        private static bool ArraysMatch<ElementType>(ElementType[] left,
                                                     ElementType[] right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            else if (right == null)
            {
                throw new ArgumentNullException("left");
            }

            bool result = true;

            if (left.Length != right.Length)
            {
                result = false;
            }
            else
            {
                for (int currentIndex = 0; currentIndex < left.Length; currentIndex++)
                {
                    result &= object.Equals(left[currentIndex], right[currentIndex]);
                }
            }

            return result;
        }

        #endregion

        #region Test stubs

        /// <summary>
        /// A subclass of <see cref="BulkDataReader"/> used for testing its utility functions.
        /// </summary>
        private class BulkDataReaderSubclass : BulkDataReader
        {

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            public BulkDataReaderSubclass()
            {
            }

            #endregion

            #region BulkDataReader

            /// <summary>
            /// See <see cref="BulkDataReader.SchemaName"/>.
            /// </summary>
            /// <remarks>
            /// Returns <see cref="BulkDataReaderTest.TestSchemaName"/>.
            /// </remarks>
            protected override string SchemaName
            {
                get { return BulkDataReaderTest.TestSchemaName; }
            }

            /// <summary>
            /// See <see cref="BulkDataReader.TableName"/>.
            /// </summary>
            /// <remarks>
            /// Returns <see cref="BulkDataReaderTest.TestTableName"/>.
            /// </remarks>
            protected override string TableName
            {
                get { return BulkDataReaderTest.TestTableName; }
            }

            /// <summary>
            /// See <see cref="BulkDataReader.AddSchemaTableRows()"/>
            /// </summary>
            /// <remarks>
            /// Creates a schema row for the various <see cref="SqlDbType"/> values.
            /// </remarks>
            protected override void AddSchemaTableRows()
            {
                AddSchemaTableRow("BigInt", null, null, null, true, false, false, SqlDbType.BigInt, null, null, null, null, null);
                AddSchemaTableRow("Binary_20", 20, null, null, false, true, false, SqlDbType.Binary, null, null, null, null, null);
                AddSchemaTableRow("Bit", null, null, null, false, false, true, SqlDbType.Bit, null, null, null, null, null);
                AddSchemaTableRow("Bit_null", null, null, null, false, false, true, SqlDbType.Bit, null, null, null, null, null);
                AddSchemaTableRow("Char_Char", 1, null, null, false, false, false, SqlDbType.Char, null, null, null, null, null);
                AddSchemaTableRow("Char_Char_Array", 1, null, null, false, false, false, SqlDbType.Char, null, null, null, null, null);
                AddSchemaTableRow("Char_String", 1, null, null, false, false, false, SqlDbType.Char, null, null, null, null, null);
                AddSchemaTableRow("Char_20_String", 20, null, null, false, false, false, SqlDbType.Char, null, null, null, null, null);
                AddSchemaTableRow("Date", null, null, null, false, false, false, SqlDbType.Date, null, null, null, null, null);
                AddSchemaTableRow("DateTime", null, null, null, false, false, false, SqlDbType.DateTime, null, null, null, null, null);
                AddSchemaTableRow("DateTime2", null, null, null, false, false, false, SqlDbType.DateTime2, null, null, null, null, null);
                AddSchemaTableRow("DateTime2_5", null, 5, null, false, false, false, SqlDbType.DateTime2, null, null, null, null, null);
                AddSchemaTableRow("DateTimeOffset", null, null, null, false, false, false, SqlDbType.DateTimeOffset, null, null, null, null, null);
                AddSchemaTableRow("DateTimeOffset_5", null, 5, null, false, false, false, SqlDbType.DateTimeOffset, null, null, null, null, null);
                AddSchemaTableRow("Decimal_20_10", null, 20, 10, false, false, false, SqlDbType.Decimal, null, null, null, null, null);
                AddSchemaTableRow("Float_50", null, 50, null, false, false, false, SqlDbType.Float, null, null, null, null, null);
                AddSchemaTableRow("Image", null, null, null, false, false, false, SqlDbType.Image, null, null, null, null, null);
                AddSchemaTableRow("Int", null, null, null, false, false, false, SqlDbType.Int, null, null, null, null, null);
                AddSchemaTableRow("Money", null, null, null, false, false, false, SqlDbType.Money, null, null, null, null, null);
                AddSchemaTableRow("NChar_20", 20, null, null, false, false, false, SqlDbType.NChar, null, null, null, null, null);
                AddSchemaTableRow("NText", null, null, null, false, false, false, SqlDbType.NText, null, null, null, null, null);
                AddSchemaTableRow("NVarChar_20", 20, null, null, false, false, false, SqlDbType.NVarChar, null, null, null, null, null);
                AddSchemaTableRow("NVarChar_Max", null, null, null, false, false, false, SqlDbType.NVarChar, null, null, null, null, null);
                AddSchemaTableRow("Real", null, null, null, false, false, false, SqlDbType.Real, null, null, null, null, null);
                AddSchemaTableRow("SmallDateTime", null, null, null, false, false, false, SqlDbType.SmallDateTime, null, null, null, null, null);
                AddSchemaTableRow("SmallInt", null, null, null, false, false, false, SqlDbType.SmallInt, null, null, null, null, null);
                AddSchemaTableRow("SmallMoney", null, null, null, false, false, false, SqlDbType.SmallMoney, null, null, null, null, null);
                AddSchemaTableRow("Text", null, null, null, false, false, false, SqlDbType.Text, null, null, null, null, null);
                AddSchemaTableRow("Time", null, null, null, false, false, false, SqlDbType.Time, null, null, null, null, null);
                AddSchemaTableRow("Time_5", null, 5, null, false, false, false, SqlDbType.Time, null, null, null, null, null);
                AddSchemaTableRow("TinyInt", null, null, null, false, false, false, SqlDbType.TinyInt, null, null, null, null, null);
                AddSchemaTableRow("Udt", null, null, null, false, false, false, SqlDbType.Udt, BulkDataReaderTest.TestUdtSchemaName, BulkDataReaderTest.TestUdtName, null, null, null);
                AddSchemaTableRow("UniqueIdentifier", null, null, null, false, false, false, SqlDbType.UniqueIdentifier, null, null, null, null, null);
                AddSchemaTableRow("VarBinary_20", 20, null, null, false, false, false, SqlDbType.VarBinary, null, null, null, null, null);
                AddSchemaTableRow("VarBinary_Max", null, null, null, false, false, false, SqlDbType.VarBinary, null, null, null, null, null);
                AddSchemaTableRow("VarChar_20", 20, null, null, false, false, false, SqlDbType.VarChar, null, null, null, null, null);
                AddSchemaTableRow("VarChar_Max", null, null, null, false, false, false, SqlDbType.VarChar, null, null, null, null, null);
                AddSchemaTableRow("Variant", null, null, null, false, false, false, SqlDbType.Variant, null, null, null, null, null);
                AddSchemaTableRow("Xml_Database", null, null, null, false, false, false, SqlDbType.Xml, null, null, BulkDataReaderTest.TestXmlSchemaCollectionDatabaseName, BulkDataReaderTest.TestXmlSchemaCollectionSchemaName, BulkDataReaderTest.TestXmlSchemaCollectionName);
                AddSchemaTableRow("Xml_Database_XML", null, null, null, false, false, false, SqlDbType.Xml, null, null, BulkDataReaderTest.TestXmlSchemaCollectionDatabaseName, BulkDataReaderTest.TestXmlSchemaCollectionSchemaName, BulkDataReaderTest.TestXmlSchemaCollectionName);
                AddSchemaTableRow("Xml_Schema", null, null, null, false, false, false, SqlDbType.Xml, null, null, null, BulkDataReaderTest.TestXmlSchemaCollectionSchemaName, BulkDataReaderTest.TestXmlSchemaCollectionName);
                AddSchemaTableRow("Xml_Xml", null, null, null, false, false, false, SqlDbType.Xml, null, null, null, null, BulkDataReaderTest.TestXmlSchemaCollectionName);
                AddSchemaTableRow("Xml", null, null, null, false, false, false, SqlDbType.Xml, null, null, null, null, null);
            }

            /// <summary>
            /// The result set returned by the <see cref="IDataReader"/>.
            /// </summary>
            public static readonly ReadOnlyCollection<object> ExpectedResultSet = new ReadOnlyCollection<object>(new List<object>
            {
                (long)10,
                new byte[20],
                true,
                null,
                'c',
                new char[] { 'c' },
                "c",
                "char 20",
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                (decimal)10.5,
                (double)10.5,
                new byte[20],
                (int)10,
                (decimal)10.5,
                "nchar 20",
                "ntext",
                "nvarchar 20",
                "nvarchar max",
                (float)10.5,
                DateTime.UtcNow,
                (short)10,
                (decimal)10.5,
                "text",
                DateTime.UtcNow.TimeOfDay,
                DateTime.UtcNow.TimeOfDay,
                (byte)10,
                new object(),
                Guid.NewGuid(),
                new byte[20],
                new byte[20],
                "varchar 20",
                "varchar max",
                (int)10,
                @"<?xml version=string.Empty1.0string.Empty encoding=string.Emptyutf-16string.Empty?><body/>",
                @"<?xml version=string.Empty1.0string.Empty encoding=string.Emptyutf-16string.Empty?><body/>",
                @"<?xml version=string.Empty1.0string.Empty encoding=string.Emptyutf-16string.Empty?><body/>",
                @"<?xml version=string.Empty1.0string.Empty encoding=string.Emptyutf-16string.Empty?><body/>",
                @"<?xml version=string.Empty1.0string.Empty encoding=string.Emptyutf-16string.Empty?><body/>"
            });

            /// <summary>
            /// See <see cref="BulkDataReader.GetValue(Int32)"/>
            /// </summary>
            /// <param name="i">
            /// The zero-based column ordinal.
            /// </param>
            /// <returns>
            /// The value of the column in <see cref="ExpectedResultSet"/>.
            /// </returns>
            /// <seealso cref="BulkDataReader.GetValue(Int32)"/>
            public override object GetValue(int i)
            {
                return BulkDataReaderSubclass.ExpectedResultSet[i];
            }

            /// <summary>
            /// The number of rows read.
            /// </summary>
            private int readCount = 0;

            /// <summary>
            /// See <see cref="BulkDataReader.Read()"/>
            /// </summary>
            /// <returns>
            /// True if there are more rows; otherwise, false.
            /// </returns>
            /// <seealso cref="BulkDataReader.Read()"/>
            public override bool Read()
            {
                return readCount++ < 1;
            }

            #endregion

        }

        private class BulkDataReaderSchemaTest : BulkDataReader
        {

            #region Properties

            /// <summary>
            /// Is the column nullable (i.e. optional)?
            /// </summary>
            public bool AllowDBNull { get; set; }

            /// <summary>
            /// The name of the column.
            /// </summary>
            public string ColumnName { get; set; }

            /// <summary>
            /// The size of the column which may be null if not applicable.
            /// </summary>
            public int? ColumnSize { get; set; }

            /// <summary>
            /// Is the column part of the primary key?
            /// </summary>
            public bool IsKey { get; set; }

            /// <summary>
            /// Are the column values unique (i.e. never duplicated)?
            /// </summary>
            public bool IsUnique { get; set; }

            /// <summary>
            /// The precision of the column which may be null if not applicable.
            /// </summary>
            public short? NumericPrecision { get; set; }

            /// <summary>
            /// The scale of the column which may be null if not applicable.
            /// </summary>
            public short? NumericScale { get; set; }

            /// <summary>
            /// The corresponding <see cref="SqlDbType"/>.
            /// </summary>
            public SqlDbType ProviderType { get; set; }

            /// <summary>
            /// The schema name of the UDT.
            /// </summary>
            public string UdtSchema { get; set; }

            /// <summary>
            /// The type name of the UDT.
            /// </summary>
            public string UdtType { get; set; }

            /// <summary>
            /// For XML columns the schema collection's database name. Otherwise, null.
            /// </summary>
            public string XmlSchemaCollectionDatabase { get; set; }

            /// <summary>
            /// For XML columns the schema collection's name. Otherwise, null.
            /// </summary>
            public string XmlSchemaCollectionName { get; set; }

            /// <summary>
            /// For XML columns the schema collection's schema name. Otherwise, null.
            /// </summary>
            public string XmlSchemaCollectionOwningSchema { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            public BulkDataReaderSchemaTest()
            {
            }

            #endregion

            #region BulkDataReader

            /// <summary>
            /// See <see cref="BulkDataReader.SchemaName"/>.
            /// </summary>
            /// <remarks>
            /// Returns <see cref="BulkDataReaderTest.TestSchemaName"/>.
            /// </remarks>
            protected override string SchemaName
            {
                get { return BulkDataReaderTest.TestSchemaName; }
            }

            /// <summary>
            /// See <see cref="BulkDataReader.TableName"/>.
            /// </summary>
            /// <remarks>
            /// Returns <see cref="BulkDataReaderTest.TestTableName"/>.
            /// </remarks>
            protected override string TableName
            {
                get { return BulkDataReaderTest.TestTableName; }
            }

            /// <summary>
            /// See <see cref="BulkDataReader.AddSchemaTableRows()"/>
            /// </summary>
            /// <remarks>
            /// Creates a schema row for the various <see cref="SqlDbType"/> values.
            /// </remarks>
            protected override void AddSchemaTableRows()
            {
                AddSchemaTableRow(this.ColumnName,
                                  this.ColumnSize,
                                  this.NumericPrecision,
                                  this.NumericScale,
                                  this.IsUnique,
                                  this.IsKey,
                                  this.AllowDBNull,
                                  this.ProviderType,
                                  this.UdtSchema,
                                  this.UdtType,
                                  this.XmlSchemaCollectionDatabase,
                                  this.XmlSchemaCollectionOwningSchema,
                                  this.XmlSchemaCollectionName);
            }

            /// <summary>
            /// See <see cref="BulkDataReader.GetValue(Int32)"/>
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The test stub is only for testing schema functionality and behaves as if it has no rows.
            /// </exception>
            /// <param name="i">
            /// The zero-based column ordinal.
            /// </param>
            /// <returns>
            /// Never returns.
            /// </returns>
            /// <seealso cref="BulkDataReader.GetValue(Int32)"/>
            public override object GetValue(int i)
            {
                throw new InvalidOperationException("No data.");
            }


            /// <summary>
            /// See <see cref="BulkDataReader.Read()"/>
            /// </summary>
            /// <returns>
            /// False.
            /// </returns>
            /// <seealso cref="BulkDataReader.Read()"/>
            public override bool Read()
            {
                return false;
            }

            #endregion

        }

        #endregion
    }
}
