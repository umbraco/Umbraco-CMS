namespace Umbraco.Search.Examine.ValueSetBuilders;

public interface IValueSetBuilderFactory
{
    IValueSetBuilder<T> Retrieve<T>();
}
