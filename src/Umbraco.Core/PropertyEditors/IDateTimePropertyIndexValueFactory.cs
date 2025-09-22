namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Factory for creating index values for Date Time properties.
/// </summary>
public interface IDateOnlyPropertyIndexValueFactory : IPropertyIndexValueFactory;

public interface ITimeOnlyPropertyIndexValueFactory : IPropertyIndexValueFactory;

public interface IDateTimeUnspecifiedPropertyIndexValueFactory : IPropertyIndexValueFactory;

public interface IDateTimeWithTimeZonePropertyIndexValueFactory : IPropertyIndexValueFactory;
