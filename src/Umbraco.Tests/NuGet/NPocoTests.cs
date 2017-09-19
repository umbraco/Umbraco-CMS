using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Testing;
using LightInject;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.NuGet
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class NPocoTests : UmbracoTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            using (var scope = ScopeProvider.CreateScope())
            {
                InsertData(scope.Database);
            }
        }

        private IScopeProvider ScopeProvider => Container.GetInstance<IScopeProvider>();

        private void InsertData(IDatabase database)
        {
            database.Execute(@"
                CREATE TABLE zbThing1 (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL
                );");

            database.Insert(new Thing1Dto
            {
                Id = 1,
                Name = "one"
            });

            database.Insert(new Thing1Dto
            {
                Id = 2,
                Name = "two"
            });

            database.Execute(@"
                CREATE TABLE zbThing2 (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL,
                    thingId int NULL,
                );");

            database.Insert(new Thing2Dto
            {
                Id = 1,
                Name = "uno",
                ThingId = 1
            });

            database.Insert(new Thing2Dto
            {
                Id = 2,
                Name = "due",
                ThingId = 2
            });

            database.Insert(new Thing2Dto
            {
                Id = 3,
                Name = "tri",
                ThingId = 1
            });

            database.Execute(@"
                CREATE TABLE zbThingGroup (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL
                );");

            database.Insert(new ThingGroupDto
            {
                Id = 1,
                Name = "g-one"
            });

            database.Insert(new ThingGroupDto
            {
                Id = 2,
                Name = "g-two"
            });

            database.Insert(new ThingGroupDto
            {
                Id = 3,
                Name = "g-three"
            });

            database.Execute(@"
                CREATE TABLE zbThing2Group (
                    thingId int NOT NULL,
                    groupId int NOT NULL
                );");

            database.Insert(new Thing2GroupDto
            {
                ThingId = 1,
                GroupId = 1
            });

            database.Insert(new Thing2GroupDto
            {
                ThingId = 1,
                GroupId = 2
            });

            database.Insert(new Thing2GroupDto
            {
                ThingId = 2,
                GroupId = 2
            });

            database.Insert(new Thing2GroupDto
            {
                ThingId = 3,
                GroupId = 3
            });
        }

        [Test]
        public void TestSimple()
        {
            // fetching a simple POCO

            using (var scope = ScopeProvider.CreateScope())
            {
                var dtos = scope.Database.Fetch<Thing1Dto>(@"
                    SELECT zbThing1.id, zbThing1.name
                    FROM zbThing1");
                Assert.AreEqual(2, dtos.Count);
                Assert.AreEqual("one", dtos.First(x => x.Id == 1).Name);
            }
        }

        [Test]
        public void TestOneToOne()
        {
            // fetching a POCO that contains the ID of another POCO,
            // and fetching that other POCO at the same time

            using (var scope = ScopeProvider.CreateScope())
            {
                var dtos = scope.Database.Fetch<Thing2Dto>(@"
                    SELECT zbThing2.id, zbThing2.name, zbThing2.thingId, zbThing1.id Thing__id, zbThing1.name Thing__name
                    FROM zbThing2
                    JOIN zbThing1 ON zbThing2.thingId=zbThing1.id");
                Assert.AreEqual(3, dtos.Count);
                Assert.AreEqual("uno", dtos.First(x => x.Id == 1).Name);
                Assert.IsNotNull(dtos.First(x => x.Id == 1).Thing);
                Assert.AreEqual("one", dtos.First(x => x.Id == 1).Thing.Name);
            }
        }

        [Test]
        public void TestOneToManyOnOne()
        {
            // fetching a POCO that has a list of other POCOs,
            // and fetching these POCOs at the same time,
            // with a pk/fk relationship
            // for one single POCO

            using (var scope = ScopeProvider.CreateScope())
            {
                var dtos = scope.Database.FetchOneToMany<Thing3Dto>(x => x.Things, x => x.Id, @"
                    SELECT zbThing1.id, zbThing1.name, zbThing2.id, zbThing2.name, zbThing2.thingId
                    FROM zbThing1
                    JOIN zbThing2 ON zbThing1.id=zbThing2.thingId
                    WHERE zbThing1.id=1");
                Assert.AreEqual(1, dtos.Count);
                var dto1 = dtos.FirstOrDefault(x => x.Id == 1);
                Assert.IsNotNull(dto1);
                Assert.AreEqual("one", dto1.Name);
                Assert.IsNotNull(dto1.Things);
                Assert.AreEqual(2, dto1.Things.Count);
                var dto2 = dto1.Things.FirstOrDefault(x => x.Id == 1);
                Assert.IsNotNull(dto2);
                Assert.AreEqual("uno", dto2.Name);
            }
        }

        [Test]
        public void TestOneToManyOnMany()
        {
            // fetching a POCO that has a list of other POCOs,
            // and fetching these POCOs at the same time,
            // with a pk/fk relationship
            // for several POCOs
            //
            // the ORDER BY clause (matching x => x.Id) is required
            // for proper aggregation to take place

            using (var scope = ScopeProvider.CreateScope())
            {
                var dtos = scope.Database.FetchOneToMany<Thing3Dto>(x => x.Things, x => x.Id, @"
                    SELECT zbThing1.id, zbThing1.name, zbThing2.id, zbThing2.name, zbThing2.thingId
                    FROM zbThing1
                    JOIN zbThing2 ON zbThing1.id=zbThing2.thingId
                    ORDER BY zbThing1.id");
                Assert.AreEqual(2, dtos.Count);
                var dto1 = dtos.FirstOrDefault(x => x.Id == 1);
                Assert.IsNotNull(dto1);
                Assert.AreEqual("one", dto1.Name);
                Assert.IsNotNull(dto1.Things);
                Assert.AreEqual(2, dto1.Things.Count);
                var dto2 = dto1.Things.FirstOrDefault(x => x.Id == 1);
                Assert.IsNotNull(dto2);
                Assert.AreEqual("uno", dto2.Name);
            }
        }

        [Test]
        public void TestManyToMany()
        {
            // fetching a POCO that has a list of other POCOs,
            // and fetching these POCOs at the same time,
            // with an n-to-n intermediate table
            //
            // the ORDER BY clause (matching x => x.Id) is required
            // for proper aggregation to take place

            using (var scope = ScopeProvider.CreateScope())
            {
                var dtos = scope.Database.FetchOneToMany<Thing4Dto>(x => x.Groups, x => x.Id, @"
                    SELECT zbThing1.id, zbThing1.name, zbThingGroup.id, zbThingGroup.name
                    FROM zbThing1
                    JOIN zbThing2Group ON zbThing1.id=zbThing2Group.thingId
                    JOIN zbThingGroup ON zbThing2Group.groupId=zbThingGroup.id
                    ORDER BY zbThing1.id");
                Assert.AreEqual(2, dtos.Count);
                var dto1 = dtos.FirstOrDefault(x => x.Id == 1);
                Assert.IsNotNull(dto1);
                Assert.AreEqual("one", dto1.Name);
                Assert.IsNotNull(dto1.Groups);
                Assert.AreEqual(2, dto1.Groups.Count);
                var dto2 = dto1.Groups.FirstOrDefault(x => x.Id == 1);
                Assert.IsNotNull(dto2);
                Assert.AreEqual("g-one", dto2.Name);
            }
        }

        [Test]
        public void TestCalculated()
        {
            // fetching a POCO that has a countof other POCOs,
            // with an n-to-n intermediate table

            using (var scope = ScopeProvider.CreateScope())
            {
                var dtos = scope.Database.Fetch<Thing5Dto>(@"
                    SELECT zbThing1.id, zbThing1.name, COUNT(zbThing2Group.groupId) as groupCount
                    FROM zbThing1
                    JOIN zbThing2Group ON zbThing1.id=zbThing2Group.thingId
                    GROUP BY zbThing1.id, zbThing1.name");
                Assert.AreEqual(2, dtos.Count);
                var dto1 = dtos.FirstOrDefault(x => x.Id == 1);
                Assert.IsNotNull(dto1);
                Assert.AreEqual("one", dto1.Name);
                Assert.AreEqual(2, dto1.GroupCount);
                var dto2 = dtos.FirstOrDefault(x => x.Id == 2);
                Assert.IsNotNull(dto2);
                Assert.AreEqual("two", dto2.Name);
                Assert.AreEqual(1, dto2.GroupCount);
            }
        }

        // no test for ReferenceType.Foreign at the moment
        // it's more or less OneToOne, but NPoco manages the keys when
        // inserting or updating

        [Test]
        public void TestSql()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var sql = new Sql()
                    .Select("*")
                    .From("zbThing1")
                    .Where("id=@id", new { id = 1 });
                WriteSql(sql);
                var dto = scope.Database.Fetch<Thing1Dto>(sql).FirstOrDefault();
                Assert.IsNotNull(dto);
                Assert.AreEqual("one", dto.Name);

                //var sql2 = new Sql(sql.SQL, new { id = 1 });
                //WriteSql(sql2);
                //dto = Database.Fetch<Thing1Dto>(sql2).FirstOrDefault();
                //Assert.IsNotNull(dto);
                //Assert.AreEqual("one", dto.Name);

                var sql3 = new Sql(sql.SQL, 1);
                WriteSql(sql3);
                dto = scope.Database.Fetch<Thing1Dto>(sql3).FirstOrDefault();
                Assert.IsNotNull(dto);
                Assert.AreEqual("one", dto.Name);
            }
        }

        [Test]
        public void TestSqlTemplates()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                // this can be used for queries that we know we'll use a *lot* and
                // want to cache as a (static) template for ever, and ever - note
                // that using a MemoryCache would allow us to set a size limit, or
                // something equivalent, to reduce risk of memory explosion
                var sql = SqlTemplate.Get("xxx", () => new Sql()
                    .Select("*")
                    .From("zbThing1")
                    .Where("id=@id", new { id = "id" })).WithNamed(new { id = 1 });

                WriteSql(sql);
                var dto = scope.Database.Fetch<Thing1Dto>(sql).FirstOrDefault();
                Assert.IsNotNull(dto);
                Assert.AreEqual("one", dto.Name);

                var sql2 = SqlTemplate.Get("xxx", () => throw new InvalidOperationException("Should be cached.")).With(1);

                WriteSql(sql2);
                dto = scope.Database.Fetch<Thing1Dto>(sql2).FirstOrDefault();
                Assert.IsNotNull(dto);
                Assert.AreEqual("one", dto.Name);

                var sql3 = SqlTemplate.Get("xxx", () => throw new InvalidOperationException("Should be cached.")).WithNamed(new { id = 1 });

                WriteSql(sql3);
                dto = scope.Database.Fetch<Thing1Dto>(sql3).FirstOrDefault();
                Assert.IsNotNull(dto);
                Assert.AreEqual("one", dto.Name);
            }
        }

        public class SqlTemplate
        {
            private static readonly Dictionary<string, SqlTemplate> Templates = new Dictionary<string, SqlTemplate>();

            private readonly string _sql;
            private readonly Dictionary<int, string> _args;

            public SqlTemplate(string sql, object[] args)
            {
                _sql = sql;
                if (args.Length > 0)
                    _args = new Dictionary<int, string>();
                for (var i = 0; i < args.Length; i++)
                    _args[i] = args[i].ToString();
            }

            public static SqlTemplate Get(string key, Func<Sql> sqlBuilder)
            {
                if (Templates.TryGetValue(key, out var template)) return template;
                var sql = sqlBuilder();
                return Templates[key] = new SqlTemplate(sql.SQL, sql.Arguments);
            }

            // must pass the args in the proper order, faster
            public Sql With(params object[] args)
            {
                return new Sql(_sql, args);
            }

            // can pass named args, slower
            // so, not much different from what Where(...) does (ie reflection)
            public Sql WithNamed(object nargs)
            {
                var args = new object[_args.Count];
                var properties = nargs.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
                for (var i = 0; i < _args.Count; i++)
                {
                    if (!properties.TryGetValue(_args[i], out var propertyInfo))
                        throw new InvalidOperationException($"Invalid argument name \"{_args[i]}\".");
                    args[i] = propertyInfo.GetValue(nargs);
                }
                return new Sql(_sql, args);
            }
        }

        private static void WriteSql(Sql sql)
        {
            Console.WriteLine();
            Console.WriteLine(sql.SQL);
            var i = 0;
            foreach (var arg in sql.Arguments)
                Console.WriteLine($"  @{i++}: {arg}");
        }

        [TableName("zbThing1")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Thing1Dto
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }
        }

        [TableName("zbThing2")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Thing2Dto
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }

            [Column("thingId")]
            public int ThingId { get; set; }

            // reference is required else value remains null
            // columnName indicates which column has the id, referenceMembreName not needed if PK
            [Reference(ReferenceType.OneToOne, ColumnName = "thingId"/*, ReferenceMemberName="id"*/)]
            public Thing1Dto Thing { get; set; }
        }

        [TableName("zbThing1")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Thing3Dto
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }

            // reference is required else FetchOneToMany aggregation does not happen
            // does not seem to require ReferenceMemberName="thingId", ColumnName not needed if PK
            [Reference(ReferenceType.Many/*, ColumnName="id", ReferenceMemberName="thingId"*/)]
            public List<Thing2Dto> Things { get; set; }
        }

        [TableName("zbThingGroup")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class ThingGroupDto
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }
        }

        [TableName("zbThing2Group")]
        [PrimaryKey("thingId, groupId", AutoIncrement = false)]
        [ExplicitColumns]
        public class Thing2GroupDto
        {
            [Column("thingId")]
            public int ThingId { get; set; }

            [Column("groupId")]
            public int GroupId { get; set; }
        }

        [TableName("zbThing1")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Thing4Dto
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }

            // reference is required else FetchOneToMany aggregation does not happen
            // not sure ColumnName nor ReferenceMemberName make much sense here
            [Reference(ReferenceType.Many/*, ColumnName="id", ReferenceMemberName="thingId"*/)]
            public List<ThingGroupDto> Groups { get; set; }
        }

        [TableName("zbThing1")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Thing5Dto
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }

            [Column("groupCount")]
            [ResultColumn] // not included in insert/update, not sql-generated
                           //[ComputedColumn] // not included in insert/update, sql-generated
            public int GroupCount { get; set; }
        }
    }
}
