using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Sync;
using Umbraco.Web.Search;

namespace Umbraco.Web.Compose
{
    /// <summary>
    /// Ensures that servers are automatically registered in the database, when using the database server registrar.
    /// </summary>
    /// <remarks>
    /// <para>At the moment servers are automatically registered upon first request and then on every
    /// request but not more than once per (configurable) period. This really is "for information & debug" purposes so
    /// we can look at the table and see what servers are registered - but the info is not used anywhere.</para>
    /// <para>Should we actually want to use this, we would need a better and more deterministic way of figuring
    /// out the "server address" ie the address to which server-to-server requests should be sent - because it
    /// probably is not the "current request address" - especially in multi-domains configurations.</para>
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    // TODO: This is legacy, we no longer need to do this but we don't want to change the behavior now
    [ComposeAfter(typeof(ExamineComposer))]
    public sealed class DatabaseServerRegistrarAndMessengerComposer : ComponentComposer<DatabaseServerRegistrarAndMessengerComponent>, ICoreComposer
    {
        public static DatabaseServerMessengerOptions GetDefaultOptions(IFactory factory)
        {
            return new DatabaseServerMessengerOptions();
        }

        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.SetDatabaseServerMessengerOptions(GetDefaultOptions);
            composition.SetServerMessenger<BatchedDatabaseServerMessenger>();
            composition.Register<ISyncBootStateAccessor>(factory => factory.GetInstance<IServerMessenger>() as BatchedDatabaseServerMessenger, Lifetime.Singleton);
        }
    }
}
