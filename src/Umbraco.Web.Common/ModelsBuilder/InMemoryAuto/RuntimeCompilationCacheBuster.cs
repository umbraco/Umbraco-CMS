using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

internal class RuntimeCompilationCacheBuster
{
    private readonly IViewCompilerProvider _viewCompilerProvider;
    private readonly IRazorViewEngine _razorViewEngine;

    public RuntimeCompilationCacheBuster(
        IViewCompilerProvider viewCompilerProvider,
        IRazorViewEngine razorViewEngine)
    {
        _viewCompilerProvider = viewCompilerProvider;
        _razorViewEngine = razorViewEngine;
    }

    private RazorViewEngine ViewEngine
    {
        get
        {
            if (_razorViewEngine is RazorViewEngine typedViewEngine)
            {
                return typedViewEngine;
            }

            throw new InvalidOperationException(
                "Unable to resolve RazorViewEngine, this means we can't clear the cache and views won't render properly");
        }
    }

    private CollectibleRuntimeViewCompiler ViewCompiler
    {
        get
        {
            if (_viewCompilerProvider.GetCompiler() is CollectibleRuntimeViewCompiler collectibleCompiler)
            {
                return collectibleCompiler;
            }

            throw new InvalidOperationException("Unable to resolve CollectibleRuntimeViewCompiler, and is unable to clear the cache");
        }
    }

    public void BustCache()
    {
        ViewCompiler.ClearCache();
        Action<RazorViewEngine>? clearCacheMethod = ReflectionUtilities.EmitMethod<Action<RazorViewEngine>>("ClearCache");
        clearCacheMethod?.Invoke(ViewEngine);
    }
}
