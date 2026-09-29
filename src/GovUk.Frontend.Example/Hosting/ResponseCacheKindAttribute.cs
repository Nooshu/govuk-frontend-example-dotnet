namespace GovUk.Frontend.Example.Hosting;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ResponseCacheKindAttribute(string kind) : Attribute
{
    public string Kind { get; } = kind;
}
