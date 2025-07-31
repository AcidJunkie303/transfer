using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests;

[SuppressMessage("Maintainability", "CA1515:Because an application's API isn't typically referenced from outside the assembly, types can be made internal", Justification = "This is the base class for our unit tests")]
public abstract class TestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected ITestOutputHelper TestOutputHelper { get; }

    protected TestBase(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    private protected CSharpAnalyzerTestBuilder<TAnalyzer> CreateTesterBuilder() => CSharpAnalyzerTestBuilder.Create<TAnalyzer>(TestOutputHelper);
}
