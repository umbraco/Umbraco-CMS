namespace Umbraco.Search.ValueSet.ValueSetBuilders;

public interface IValueSetBuilderFactory
{
    IValueSetBuilder<T> Retrieve<T>();
}
