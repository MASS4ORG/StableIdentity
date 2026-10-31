using System.Diagnostics.CodeAnalysis;

namespace MASS4.StableIdentity;

/// <summary>
/// Opaque persistent identity of a registered type or symbol. It is never derived from CLR type names, so renaming
/// code does not invalidate saved data.
/// </summary>
/// <param name="Value">The underlying GUID.</param>
public readonly record struct StableId(Guid Value) : IComparable<StableId>
{
    /// <summary>The empty identity, which no registry or rename map accepts.</summary>
    public static StableId Empty => default;

    /// <summary>Whether this identity is empty.</summary>
    public bool IsEmpty => Value == Guid.Empty;

    /// <summary>Creates a new random identity.</summary>
    public static StableId New() => new(Guid.NewGuid());

    /// <summary>Parses the canonical hyphenated form produced by <see cref="ToString"/>.</summary>
    /// <exception cref="FormatException">The text is not in the canonical form.</exception>
    public static StableId Parse(string text) => new(Guid.ParseExact(text, "D"));

    /// <summary>Parses the canonical hyphenated form, returning false for anything else.</summary>
    public static bool TryParse([NotNullWhen(true)] string? text, out StableId id)
    {
        var parsed = Guid.TryParseExact(text, "D", out var value);
        id = new(value);
        return parsed;
    }

    /// <inheritdoc />
    public int CompareTo(StableId other) => Value.CompareTo(other.Value);

    /// <summary>Returns the canonical lowercase hyphenated form.</summary>
    public override string ToString() => Value.ToString("D");
}
