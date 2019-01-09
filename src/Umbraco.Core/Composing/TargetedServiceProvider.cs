namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a base class for targeted service factories.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public abstract class TargetedServiceProvider<TService>
    {
        private readonly IFactory _factory;

        protected TargetedServiceProvider(IFactory factory)
        {
            _factory = factory;
        }

        public TService For<TTarget>() => _factory.GetInstanceFor<TService, TTarget>();
    }
}
