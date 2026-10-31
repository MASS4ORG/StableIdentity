namespace MASS4.StableIdentity;

/// <summary>An entry addressable by a stable identity, such as a node type, port type or serialized class.</summary>
public interface ITypeIdentity
{
    /// <summary>The persistent identity written into saved data.</summary>
    StableId Id { get; }

    /// <summary>Human-readable name for tools and diagnostics; never used for resolution.</summary>
    string DisplayName { get; }
}
