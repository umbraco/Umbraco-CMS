using System.Globalization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

/*
 * This is a partial clone of the frameworks CompilationFailedExceptionFactory, a few things has been simplified to fit our needs.
 * https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Razor.RuntimeCompilation/src/CompilationFailedExceptionFactory.cs
 */
internal static class CompilationExceptionFactory
{
    public static UmbracoCompilationException Create(
        RazorCodeDocument codeDocument,
        IEnumerable<RazorDiagnostic> diagnostics)
    {
        // If a SourceLocation does not specify a file path, assume it is produced from parsing the current file.
        var messageGroups = diagnostics.GroupBy(
            razorError => razorError.Span.FilePath ?? codeDocument.Source.FilePath,
            StringComparer.Ordinal);

        var failures = new List<CompilationFailure>();
        foreach (var group in messageGroups)
        {
            var filePath = group.Key;
            var fileContent = ReadContent(codeDocument, filePath);
            var compilationFailure = new CompilationFailure(
                filePath,
                fileContent,
                compiledContent: string.Empty,
                messages: group.Select(parserError => CreateDiagnosticMessage(parserError, filePath)));
            failures.Add(compilationFailure);
        }

        return new UmbracoCompilationException{CompilationFailures = failures};
    }

    public static UmbracoCompilationException Create(
        RazorCodeDocument codeDocument,
        string compilationContent,
        string assemblyName,
        IEnumerable<Diagnostic> diagnostics)
    {
        var diagnosticGroups = diagnostics
            .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
            .GroupBy(diagnostic => GetFilePath(codeDocument, diagnostic), StringComparer.Ordinal);

        var failures = new List<CompilationFailure>();
        foreach (var group in diagnosticGroups)
        {
            var sourceFilePath = group.Key;
            string sourceFileContent;
            if (string.Equals(assemblyName, sourceFilePath, StringComparison.Ordinal))
            {
                // The error is in the generated code and does not have a mapping line pragma
                sourceFileContent = compilationContent;
            }
            else
            {
                sourceFileContent = ReadContent(codeDocument, sourceFilePath!);
            }

            var compilationFailure = new CompilationFailure(
                sourceFilePath,
                sourceFileContent,
                compilationContent,
                group.Select(GetDiagnosticMessage));

            failures.Add(compilationFailure);
        }

        return new UmbracoCompilationException{ CompilationFailures = failures};
    }

    private static string ReadContent(RazorCodeDocument codeDocument, string filePath)
    {
        RazorSourceDocument? sourceDocument;
        if (string.IsNullOrEmpty(filePath) || string.Equals(codeDocument.Source.FilePath, filePath, StringComparison.Ordinal))
        {
            sourceDocument = codeDocument.Source;
        }
        else
        {
            sourceDocument = codeDocument.Imports.FirstOrDefault(f => string.Equals(f.FilePath, filePath, StringComparison.Ordinal));
        }

        if (sourceDocument != null)
        {
            var contentChars = new char[sourceDocument.Length];
            sourceDocument.CopyTo(0, contentChars, 0, sourceDocument.Length);
            return new string(contentChars);
        }

        return string.Empty;
    }

    private static DiagnosticMessage GetDiagnosticMessage(Diagnostic diagnostic)
    {
        var mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
        return new DiagnosticMessage(
            diagnostic.GetMessage(CultureInfo.CurrentCulture),
            CSharpDiagnosticFormatter.Instance.Format(diagnostic, CultureInfo.CurrentCulture),
            mappedLineSpan.Path,
            mappedLineSpan.StartLinePosition.Line + 1,
            mappedLineSpan.StartLinePosition.Character + 1,
            mappedLineSpan.EndLinePosition.Line + 1,
            mappedLineSpan.EndLinePosition.Character + 1);
    }

    private static string GetFilePath(RazorCodeDocument codeDocument, Diagnostic diagnostic)
    {
        if (diagnostic.Location == Location.None)
        {
            return codeDocument.Source.FilePath;
        }

        return diagnostic.Location.GetMappedLineSpan().Path;
    }

    private static DiagnosticMessage CreateDiagnosticMessage(
        RazorDiagnostic razorDiagnostic,
        string filePath)
    {
        var sourceSpan = razorDiagnostic.Span;
        var message = razorDiagnostic.GetMessage(CultureInfo.CurrentCulture);
        return new DiagnosticMessage(
            message: message,
            formattedMessage: razorDiagnostic.ToString(),
            filePath: filePath,
            startLine: sourceSpan.LineIndex + 1,
            startColumn: sourceSpan.CharacterIndex,
            endLine: sourceSpan.LineIndex + 1,
            endColumn: sourceSpan.CharacterIndex + sourceSpan.Length);
    }
}
