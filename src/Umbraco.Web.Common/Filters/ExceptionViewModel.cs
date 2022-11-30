namespace Umbraco.Cms.Web.Common.Filters;

public class ExceptionViewModel
{
    public string? ExceptionMessage { get; set; }

    public Type? ExceptionType { get; set; }

    public string? StackTrace { get; set; }
}
