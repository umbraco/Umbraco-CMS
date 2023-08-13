using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Umbraco.Cms.Core.Analyzers;

using VerifyCS = Umbraco.Cms.Tests.Analyzers.Utilities.CSharpAnalyzerVerifier<Umbraco.Cms.Core.Analyzers.InternalUsageDiagnosticAnalyzer>;

namespace Umbraco.Cms.Tests.Analyzers;

/// <summary>
/// Tests the <see cref="InternalUsageDiagnosticAnalyzer"/> class.
/// </summary>
/// <remarks>
/// Type links to types used in tests:
/// Tests with <see cref="Umbraco.Cms.Persistence.EFCore.UmbracoDbContext"/> and <see cref="Microsoft.EntityFrameworkCore.DbContextOptions{TContext}"/> of <see cref="Umbraco.Cms.Persistence.EFCore.UmbracoDbContext"/>
/// Tests with <see cref="Umbraco.Cms.Persistence.EFCore.Migrations.IMigrationProvider"/>
/// Tests with <see cref="Umbraco.Cms.Core.GuidUdi"/>
///
/// Inspired by: https://github.com/dotnet/efcore/blob/main/test/EFCore.Analyzers.Tests/InternalUsageDiagnosticAnalyzerTests.cs
/// </remarks>
public class InternalUsageDiagnosticAnalyzerTests
{
    [Test]
    public Task Instantiation_on_internal_type()
        => VerifySingleInternalUsageAsync(
"""
class C
{
    void M()
    {
        new {|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|}(new Microsoft.EntityFrameworkCore.DbContextOptions<Umbraco.Cms.Persistence.EFCore.UmbracoDbContext>());
    }
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public async Task Base_type()
    {
        var source = """
class MyClass : {|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|}
{
    MyClass() {|#1:: base(new Microsoft.EntityFrameworkCore.DbContextOptions<Umbraco.Cms.Persistence.EFCore.UmbracoDbContext>())|} {}
}
""";

        await VerifyCS.VerifyAnalyzerAsync(
            source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(InternalUsageDiagnosticAnalyzer.Descriptor.MessageFormat)
                .WithArguments("Umbraco.Cms.Persistence.EFCore.UmbracoDbContext"),
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(1)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(InternalUsageDiagnosticAnalyzer.Descriptor.MessageFormat)
                .WithArguments("Umbraco.Cms.Persistence.EFCore.UmbracoDbContext"));
    }

    [Test]
    public Task Implemented_interface()
        => VerifySingleInternalUsageAsync(
"""
using System;
using System.Threading.Tasks;
using Umbraco.Cms.Persistence.EFCore.Migrations;
            
class {|#0:MyClass|} : IMigrationProvider
{
    public string ProviderName => string.Empty;
    public Task MigrateAsync(EFCoreMigration migration) => Task.CompletedTask;
    public Task MigrateAllAsync() => Task.CompletedTask;
}
""",
"Umbraco.Cms.Persistence.EFCore.Migrations.IMigrationProvider");

    [Test]
    public Task Local_variable_declaration()
        => VerifySingleInternalUsageAsync(
"""
class C
{
    void M()
    {
        {|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|} umbracoDbContext = null;
    }
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public Task Generic_type_parameter_in_method_call()
        => VerifySingleInternalUsageAsync(
"""
class C
{
    void M()
    {
        void SomeGenericMethod<T>() {}
            
        {|#0:SomeGenericMethod<Umbraco.Cms.Persistence.EFCore.UmbracoDbContext>()|};
    }
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public Task Typeof()
        => VerifySingleInternalUsageAsync(
"""
class C
{
    void M()
    {
        var t = typeof({|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|});
    }
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public Task Field_declaration()
        => VerifySingleInternalUsageAsync(
"""
class MyClass
{
    private readonly {|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|} _stateManager;
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public Task Property_declaration()
        => VerifySingleInternalUsageAsync(
"""
class MyClass
{
    private {|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|} StateManager { get; set; }
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public Task Method_declaration_return_type()
        => VerifySingleInternalUsageAsync(
"""
class MyClass
{
    private {|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|} Foo() => null;
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public Task Method_declaration_parameter()
        => VerifySingleInternalUsageAsync(
"""
class MyClass
{
    private void Foo({|#0:Umbraco.Cms.Persistence.EFCore.UmbracoDbContext|} stateManager) {}
}
""",
"Umbraco.Cms.Persistence.EFCore.UmbracoDbContext");

    [Test]
    public Task No_warning_on_non_internal()
        => VerifyCS.VerifyAnalyzerAsync("""
class C
{
    void M()
    {
        var a = new Umbraco.Cms.Core.GuidUdi(string.Empty, System.Guid.NewGuid());
        var x = a.Guid;
    }
}
""");

    [Test]
    public Task No_warning_in_same_assembly()
        => VerifyCS.VerifyAnalyzerAsync("""
namespace Umbraco.My.Thing
{
    [Umbraco.Cms.Core.Attributes.UmbracoInternal]
    class MyClass
    {
        static internal void Foo() {}
    }
}

namespace Bar
{
    class Program
    {
        public void Main(string[] args) {
            Umbraco.My.Thing.MyClass.Foo();
        }
    }
}
""");

    private Task VerifySingleInternalUsageAsync(string source, string internalApi)
        => VerifyCS.VerifyAnalyzerAsync(
            source,
            VerifyCS.Diagnostic(InternalUsageDiagnosticAnalyzer.Id)
                .WithLocation(0)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessageFormat(InternalUsageDiagnosticAnalyzer.Descriptor.MessageFormat)
                .WithArguments(internalApi));
}
