namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithPropertyValues
    {
        object PropertyValues { get; set; }

        string PropertyValuesCulture { get; set; }

        string PropertyValuesSegment { get; set; }
    }
}
