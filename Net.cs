using Microsoft.CodeAnalysis.Testing;

namespace AcidJunkie.Analyzers.Tests.Runtime;

internal static class Net
{
    internal static class Assemblies
    {
        private static readonly Lazy<ReferenceAssemblies> LazyNet80 = new(static () =>
            new ReferenceAssemblies(
                    "net8.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "8.0.4"),
                    Path.Combine("ref", "net8.0"))
                .WithPackages([new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "1.0.0.0")])
        );

        private static readonly Lazy<ReferenceAssemblies> LazyNet90 = new(static () =>
            new ReferenceAssemblies(
                    "net9.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "9.0.0"),
                    Path.Combine("ref", "net9.0"))
                .WithPackages([new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "1.0.0.0")])
        );

        public static ReferenceAssemblies Net80 => LazyNet80.Value;
        public static ReferenceAssemblies Net90 => LazyNet90.Value;
    }
}
