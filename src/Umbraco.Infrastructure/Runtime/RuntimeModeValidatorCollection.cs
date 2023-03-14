using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Runtime;

public class RuntimeModeValidatorCollection : BuilderCollectionBase<IRuntimeModeValidator>
{
    public RuntimeModeValidatorCollection(Func<IEnumerable<IRuntimeModeValidator>> items)
        : base(items)
    { }
}
