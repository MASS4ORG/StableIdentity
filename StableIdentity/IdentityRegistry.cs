using System.Diagnostics.CodeAnalysis;

namespace MASS4.StableIdentity;

/// <summary>
/// Resolves stable identities to registered entries, following its <see cref="RenameMap"/> so that identities found in
/// older data still resolve after a consumer renames or replaces a type.
/// </summary>
/// <typeparam name="T">The kind of entry.</typeparam>
public sealed class IdentityRegistry<T> where T : class, ITypeIdentity
{
    readonly Dictionary<StableId, T> entries = [];
    readonly RenameMap renames = new();

    /// <summary>All registered entries, in no particular order.</summary>
    public IEnumerable<T> Entries => entries.Values;

    /// <summary>Number of registered entries.</summary>
    public int Count => entries.Count;

    /// <summary>Rename entries as previous → next identity; add them through <see cref="AddRename"/>.</summary>
    public IReadOnlyCollection<KeyValuePair<StableId, StableId>> Renames => renames;

    /// <summary>Registers an entry under its own identity.</summary>
    /// <exception cref="ArgumentException">The identity is empty, registered, or recorded as renamed.</exception>
    public void Register(T entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.Id.IsEmpty)
        {
            throw new ArgumentException($"'{entry.DisplayName}' has an empty identity.", nameof(entry));
        }
        if (renames.IsRenamed(entry.Id))
        {
            throw new ArgumentException($"{entry.Id} is recorded as renamed and cannot be registered.", nameof(entry));
        }
        if (!entries.TryAdd(entry.Id, entry))
        {
            throw new ArgumentException($"{entry.Id} is already registered.", nameof(entry));
        }
    }

    /// <summary>Records that <paramref name="previous"/> now resolves to <paramref name="current"/>.</summary>
    /// <exception cref="ArgumentException">The rename is empty, conflicts, forms a cycle, or renames a registered
    /// identity.</exception>
    public void AddRename(StableId previous, StableId current)
    {
        if (entries.ContainsKey(previous))
        {
            throw new ArgumentException($"{previous} is still registered and cannot be renamed.", nameof(previous));
        }
        renames.Add(previous, current);
    }

    /// <summary>Follows rename entries until reaching an identity that is not renamed.</summary>
    public StableId Canonicalize(StableId id) => renames.Canonicalize(id);

    /// <summary>Resolves an identity, following renames, to its registered entry.</summary>
    public bool TryResolve(StableId id, [NotNullWhen(true)] out T? entry) =>
        entries.TryGetValue(Canonicalize(id), out entry);

    /// <summary>Resolves an identity or throws when it is not registered.</summary>
    /// <exception cref="KeyNotFoundException">The identity does not resolve.</exception>
    public T Resolve(StableId id) =>
        TryResolve(id, out var entry) ? entry : throw new KeyNotFoundException($"{id} is not registered.");
}
