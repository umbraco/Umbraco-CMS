namespace Umbraco.Tests.Shared.Builders
{
    public abstract class ChildBuilderBase<TParent, TType> : BuilderBase<TType>
    {
        private readonly TParent _parentBuilder;

        protected ChildBuilderBase(TParent parentBuilder)
        {
            _parentBuilder = parentBuilder;
        }


        public TParent Done()
        {
            return _parentBuilder;
        }

    }
}
