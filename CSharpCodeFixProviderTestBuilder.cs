using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Tests.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Helpers;

internal static class CSharpCodeFixProviderTestBuilder
{
    public static CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider> Create<TAnalyzer, TCodeFixProvider>(ITestOutputHelper testOutputHelper)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFixProvider : CodeFixProvider, new()
        => new(testOutputHelper);
}

internal sealed class CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFixProvider : CodeFixProvider, new()
{
    private readonly List<string> _additionalGlobalOptionsLines = [];
    private readonly List<PackageIdentity> _additionalPackages = [];
    private readonly List<Type> _additionalTypes = [];
    private readonly ITestOutputHelper _testOutputHelper;
    private string? _fixedCode;
    private string? _testCode;

    public CSharpCodeFixProviderTestBuilder(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider> WithTestCode(string code)
    {
        _testCode = code;
        return this;
    }

    public CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider> WithFixedCode(string code)
    {
        _fixedCode = code;
        return this;
    }

    public CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider> WithNugetPackage(string packageName, string packageVersion)
    {
        var package = new PackageIdentity(packageName, packageVersion);
        _additionalPackages.Add(package);
        return this;
    }

    public CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider> WithAdditionalReference<T>()
    {
        _additionalTypes.Add(typeof(T));
        return this;
    }

    public CSharpCodeFixProviderTestBuilder<TAnalyzer, TCodeFixProvider> WithGlobalOptions(string optionsLine)
    {
        _additionalGlobalOptionsLines.Add(optionsLine);
        return this;
    }

    public CSharpCodeFixTest<TAnalyzer, TCodeFixProvider, DefaultVerifier> Build()
    {
        if (_testCode.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException($"No fix code added ({nameof(WithTestCode)})!");
        }

        if (_fixedCode.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException($"No fix code added ({nameof(WithFixedCode)})!");
        }

        LogSyntaxTree(_testCode);

        var codeFixTest = new CSharpCodeFixTest<TAnalyzer, TCodeFixProvider, DefaultVerifier>
        {
            TestState =
            {
#if NET9_0
                ReferenceAssemblies = Net.Assemblies.Net90.AddPackages([.._additionalPackages]),
#elif NET8_0
                ReferenceAssemblies = Net.Assemblies.Net80.AddPackages([.._additionalPackages]),
#else
                .NET framework not handled!
#endif
            },
            FixedCode = _fixedCode,
            TestCode = _testCode
        };

        foreach (var additionalType in _additionalTypes)
        {
            var reference = MetadataReference.CreateFromFile(additionalType.Assembly.Location);
            codeFixTest.TestState.AdditionalReferences.Add(reference);
        }

        if (_additionalGlobalOptionsLines.Count > 0)
        {
            var content = string.Join(Environment.NewLine, _additionalGlobalOptionsLines);
            codeFixTest.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", content));
        }

        return codeFixTest;
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
