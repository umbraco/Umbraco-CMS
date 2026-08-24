using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.SchemaLockdown;

[TestFixture]
public class SchemaLockdownRulesTests
{
    private sealed class DelegateConfigurator : ISchemaLockdownConfigurator
    {
        private readonly Action<ISchemaLockdownConfigurableRules> _configure;

        public DelegateConfigurator(Action<ISchemaLockdownConfigurableRules> configure) => _configure = configure;

        public void Configure(ISchemaLockdownConfigurableRules rules) => _configure(rules);
    }

    private static SchemaLockdownRules CreateRules(params Action<ISchemaLockdownConfigurableRules>[] configure)
        => new(new SchemaLockdownConfiguratorCollection(
            () => configure.Select(ISchemaLockdownConfigurator (x) => new DelegateConfigurator(x))));

    [Test]
    public void Nothing_Is_Locked_When_No_Configurator_Is_Registered()
    {
        SchemaLockdownRules rules = CreateRules();

        Assert.Multiple(() =>
        {
            foreach (var entityType in SchemaEntityTypes.All)
            {
                Assert.That(rules.IsAllowed(entityType, SchemaOperation.Create), Is.True);
                Assert.That(rules.IsAllowed(entityType, SchemaOperation.Update), Is.True);
                Assert.That(rules.IsAllowed(entityType, SchemaOperation.Delete), Is.True);
            }
        });
    }

    [Test]
    public void Only_The_Entity_Types_A_Configurator_Names_Are_Locked()
    {
        SchemaLockdownRules rules = CreateRules(x => x.BlockMutations(Constants.UdiEntityType.DocumentType));

        Assert.Multiple(() =>
        {
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Read), Is.True);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Create), Is.True);
        });
    }

    [Test]
    public void Configurators_Run_In_Order_And_Later_Writes_Win()
    {
        SchemaLockdownRules rules = CreateRules(
            x => x.Allow(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete),
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete));

        Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
    }

    [Test]
    public void Block_Then_Allow_Leaves_Cell_Allowed()
    {
        SchemaLockdownRules rules = CreateRules(x =>
        {
            x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete);
            x.Allow(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete);
        });

        Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.True);
    }

    [Test]
    public void Cells_Are_Independent()
    {
        SchemaLockdownRules rules = CreateRules(x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Create));

        Assert.Multiple(() =>
        {
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.True);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Create), Is.True);
        });
    }

    [Test]
    public void BlockMutations_Blocks_Every_Operation_Except_Read()
    {
        SchemaLockdownRules rules = CreateRules(x => x.BlockMutations(Constants.UdiEntityType.DocumentType));

        Assert.Multiple(() =>
        {
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Update), Is.False);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Unknown), Is.False);
            Assert.That(rules.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Read), Is.True);
        });
    }

    [Test]
    public void Cell_Lookup_Is_Case_Insensitive_On_Entity_Type()
    {
        SchemaLockdownRules blocked = CreateRules(x => x.Block("Dictionary-Item", SchemaOperation.Update));

        Assert.Multiple(() =>
        {
            Assert.That(blocked.IsAllowed("dictionary-item", SchemaOperation.Update), Is.False);
            Assert.That(blocked.IsAllowed("Dictionary-Item", SchemaOperation.Update), Is.False);
        });

        SchemaLockdownRules allowed = CreateRules(x =>
        {
            x.Block("Dictionary-Item", SchemaOperation.Update);
            x.Allow("dictionary-item", SchemaOperation.Update);
        });

        Assert.That(allowed.IsAllowed("DICTIONARY-ITEM", SchemaOperation.Update), Is.True);
    }

    // Nothing consults a read cell, so a configurator writing one would be writing something that cannot take effect.
    // This is what lets every consumer take reads as permitted without asking.
    [Test]
    public void Reads_Stay_Permitted_Whatever_A_Configurator_Writes()
    {
        SchemaLockdownRules rules = CreateRules(
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Read),
            x => x.BlockMutations(Constants.UdiEntityType.DocumentType),
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Read));

        Assert.Multiple(() =>
        {
            foreach (var entityType in SchemaEntityTypes.All)
            {
                Assert.That(rules.IsAllowed(entityType, SchemaOperation.Read), Is.True);
            }
        });
    }

    [Test]
    public void Mutating_After_Construction_Throws()
    {
        SchemaLockdownRules rules = CreateRules(x => x.BlockMutations(Constants.UdiEntityType.DocumentType));

        Assert.Throws<InvalidOperationException>(
            () => rules.Allow(Constants.UdiEntityType.DataType, SchemaOperation.Create));
    }
}
