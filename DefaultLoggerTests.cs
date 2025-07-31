using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Logging;
using FluentAssertions;

namespace AcidJunkie.Analyzers.Tests.Logging;

public sealed class DefaultLoggerTests
{
    [Fact]
    public void WriteLine_ShouldContainAllInformation()
    {
        // arrange
        var sut = new DefaultLogger<DefaultLoggerTests>();
        EnsureLogFileIsDeleted();

        // act
        sut.WriteLine(static () => "test1");

        // assert
        var logFileContent = GetLogFileContent();
        logFileContent.Should().NotBeEmpty();
        logFileContent.Should().Contain("Context=DefaultLoggerTests");
        logFileContent.Should().Contain($"Method={nameof(WriteLine_ShouldContainAllInformation)}");
        logFileContent.Should().Contain("Message=test1");
        logFileContent.Should().Contain($"PID={Environment.ProcessId}");
        logFileContent.Should().Contain($"TID={Environment.CurrentManagedThreadId}");
    }

    [SuppressMessage("Dunno", "MA0045:Do not use blocking calls in a sync method (need to make calling method async)", Justification = "We're in non-async context here")]
    private static string GetLogFileContent() =>
        File.ReadAllText(DefaultLogger.LogFilePath);

    private static void EnsureLogFileIsDeleted()
    {
        if (File.Exists(DefaultLogger.LogFilePath))
        {
            File.Delete(DefaultLogger.LogFilePath);
        }
    }
}
