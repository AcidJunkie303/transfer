using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Diagnosers;

[SuppressMessage("Security", "S1075:Refactor your code not to use hardcoded absolution paths or URIs", Justification = "Path to the documentation")]
public static class CommonRules
{
    public static class UnhandledError
    {
        private const string Category = "Warning";
        public const string DiagnosticId = "AJ9999";
        public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ9999.md";

        public static readonly LocalizableString Title = "The AcidJunkie analyzer package encountered an error";
        public static readonly LocalizableString MessageFormat = "An error occurred in the AcidJunkie.Analyzers package. Check the log file 'AJ.Analyzers.log' in the temp folder.";
        public static readonly LocalizableString Description = Title;
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
    }

    public static class InvalidConfigurationValue
    {
        private const string Category = "Error";
        public const string DiagnosticId = "AJ9998";
        public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ9998.md";

        public static readonly LocalizableString Title = "Invalid AJ analyzer configuration value";
        public static readonly LocalizableString MessageFormat = "The configuration entry '{0}' in file '{1}' is invalid because: {2}";
        public static readonly LocalizableString Description = Title;
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri, "CompilationEnd");
    }
}
