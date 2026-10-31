using System.Collections;

namespace MASS4.StableIdentity;

/// <summary>
/// Records that previous identities now resolve to current ones, so data saved under an old identity still loads after
/// a consumer renames or replaces a type. Chains are allowed and followed; conflicts and cycles are rejected. Not
/// synchronized: finish adding entries before reading from several threads.
/// </summary>
public sealed class RenameMap : IReadOnlyCollection<KeyValuePair<StableId, StableId>>
{
    readonly Dictionary<StableId, StableId> renames = [];

    /// <summary>Number of rename entries; chains are not collapsed.</summary>
    public int Count => renames.Count;

    /// <summary>Whether <paramref name="id"/> is recorded as renamed.</summary>
    public bool IsRenamed(StableId id) => renames.ContainsKey(id);

    /// <summary>Records that <paramref name="previous"/> now resolves to <paramref name="current"/>.</summary>
    /// <exception cref="ArgumentException">The rename is empty, already recorded, or forms a cycle.</exception>
    public void Add(StableId previous, StableId current)
    {
        if (previous.IsEmpty || current.IsEmpty)
        {
            throw new ArgumentException("Renames cannot involve the empty identity.");
        }
        if (Canonicalize(current) == previous)
        {
            throw new ArgumentException($"Renaming {previous} to {current} would form a cycle.", nameof(current));
        }
        if (!renames.TryAdd(previous, current))
        {
            throw new ArgumentException($"{previous} already has a rename entry.", nameof(previous));
        }
    }

    /// <summary>Follows rename entries until reaching an identity that is not renamed.</summary>
    public StableId Canonicalize(StableId id)
    {
        // Cycles are rejected by Add, so the chain always terminates.
        while (renames.TryGetValue(id, out var next))
        {
            id = next;
        }
        return id;
    }

    /// <summary>Enumerates rename entries as previous → next identity, in no particular order.</summary>
    public IEnumerator<KeyValuePair<StableId, StableId>> GetEnumerator() => renames.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
