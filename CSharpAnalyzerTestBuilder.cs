using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Tests.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Helpers;

internal static class CSharpAnalyzerTestBuilder
{
    public static CSharpAnalyzerTestBuilder<TAnalyzer> Create<TAnalyzer>(ITestOutputHelper testOutputHelper)
        where TAnalyzer : DiagnosticAnalyzer, new()
        => new(testOutputHelper);
}

internal sealed class CSharpAnalyzerTestBuilder<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private readonly List<string> _additionalGlobalOptionsLines = [];
    private readonly List<PackageIdentity> _additionalPackages = [];
    private readonly List<Type> _additionalTypes = [];
    private readonly ITestOutputHelper _testOutputHelper;
    private string? _code;

    public CSharpAnalyzerTestBuilder(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithTestCode(string code)
    {
        _code = code;
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithNugetPackage(string packageName, string packageVersion)
    {
        var package = new PackageIdentity(packageName, packageVersion);
        _additionalPackages.Add(package);
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithAdditionalReference<T>()
    {
        _additionalTypes.Add(typeof(T));
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithGlobalOptions(string optionsLine)
    {
        _additionalGlobalOptionsLines.Add(optionsLine);
        return this;
    }

    public CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> Build()
    {
        if (_code.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("No code added!");
        }

        LogSyntaxTree(_code);

        var analyzerTest = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                Sources =
                {
                    _code
                },
#if NET9_0
                ReferenceAssemblies = Net.Assemblies.Net90.AddPackages([.._additionalPackages]),
#elif NET8_0
                ReferenceAssemblies = Net.Assemblies.Net80.AddPackages([.._additionalPackages]),
#else
                .NET framework not handled!
#endif
            }
        };

        foreach (var additionalType in _additionalTypes)
        {
            var reference = MetadataReference.CreateFromFile(additionalType.Assembly.Location);
            analyzerTest.TestState.AdditionalReferences.Add(reference);
        }

        if (_additionalGlobalOptionsLines.Count > 0)
        {
            var content = string.Join(Environment.NewLine, _additionalGlobalOptionsLines);
            analyzerTest.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", content));
        }

        return analyzerTest;
    }

    private void LogSyntaxTree(string code)
    {
        TestFileMarkupParser.GetSpans(code, out var markupFreeCode, out ImmutableArray<TextSpan> _);
        var tree = CSharpSyntaxTree.ParseText(markupFreeCode);
        var root = tree.GetCompilationUnitRoot();
        var hierarchy = SyntaxTreeVisualizer.VisualizeHierarchy(root);
        _testOutputHelper.WriteLine(hierarchy);
    }
}
