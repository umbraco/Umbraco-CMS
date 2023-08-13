using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Umbraco.Cms.Core.Analyzers;

/// <summary>
/// Analyzes the usage of internal APIs.
/// </summary>
/// <remarks>
/// Inspired by: https://github.com/dotnet/efcore/blob/main/src/EFCore.Analyzers/InternalUsageDiagnosticAnalyzer.cs
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InternalUsageDiagnosticAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "UM1001";

    public static readonly DiagnosticDescriptor Descriptor
        // HACK: Work around dotnet/roslyn-analyzers#5890 by not using target-typed new
        = new DiagnosticDescriptor(
            Id,
            title: "Internal Umbraco API usage",
            messageFormat: "{0} is an internal API of Umbraco CMS and not subject to the same compatibility standards as public APIs. It may be changed or removed without notice in any release.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(
            AnalyzeNode,
            OperationKind.FieldReference,
            OperationKind.PropertyReference,
            OperationKind.MethodReference,
            OperationKind.EventReference,
            OperationKind.Invocation,
            OperationKind.ObjectCreation,
            OperationKind.VariableDeclaration,
            OperationKind.TypeOf);

        context.RegisterSymbolAction(
            AnalyzeSymbol,
            SymbolKind.NamedType,
            SymbolKind.Method,
            SymbolKind.Property,
            SymbolKind.Field,
            SymbolKind.Event);
    }

    private static void AnalyzeNode(OperationAnalysisContext context)
    {
        switch (context.Operation)
        {
            case IFieldReferenceOperation fieldReference:
                AnalyzeMember(context, fieldReference.Field);
                break;

            case IPropertyReferenceOperation propertyReference:
                AnalyzeMember(context, propertyReference.Property);
                break;

            case IEventReferenceOperation eventReference:
                AnalyzeMember(context, eventReference.Event);
                break;

            case IMethodReferenceOperation methodReference:
                AnalyzeMember(context, methodReference.Method);
                break;

            case IObjectCreationOperation { Constructor: { } constructor }:
                AnalyzeMember(context, constructor);
                break;

            case IInvocationOperation invocation:
                AnalyzeInvocation(context, invocation);
                break;

            case IVariableDeclarationOperation variableDeclaration:
                AnalyzeVariableDeclaration(context, variableDeclaration);
                break;

            case ITypeOfOperation typeOf:
                AnalyzeTypeof(context, typeOf);
                break;

            default:
                throw new ArgumentException($"Unexpected operation: {context.Operation.Kind}");
        }
    }

    private static void AnalyzeMember(OperationAnalysisContext context, ISymbol symbol)
    {
        if (symbol.ContainingAssembly?.Equals(context.Compilation.Assembly, SymbolEqualityComparer.Default) == true)
        {
            // Skip all methods inside the same assembly - internal access is fine
            return;
        }

        INamedTypeSymbol containingType = symbol.ContainingType;

        if (HasInternalAttribute(symbol))
        {
            ReportDiagnostic(context, symbol.Name == WellKnownMemberNames.InstanceConstructorName ? containingType : $"{containingType}.{symbol.Name}");
            return;
        }

        if (IsInternal(context, containingType))
        {
            ReportDiagnostic(context, containingType);
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, IInvocationOperation invocation)
    {
        // First check for any internal type parameters
        foreach (ITypeSymbol typeSymbol in invocation.TargetMethod.TypeArguments)
        {
            if (IsInternal(context, typeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Operation.Syntax.GetLocation(), typeSymbol));
            }
        }

        // Then check the method being invoked
        AnalyzeMember(context, invocation.TargetMethod);
    }

    private static void AnalyzeVariableDeclaration(OperationAnalysisContext context, IVariableDeclarationOperation variableDeclaration)
    {
        foreach (IVariableDeclaratorOperation declarator in variableDeclaration.Declarators)
        {
            if (IsInternal(context, declarator.Symbol.Type))
            {
                SyntaxNode syntax = context.Operation.Syntax switch
                {
                    CSharpSyntax.VariableDeclarationSyntax s => s.Type,
                    _ => context.Operation.Syntax,
                };
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, syntax.GetLocation(), declarator.Symbol.Type));
                return;
            }
        }
    }

    private static void AnalyzeTypeof(OperationAnalysisContext context, ITypeOfOperation typeOf)
    {
        if (IsInternal(context, typeOf.TypeOperand))
        {
            ReportDiagnostic(context, typeOf.TypeOperand);
        }
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        switch (context.Symbol)
        {
            case INamedTypeSymbol symbol:
                AnalyzeNamedTypeSymbol(context, symbol);
                break;

            case IMethodSymbol symbol:
                AnalyzeMethodTypeSymbol(context, symbol);
                break;

            case IFieldSymbol symbol:
                AnalyzeMemberDeclarationTypeSymbol(context, symbol, symbol.Type);
                break;

            case IPropertySymbol symbol:
                AnalyzeMemberDeclarationTypeSymbol(context, symbol, symbol.Type);
                break;

            case IEventSymbol symbol:
                AnalyzeMemberDeclarationTypeSymbol(context, symbol, symbol.Type);
                break;

            default:
                throw new ArgumentException($"Unexpected {nameof(ISymbol)}: {context.Symbol.GetType().Name}");
        }
    }

    private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context, INamedTypeSymbol symbol)
    {
        if (symbol.BaseType is ITypeSymbol baseSymbol
            && IsInternal(context, baseSymbol))
        {
            foreach (SyntaxReference declaringSyntax in symbol.DeclaringSyntaxReferences)
            {
                Location location = declaringSyntax.GetSyntax() switch
                {
                    CSharpSyntax.ClassDeclarationSyntax { BaseList.Types.Count: > 0 } s => s.BaseList.Types[0].GetLocation(),
                    { } otherSyntax => otherSyntax.GetLocation(),
                };

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, baseSymbol));
            }
        }

        foreach (INamedTypeSymbol? @interface in symbol.Interfaces.Where(i => IsInternal(context, i)))
        {
            foreach (SyntaxReference declaringSyntax in symbol.DeclaringSyntaxReferences)
            {
                Location location = declaringSyntax.GetSyntax() switch
                {
                    CSharpSyntax.ClassDeclarationSyntax s => s.Identifier.GetLocation(),
                    { } otherSyntax => otherSyntax.GetLocation(),
                };

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, @interface));
            }
        }
    }

    private static void AnalyzeMethodTypeSymbol(SymbolAnalysisContext context, IMethodSymbol symbol)
    {
        if (symbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
        {
            // Property getters/setters are handled via IPropertySymbol
            return;
        }

        if (IsInternal(context, symbol.ReturnType))
        {
            foreach (SyntaxReference declaringSyntax in symbol.DeclaringSyntaxReferences)
            {
                Location location = declaringSyntax.GetSyntax() switch
                {
                    CSharpSyntax.MethodDeclarationSyntax s => s.ReturnType.GetLocation(),
                    { } otherSyntax => otherSyntax.GetLocation(),
                };

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, symbol.ReturnType));
            }
        }

        foreach (IParameterSymbol? paramSymbol in symbol.Parameters.Where(ps => IsInternal(context, ps.Type)))
        {
            foreach (SyntaxReference declaringSyntax in paramSymbol.DeclaringSyntaxReferences)
            {
                Location location = declaringSyntax.GetSyntax() switch
                {
                    CSharpSyntax.ParameterSyntax { Type: not null } s => s.Type.GetLocation(),

                    { } otherSyntax => otherSyntax.GetLocation()
                };

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, paramSymbol.Type));
            }
        }
    }

    private static void AnalyzeMemberDeclarationTypeSymbol(
        SymbolAnalysisContext context,
        ISymbol declarationSymbol,
        ITypeSymbol typeSymbol)
    {
        if (IsInternal(context, typeSymbol))
        {
            foreach (SyntaxReference declaringSyntax in declarationSymbol.DeclaringSyntaxReferences)
            {
                ReportDiagnostic(context, declaringSyntax.GetSyntax(), typeSymbol);
            }
        }
    }

    private static void ReportDiagnostic(OperationAnalysisContext context, object messageArg)
        => context.ReportDiagnostic(
            Diagnostic.Create(Descriptor, NarrowDownSyntax(context.Operation.Syntax).GetLocation(), messageArg));

    private static void ReportDiagnostic(SymbolAnalysisContext context, SyntaxNode syntax, object messageArg)
        => context.ReportDiagnostic(Diagnostic.Create(Descriptor, NarrowDownSyntax(syntax).GetLocation(), messageArg));

    /// <summary>
    ///     Given a syntax node, pattern matches some known types and returns a narrowed-down node for the type syntax which
    ///     should be reported in diagnostics.
    /// </summary>
    private static SyntaxNode NarrowDownSyntax(SyntaxNode syntax)
        => syntax switch
        {
            CSharpSyntax.InvocationExpressionSyntax { Expression: CSharpSyntax.MemberAccessExpressionSyntax memberAccessSyntax } => memberAccessSyntax.Name,
            CSharpSyntax.MemberAccessExpressionSyntax s => s.Name,
            CSharpSyntax.ObjectCreationExpressionSyntax s => s.Type,
            CSharpSyntax.PropertyDeclarationSyntax s => s.Type,
            CSharpSyntax.VariableDeclaratorSyntax declarator
                => declarator.Parent is CSharpSyntax.VariableDeclarationSyntax declaration
                    ? declaration.Type
                    : declarator,
            CSharpSyntax.TypeOfExpressionSyntax s => s.Type,

            _ => syntax,
        };

    private static bool IsInternal(SymbolAnalysisContext context, ITypeSymbol symbol)
        => symbol.ContainingAssembly?.Equals(context.Compilation.Assembly, SymbolEqualityComparer.Default) != true
            && HasInternalAttribute(symbol);

    private static bool IsInternal(OperationAnalysisContext context, ITypeSymbol symbol)
        => symbol.ContainingAssembly?.Equals(context.Compilation.Assembly, SymbolEqualityComparer.Default) != true
            && HasInternalAttribute(symbol);

    private static bool HasInternalAttribute(ISymbol symbol)
        => symbol.GetAttributes().Any(
            a =>
                a.AttributeClass!.ToDisplayString()
                == "Umbraco.Cms.Core.Attributes.UmbracoInternalAttribute");
}
