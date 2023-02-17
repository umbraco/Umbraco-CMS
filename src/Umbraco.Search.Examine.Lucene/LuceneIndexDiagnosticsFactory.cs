// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Implementation of <see cref="IIndexDiagnosticsFactory" /> which returns <see cref="LuceneIndexDiagnostics" />
///     for lucene based indexes that don't have an implementation else fallsback to the default
///     <see cref="IndexDiagnosticsFactory" /> implementation.
/// </summary>
public class LuceneIndexDiagnosticsFactory : IndexDiagnosticsFactory
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILoggerFactory _loggerFactory;

    public LuceneIndexDiagnosticsFactory(
        ILoggerFactory loggerFactory,
        IHostingEnvironment hostingEnvironment)
    {
        _loggerFactory = loggerFactory;
        _hostingEnvironment = hostingEnvironment;
    }

    public override IIndexDiagnostics Create(IIndex index)
    {
        if (!(index is IIndexDiagnostics indexDiag))
        {
            if (index is LuceneIndex luceneIndex)
            {
                indexDiag = new LuceneIndexDiagnostics(
                    luceneIndex,
                    _loggerFactory.CreateLogger<LuceneIndexDiagnostics>(),
                    _hostingEnvironment,
                    null);
            }
            else
            {
                indexDiag = base.Create(index);
            }
        }

        return indexDiag;
    }
}
