using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Migrations;
using Umbraco.Tests.Migrations.Stubs;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class FindingMigrationsTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Find_Migrations_With_Target_Version_Six()
        {
            var builder = new MigrationCollectionBuilder(Container)
                .Add<AlterUserTableMigrationStub>()
                .Add<Dummy>()
                .Add<SixZeroMigration1>()
                .Add<SixZeroMigration2>()
                .Add<FourElevenMigration>()
                .Add<FiveZeroMigration>();

            var foundMigrations = builder.MigrationTypes;
            var targetVersion = new Version("6.0.0");
            var list = new List<Type>();

            foreach (var migration in foundMigrations)
            {
                var migrationAttribute = migration.FirstAttribute<MigrationAttribute>();
                if (migrationAttribute != null)
                {
                    if (migrationAttribute.TargetVersion == targetVersion)
                    {
                        list.Add(migration);
                    }
                }
            }

            Assert.That(list.Count, Is.EqualTo(3));

            // migrations Up actually executes in v8
            //foreach (var migration1 in list)
            //{
            //    var migration = (MigrationBase) migration1;
            //    migration.Up();
            //}

            //Assert.That(context.Expressions.Any(), Is.True);

            ////Console output
            //foreach (var expression in context.Expressions)
            //{
            //    Debug.Print(expression.ToString());
            //}
        }
    }
}
