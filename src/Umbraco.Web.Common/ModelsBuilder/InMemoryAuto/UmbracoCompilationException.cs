using Microsoft.AspNetCore.Diagnostics;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

internal sealed class UmbracoCompilationException : Exception, ICompilationException
{
    public IEnumerable<CompilationFailure?>? CompilationFailures { get; init; }
}
