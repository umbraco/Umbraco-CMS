// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
public class NPocoFetchTests : UmbracoIntegrationTest
{
    [SetUp]
    protected void SeedDatabase()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            InsertData(ScopeAccessor.AmbientScope.Database);
            scope.Complete();
        }
    }

    private static void InsertData(IDatabase database)
    {
        database.Execute(@"
                CREATE TABLE zbThing1 (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL
                );");

        database.Insert(new Thing1Dto { Id = 1, Name = "one" });

        database.Insert(new Thing1Dto { Id = 2, Name = "two" });

        database.Execute(@"
                CREATE TABLE zbThing2 (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL,
                    thingId int NULL
                );");

        database.Insert(new Thing2Dto { Id = 1, Name = "uno", ThingId = 1 });

        database.Insert(new Thing2Dto { Id = 2, Name = "due", ThingId = 2 });

        database.Insert(new Thing2Dto { Id = 3, Name = "tri", ThingId = 1 });

        database.Execute(@"
                CREATE TABLE zbThingGroup (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL
                );");

        database.Insert(new ThingGroupDto { Id = 1, Name = "g-one" });

        database.Insert(new ThingGroupDto { Id = 2, Name = "g-two" });

        database.Insert(new ThingGroupDto { Id = 3, Name = "g-three" });

        database.Execute(@"
                CREATE TABLE zbThing2Group (
                    thingId int NOT NULL,
                    groupId int NOT NULL
                );");

        database.Insert(new Thing2GroupDto { ThingId = 1, GroupId = 1 });

        database.Insert(new Thing2GroupDto { ThingId = 1, GroupId = 2 });

        database.Insert(new Thing2GroupDto { ThingId = 2, GroupId = 2 });

        database.Insert(new Thing2GroupDto { ThingId = 3, GroupId = 3 });

        database.Execute(@"
                CREATE TABLE zbThingA1 (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL
                );");

        database.Execute(@"
                CREATE TABLE zbThingA2 (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL
                );");

        database.Execute(@"
                CREATE TABLE zbThingA3 (
                    id int PRIMARY KEY NOT NULL,
                    name NVARCHAR(255) NULL
                );");

        database.Execute(@"
                CREATE TABLE zbThingA12 (
                    thing1id int NOT NULL,
                    thing2id int NOT NULL,
                    name NVARCHAR(255) NOT NULL
                );");
    }

    [Test]
    public void TestSimple()
    {
        // Fetching a simple POCO
        using (var scope = ScopeProvider.CreateScope())
        {
            // This is the raw SQL, but it's better to use expressions and no magic strings!
            // var sql = @"
            //    SELECT zbThing1.id, zbThing1.name
            //    FROM zbThing1";
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Select<Thing1Dto>()
                .From<Thing1Dto>();

            var dtos = ScopeAccessor.AmbientScope.Database.Fetch<Thing1Dto>(sql);
            Assert.AreEqual(2, dtos.Count);
            Assert.AreEqual("one", dtos.First(x => x.Id == 1).Name);
        }
    }

    [Test]
    public void TestOneToOne()
    {
        // Fetching a POCO that contains the ID of another POCO,
        // and fetching that other POCO at the same time.
        using (var scope = ScopeProvider.CreateScope())
        {
            // This is the raw SQL, but it's better to use expressions and no magic strings!
            // var sql = @"
            //    SELECT zbThing2.id, zbThing2.name, zbThing2.thingId,
            //           zbThing1.id Thing__id, zbThing1.name Thing__name
            //    FROM zbThing2
            //    JOIN zbThing1 ON zbThing2.thingId=zbThing1.id";
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Select<Thing2Dto>(r => r.Select(x => x.Thing))
                .From<Thing2Dto>()
                .InnerJoin<Thing1Dto>().On<Thing2Dto, Thing1Dto>((t2, t1) => t2.ThingId == t1.Id);

            var dtos = ScopeAccessor.AmbientScope.Database.Fetch<Thing2Dto>(sql);
            Assert.AreEqual(3, dtos.Count);
            Assert.AreEqual("uno", dtos.First(x => x.Id == 1).Name);
            Assert.IsNotNull(dtos.First(x => x.Id == 1).Thing);
            Assert.AreEqual("one", dtos.First(x => x.Id == 1).Thing.Name);
        }
    }

    [Test]
    public void TestOneToManyOnOne()
    {
        // Fetching a POCO that has a list of other POCOs,
        // and fetching these POCOs at the same time,
        // with a pk/fk relationship
        // for one single POCO.
        using (var scope = ScopeProvider.CreateScope())
        {
            // This is the raw SQL, but it's better to use expressions and no magic strings!
            // var dtos = scope.Database.FetchOneToMany<Thing3Dto>(x => x.Things, x => x.Id, @"
            //    SELECT zbThing1.id AS Id, zbThing1.name AS Name,
            //           zbThing2.id AS Things__Id, zbThing2.name AS Things__Name, zbThing2.thingId AS Things__ThingId
            //    FROM zbThing1
            //    JOIN zbThing2 ON zbThing1.id=zbThing2.thingId
            //    WHERE zbThing1.id=1");
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Select<Thing3Dto>(r => r.Select(x => x.Things))
                .From<Thing3Dto>()
                .InnerJoin<Thing2Dto>().On<Thing3Dto, Thing2Dto>(left => left.Id, right => right.ThingId)
                .Where<Thing3Dto>(x => x.Id == 1);

            // var dtos = scope.Database.FetchOneToMany<Thing3Dto>(x => x.Things, x => x.Id, sql);
            var dtos = ScopeAccessor.AmbientScope.Database.FetchOneToMany<Thing3Dto>(x => x.Things, sql);

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
        // Fetching a POCO that has a list of other POCOs,
        // and fetching these POCOs at the same time,
        // with a pk/fk relationship
        // for several POCOs.
        //
        // The ORDER BY clause (matching x => x.Id) is required
        // for proper aggregation to take place.
        using (var scope = ScopeProvider.CreateScope())
        {
            // This is the raw SQL, but it's better to use expressions and no magic strings!
            // var sql = @"
            //    SELECT zbThing1.id AS Id, zbThing1.name AS Name,
            //           zbThing2.id AS Things__Id, zbThing2.name AS Things__Name, zbThing2.thingId AS Things__ThingId
            //    FROM zbThing1
            //    JOIN zbThing2 ON zbThing1.id=zbThing2.thingId
            //    ORDER BY zbThing1.id";
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Select<Thing3Dto>(r => r.Select(x => x.Things)) // select Thing3Dto, and Thing2Dto for Things
                .From<Thing3Dto>()
                .InnerJoin<Thing2Dto>().On<Thing3Dto, Thing2Dto>(left => left.Id, right => right.ThingId)
                .OrderBy<Thing3Dto>(x => x.Id);

            var dtos = ScopeAccessor.AmbientScope.Database.FetchOneToMany<Thing3Dto>(x => x.Things, /*x => x.Id,*/ sql);

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
    public void TestOneToManyOnManyTemplate()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            ScopeAccessor.AmbientScope.SqlContext.Templates.Clear();

            var sql = ScopeAccessor.AmbientScope.SqlContext.Templates.Get("xxx", s => s
                .Select<Thing3Dto>(r => r.Select(x => x.Things)) // select Thing3Dto, and Thing2Dto for Things
                .From<Thing3Dto>()
                .InnerJoin<Thing2Dto>().On<Thing3Dto, Thing2Dto>(left => left.Id, right => right.ThingId)
                .OrderBy<Thing3Dto>(x => x.Id)).Sql();

            // cached
            sql = ScopeAccessor.AmbientScope.SqlContext.Templates.Get("xxx", s => throw new InvalidOperationException())
                .Sql();

            var dtos = ScopeAccessor.AmbientScope.Database.FetchOneToMany<Thing3Dto>(x => x.Things, /*x => x.Id,*/ sql);

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
        // Fetching a POCO that has a list of other POCOs,
        // and fetching these POCOs at the same time,
        // with an n-to-n intermediate table.
        //
        // The ORDER BY clause (matching x => x.Id) is required
        // for proper aggregation to take place.
        using (var scope = ScopeProvider.CreateScope())
        {
            // This is the raw SQL, but it's better to use expressions and no magic strings!
            // var sql = @"
            //    SELECT zbThing1.id, zbThing1.name, zbThingGroup.id, zbThingGroup.name
            //    FROM zbThing1
            //    JOIN zbThing2Group ON zbThing1.id=zbThing2Group.thingId
            //    JOIN zbThingGroup ON zbThing2Group.groupId=zbThingGroup.id
            //    ORDER BY zbThing1.id";
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Select<Thing4Dto>(r => r.Select(x => x.Groups))
                .From<Thing4Dto>()
                .InnerJoin<Thing2GroupDto>().On<Thing4Dto, Thing2GroupDto>((t, t2g) => t.Id == t2g.ThingId)
                .InnerJoin<ThingGroupDto>().On<Thing2GroupDto, ThingGroupDto>((t2g, tg) => t2g.GroupId == tg.Id)
                .OrderBy<Thing4Dto>(x => x.Id);

            var dtos = ScopeAccessor.AmbientScope.Database.FetchOneToMany<Thing4Dto>(x => x.Groups, /*x => x.Id,*/ sql);

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
        // Fetching a POCO that has a countof other POCOs,
        // with an n-to-n intermediate table.
        using (var scope = ScopeProvider.CreateScope())
        {
            // This is the raw SQL, but it's better to use expressions and no magic strings!
            // var sql = @"
            //    SELECT zbThing1.id, zbThing1.name, COUNT(zbThing2Group.groupId) as groupCount
            //    FROM zbThing1
            //    JOIN zbThing2Group ON zbThing1.id=zbThing2Group.thingId
            //    GROUP BY zbThing1.id, zbThing1.name";
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Select<Thing1Dto>()
                .Append(", COUNT(zbThing2Group.groupId) AS groupCount") // FIXME:
                .From<Thing1Dto>()
                .InnerJoin<Thing2GroupDto>().On<Thing1Dto, Thing2GroupDto>((t, t2g) => t.Id == t2g.ThingId)
                .GroupBy<Thing1Dto>(x => x.Id, x => x.Name);

            var dtos = ScopeAccessor.AmbientScope.Database.Fetch<Thing5Dto>(sql);

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
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .SelectAll()
                .From<Thing1Dto>()
                .Where<Thing1Dto>(x => x.Id == 1);

            var dto = ScopeAccessor.AmbientScope.Database.Fetch<Thing1Dto>(sql).FirstOrDefault();
            Assert.IsNotNull(dto);
            Assert.AreEqual("one", dto.Name);

            //// var sql2 = new Sql(sql.SQL, new { id = 1 });
            //// WriteSql(sql2);
            //// dto = Database.Fetch<Thing1Dto>(sql2).FirstOrDefault();
            //// Assert.IsNotNull(dto);
            //// Assert.AreEqual("one", dto.Name);

            var sql3 = new Sql(sql.SQL, 1);
            dto = ScopeAccessor.AmbientScope.Database.Fetch<Thing1Dto>(sql3).FirstOrDefault();
            Assert.IsNotNull(dto);
            Assert.AreEqual("one", dto.Name);
        }
    }

    [Test]
    public void TestMultipleOneToOne()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            var tA1A = new ThingA1Dto { Id = 1, Name = "a1_a" };
            ScopeAccessor.AmbientScope.Database.Insert(tA1A);
            var tA1B = new ThingA1Dto { Id = 2, Name = "a1_b" };
            ScopeAccessor.AmbientScope.Database.Insert(tA1B);
            var tA1C = new ThingA1Dto { Id = 3, Name = "a1_c" };
            ScopeAccessor.AmbientScope.Database.Insert(tA1C);

            var tA2A = new ThingA2Dto { Id = 1, Name = "a2_a" };
            ScopeAccessor.AmbientScope.Database.Insert(tA2A);
            var tA2B = new ThingA2Dto { Id = 2, Name = "a2_b" };
            ScopeAccessor.AmbientScope.Database.Insert(tA2B);
            var tA2C = new ThingA2Dto { Id = 3, Name = "a2_c" };
            ScopeAccessor.AmbientScope.Database.Insert(tA2C);

            var tA3A = new ThingA3Dto { Id = 1, Name = "a3_a" };
            ScopeAccessor.AmbientScope.Database.Insert(tA3A);
            var tA3B = new ThingA3Dto { Id = 2, Name = "a3_b" };
            ScopeAccessor.AmbientScope.Database.Insert(tA3B);

            var k1 = new ThingA12Dto { Name = "a", Thing1Id = tA1A.Id, Thing2Id = tA2A.Id };
            ScopeAccessor.AmbientScope.Database.Insert(k1);
            var k2 = new ThingA12Dto { Name = "b", Thing1Id = tA1A.Id, Thing2Id = tA2B.Id };
            ScopeAccessor.AmbientScope.Database.Insert(k2);

            var sql = @"SELECT a1.id, a1.name,
a2.id AS T2A__Id, a2.name AS T2A__Name, a3.id AS T2A__T3__Id, a3.name AS T2A__T3__Name,
a2x.id AS T2B__Id, a2x.name AS T2B__Name, a3x.id AS T2B__T3__Id, a3x.name AS T2B__T3__Name
FROM zbThingA1 a1
JOIN zbThingA12 a12 ON a1.id=a12.thing1id AND a12.name='a'
JOIN zbThingA2 a2 ON a12.thing2id=a2.id
JOIN zbThingA3 a3 ON a2.id=a3.id
JOIN zbThingA12 a12x ON a1.id=a12x.thing1id AND a12x.name='b'
JOIN zbThingA2 a2x ON a12x.thing2id=a2x.id
JOIN zbThingA3 a3x ON a2x.id=a3x.id
";

            var ts = ScopeAccessor.AmbientScope.Database.Fetch<ThingA1Dto>(sql);
            Assert.AreEqual(1, ts.Count);

            var t = ts.First();
            Assert.AreEqual("a1_a", t.Name);
            Assert.AreEqual("a2_a", t.T2A.Name);
            Assert.AreEqual("a2_b", t.T2B.Name);

            Assert.AreEqual("a3_a", t.T2A.T3.Name);
            Assert.AreEqual("a3_b", t.T2B.T3.Name);

            scope.Complete();
        }
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
        [Reference(ReferenceType.OneToOne, ColumnName = "thingId" /*, ReferenceMemberName="id"*/)]
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
        [Reference(ReferenceType.Many /*, ColumnName="id", ReferenceMemberName="thingId"*/)]
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
        [Column("id")] public int Id { get; set; }

        [Column("name")] public string Name { get; set; }

        // reference is required else FetchOneToMany aggregation does not happen
        // not sure ColumnName nor ReferenceMemberName make much sense here
        [Reference(ReferenceType.Many /*, ColumnName="id", ReferenceMemberName="thingId"*/)]
        public List<ThingGroupDto> Groups { get; set; }
    }

    [TableName("zbThing1")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Thing5Dto
    {
        [Column("id")] public int Id { get; set; }

        [Column("name")] public string Name { get; set; }

        [Column("groupCount")]
        [ResultColumn] // not included in insert/update, not sql-generated
        public int GroupCount { get; set; }
    }

    [TableName("zbThingA1")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class ThingA1Dto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne)]
        public ThingA2Dto T2A { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne)]
        public ThingA2Dto T2B { get; set; }
    }

    [TableName("zbThingA2")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class ThingA2Dto
    {
        [Column("id")] public int Id { get; set; }

        [Column("name")] public string Name { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne)]
        public ThingA3Dto T3 { get; set; }
    }

    [TableName("zbThingA3")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class ThingA3Dto
    {
        [Column("id")] public int Id { get; set; }

        [Column("name")] public string Name { get; set; }
    }

    [TableName("zbThingA12")]
    [ExplicitColumns]
    public class ThingA12Dto
    {
        [Column("thing1id")] public int Thing1Id { get; set; }

        [Column("thing2id")] public int Thing2Id { get; set; }

        [Column("name")] public string Name { get; set; }
    }
}
