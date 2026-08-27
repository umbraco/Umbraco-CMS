using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.SchemaLockdown;

[TestFixture]
public class SchemaRestrictionsTests
{
    private static readonly string[] EntityTypes =
    [
        Constants.UdiEntityType.DocumentType,
        Constants.UdiEntityType.DataType,
        Constants.UdiEntityType.DictionaryItem,
    ];

    private sealed class DelegateConfigurator : ISchemaLockdownConfigurator
    {
        private readonly Action<ISchemaRestrictionsBuilder> _configure;

        public DelegateConfigurator(Action<ISchemaRestrictionsBuilder> configure) => _configure = configure;

        public void Configure(ISchemaRestrictionsBuilder builder) => _configure(builder);
    }

    private static SchemaRestrictions CreateRestrictions(params Action<ISchemaRestrictionsBuilder>[] configure)
        => new(new SchemaLockdownConfiguratorCollection(
            () => configure.Select(ISchemaLockdownConfigurator (x) => new DelegateConfigurator(x))));

    [Test]
    public void Nothing_Is_Locked_When_No_Configurator_Is_Registered()
    {
        SchemaRestrictions restrictions = CreateRestrictions();

        Assert.Multiple(() =>
        {
            foreach (var entityType in EntityTypes)
            {
                Assert.That(restrictions.IsAllowed(entityType, SchemaOperation.Create), Is.True);
                Assert.That(restrictions.IsAllowed(entityType, SchemaOperation.Update), Is.True);
                Assert.That(restrictions.IsAllowed(entityType, SchemaOperation.Delete), Is.True);
            }
        });
    }

    [Test]
    public void Only_The_Entity_Types_A_Configurator_Names_Are_Locked()
    {
        SchemaRestrictions restrictions = CreateRestrictions(x => x.BlockMutations(Constants.UdiEntityType.DocumentType));

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Read), Is.True);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Create), Is.True);
        });
    }

    // Denials only accumulate, so every configurator's decisions survive whichever order they run in.
    [Test]
    public void Every_Configurators_Denials_Are_Kept_Whatever_The_Order()
    {
        SchemaRestrictions restrictions = CreateRestrictions(
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete),
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Create),
            x => x.Block(Constants.UdiEntityType.DataType, SchemaOperation.Update));

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Update), Is.True);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Update), Is.False);
        });
    }

    [Test]
    public void Blocking_The_Same_Operation_Twice_Is_Harmless()
    {
        SchemaRestrictions restrictions = CreateRestrictions(
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete),
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete));

        Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
    }

    [Test]
    public void Cells_Are_Independent()
    {
        SchemaRestrictions restrictions = CreateRestrictions(x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Create));

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.True);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Create), Is.True);
        });
    }

    [Test]
    public void BlockMutations_Blocks_Every_Operation_Except_Read()
    {
        SchemaRestrictions restrictions = CreateRestrictions(x => x.BlockMutations(Constants.UdiEntityType.DocumentType));

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Update), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Unknown), Is.False);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Read), Is.True);
        });
    }

    [Test]
    public void Cell_Lookup_Is_Case_Insensitive_On_Entity_Type()
    {
        SchemaRestrictions restrictions = CreateRestrictions(x => x.Block("Dictionary-Item", SchemaOperation.Update));

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsAllowed("dictionary-item", SchemaOperation.Update), Is.False);
            Assert.That(restrictions.IsAllowed("Dictionary-Item", SchemaOperation.Update), Is.False);
            Assert.That(restrictions.IsAllowed("DICTIONARY-ITEM", SchemaOperation.Unknown), Is.False);
            Assert.That(restrictions.RestrictedEntityTypes, Is.EquivalentTo(new[] { "Dictionary-Item" }));
        });
    }

    // A single denial is enough: the unclassified operation could be the one that was denied.
    [Test]
    public void Unknown_Is_Blocked_Wherever_Any_Operation_Is_Blocked()
    {
        SchemaRestrictions single = CreateRestrictions(x => x.Block(Constants.UdiEntityType.DataType, SchemaOperation.Create));
        SchemaRestrictions all = CreateRestrictions(x => x.BlockMutations(Constants.UdiEntityType.DataType));

        Assert.Multiple(() =>
        {
            Assert.That(single.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Unknown), Is.False);
            Assert.That(all.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Unknown), Is.False);
        });
    }

    [Test]
    public void Unknown_Stays_Permitted_On_An_Entity_Type_Nothing_Is_Blocked_On()
    {
        SchemaRestrictions restrictions = CreateRestrictions(x => x.BlockMutations(Constants.UdiEntityType.DocumentType));

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Unknown), Is.True);
            Assert.That(CreateRestrictions().IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Unknown), Is.True);
        });
    }

    // Nothing consults a read cell, so a configurator writing one would be writing something that cannot take effect.
    // This is what lets every consumer take reads as permitted without asking.
    [Test]
    public void Reads_Stay_Permitted_Whatever_A_Configurator_Writes()
    {
        SchemaRestrictions restrictions = CreateRestrictions(
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Read),
            x => x.BlockMutations(Constants.UdiEntityType.DocumentType),
            x => x.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Read));

        Assert.Multiple(() =>
        {
            foreach (var entityType in EntityTypes)
            {
                Assert.That(restrictions.IsAllowed(entityType, SchemaOperation.Read), Is.True);
            }
        });
    }

    [Test]
    public void Restricted_Entity_Types_Are_Empty_When_No_Configurator_Is_Registered()
        => Assert.That(CreateRestrictions().RestrictedEntityTypes, Is.Empty);

    [Test]
    public void Restricted_Entity_Types_Are_Only_Those_A_Configurator_Blocked_Something_On()
    {
        SchemaRestrictions restrictions = CreateRestrictions(
            x => x.BlockMutations(Constants.UdiEntityType.DocumentType),
            x => x.Block(Constants.UdiEntityType.DataType, SchemaOperation.Delete),
            x => x.Block(Constants.UdiEntityType.MediaType, SchemaOperation.Read));

        Assert.That(
            restrictions.RestrictedEntityTypes,
            Is.EquivalentTo(new[] { Constants.UdiEntityType.DocumentType, Constants.UdiEntityType.DataType }));
    }

    [Test]
    public void Restricted_Entity_Types_Are_Visible_To_A_Configurator_Still_Running()
    {
        string[]? seen = null;

        CreateRestrictions(
            x => x.BlockMutations(Constants.UdiEntityType.DocumentType),
            x => seen = x.RestrictedEntityTypes.ToArray());

        Assert.That(seen, Is.EquivalentTo(new[] { Constants.UdiEntityType.DocumentType }));
    }

    [Test]
    public void Mutating_After_Construction_Throws()
    {
        SchemaRestrictions restrictions = CreateRestrictions(x => x.BlockMutations(Constants.UdiEntityType.DocumentType));

        Assert.Throws<InvalidOperationException>(
            () => restrictions.Block(Constants.UdiEntityType.DataType, SchemaOperation.Create));
    }
}
