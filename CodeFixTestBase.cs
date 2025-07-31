using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests;

[SuppressMessage("Maintainability", "CA1515:Because an application's API isn't typically referenced from outside the assembly, types can be made internal", Justification = "This is the base class for our unit tests")]
public abstract class CodeFixTestBase<TAnalyzer, TCodeFixProvider>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFixProvider : CodeFixProvider, new()
{
    protected CodeFixTestBase(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    protected ITestOutputHelper TestOutputHelper { get; }

    internal CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider> CreateTestBuilder() => CSharpCodeFixProviderTestBuilder.Create<TAnalyzer, TCodeFixProvider>(TestOutputHelper);
}
