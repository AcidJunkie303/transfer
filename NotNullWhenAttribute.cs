#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace -> The compiler supports NotNullWhenAttribute but it's not part of .NET Standard 2.0
namespace System.Diagnostics.CodeAnalysis;
#pragma warning restore IDE0130

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class NotNullWhenAttribute : Attribute
{
    public bool ReturnValue { get; }

    public NotNullWhenAttribute(bool returnValue)
    {
        ReturnValue = returnValue;
    }
}
