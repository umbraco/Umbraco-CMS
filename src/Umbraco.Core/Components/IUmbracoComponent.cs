using Umbraco.Core.Composing;

namespace Umbraco.Core.Components
{
    // FIXME BREAK THIS!

    public interface IComposer : IDiscoverable
    {
        void Compose(Composition composition);
    }

    public interface IRuntimeComposer : IComposer
    { }

    [Require(typeof(IRuntimeComposer))]
    public interface ICoreComposer : IComposer
    { }

    [Require(typeof(ICoreComposer))]
    public interface IUserComposer : IComposer
    { }

    // will be disposed if disposable, CANT be disposed multiple times, beware!
    public interface IComponent
    { }
}
