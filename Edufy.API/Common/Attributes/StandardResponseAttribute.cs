namespace Edufy.API.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class StandardResponseAttribute : Attribute
{
}