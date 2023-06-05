using Microsoft.AspNetCore.Diagnostics;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

internal class UmbracoCompilationException : Exception, ICompilationException
{
    public IEnumerable<CompilationFailure?>? CompilationFailures { get; init; }
}
