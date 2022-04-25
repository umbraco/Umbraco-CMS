using System;

// ReSharper disable once CheckNamespace
namespace Umbraco.Cms.Core.Scoping;

[Obsolete("Please use Umbraco.Cms.Infrastructure.Scoping.IScopeProvider or Umbraco.Cms.Core.ICoreScopeProvider instead.")]
public interface IScopeProvider : Infrastructure.Scoping.IScopeProvider
{
}
