﻿using System;
using System.Linq;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class MigrationPlanTests
    {
        [Test]
        public void CanExecute()
        {
            var logger = Mock.Of<ILogger>();

            var database = new TestDatabase();
            var scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var sqlContext = new SqlContext(new SqlCeSyntaxProvider(), DatabaseType.SQLCe, Mock.Of<IPocoDataFactory>());
            var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

            var migrationBuilder = Mock.Of<IMigrationBuilder>();
            Mock.Get(migrationBuilder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "DeleteRedirectUrlTable":
                            return new DeleteRedirectUrlTable(c);
                        case "NoopMigration":
                            return new NoopMigration();
                        default:
                            throw new NotSupportedException();
                    }
                });

            var plan = new MigrationPlan("default")
                .From(string.Empty)
                .To<DeleteRedirectUrlTable>("{4A9A1A8F-0DA1-4BCF-AD06-C19D79152E35}")
                .To<NoopMigration>("VERSION.33");

            var kvs = Mock.Of<IKeyValueService>();
            Mock.Get(kvs).Setup(x => x.GetValue(It.IsAny<string>())).Returns<string>(k => k == "Umbraco.Tests.MigrationPlan" ? string.Empty : null);

            string state;
            using (var s = scopeProvider.CreateScope())
            {
                // read current state
                var sourceState = kvs.GetValue("Umbraco.Tests.MigrationPlan") ?? string.Empty;

                // execute plan
                state = plan.Execute(s, sourceState, migrationBuilder, logger);

                // save new state
                kvs.SetValue("Umbraco.Tests.MigrationPlan", sourceState, state);

                // fixme - what about post-migrations?
                s.Complete();
            }

            Assert.AreEqual("VERSION.33", state);
            Assert.AreEqual(1, database.Operations.Count);
            Assert.AreEqual("DROP TABLE [umbracoRedirectUrl]", database.Operations[0].Sql);
        }

        [Test]
        public void CanAddMigrations()
        {
            var plan = new MigrationPlan("default");
            plan
                .From(string.Empty)
                .To("aaa")
                .To("bbb")
                .To("ccc");
        }

        [Test]
        public void CannotTransitionToSameState()
        {
            var plan = new MigrationPlan("default");
            Assert.Throws<ArgumentException>(() =>
            {
                plan.From("aaa").To("aaa");
            });
        }

        [Test]
        public void OnlyOneTransitionPerState()
        {
            var plan = new MigrationPlan("default");
            plan.From("aaa").To("bbb");
            Assert.Throws<InvalidOperationException>(() =>
            {
                plan.From("aaa").To("ccc");
            });
        }

        [Test]
        public void CannotContainTwoMoreHeads()
        {
            var plan = new MigrationPlan("default");
            plan
                .From(string.Empty)
                .To("aaa")
                .To("bbb")
                .From("ccc")
                .To("ddd");
            Assert.Throws<Exception>(() => plan.Validate());
        }

        [Test]
        public void CannotContainLoops()
        {
            var plan = new MigrationPlan("default");
            plan
                .From("aaa")
                .To("bbb")
                .To("ccc")
                .To("aaa");
            Assert.Throws<Exception>(() => plan.Validate());
        }

        [Test]
        public void ValidateUmbracoPlan()
        {
            var plan = new UmbracoPlan();
            plan.Validate();
            Console.WriteLine(plan.FinalState);
            Assert.IsFalse(string.IsNullOrWhiteSpace(plan.FinalState));
        }

        [Test]
        public void CanCopyChain()
        {
            var plan = new MigrationPlan("default");
            plan
                .From(string.Empty)
                .To("aaa")
                .To("bbb")
                .To("ccc")
                .To("ddd")
                .To("eee");

            plan
                .From("xxx")
                .To("yyy", "bbb", "ddd")
                .To("eee");

            WritePlanToConsole(plan);

            plan.Validate();
            Assert.AreEqual("eee", plan.FollowPath("xxx"));
            Assert.AreEqual("yyy", plan.FollowPath("xxx", "yyy"));
        }

        private void WritePlanToConsole(MigrationPlan plan)
        {
            var final = plan.Transitions.First(x => x.Value == null).Key;

            Console.WriteLine("plan \"{0}\" to final state \"{1}\":", plan.Name, final);
            foreach (var (_, transition) in plan.Transitions)
                if (transition != null)
                    Console.WriteLine(transition);
        }

        public class DeleteRedirectUrlTable : MigrationBase
        {
            public DeleteRedirectUrlTable(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                Delete.Table("umbracoRedirectUrl").Do();
            }
        }
    }
}
