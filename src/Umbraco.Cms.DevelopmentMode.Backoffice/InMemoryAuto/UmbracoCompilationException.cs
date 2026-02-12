using Microsoft.AspNetCore.Diagnostics;

namespace Umbraco.Cms.DevelopmentMode.Backoffice.InMemoryAuto;

internal sealed class UmbracoCompilationException : Exception, ICompilationException
{
    public IEnumerable<CompilationFailure?>? CompilationFailures { get; init; }
}
