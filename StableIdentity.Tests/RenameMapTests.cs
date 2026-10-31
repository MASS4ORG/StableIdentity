namespace MASS4.StableIdentity.Tests;

public class RenameMapTests
{
    static StableId Id(int value) => new(new Guid(value, 0, 0, new byte[8]));

    [Test]
    public async Task CanonicalizeFollowsChainsAndLeavesUnrenamedIdentities()
    {
        var map = new RenameMap();
        map.Add(Id(1), Id(2));
        map.Add(Id(2), Id(3));

        await Assert.That(map.Canonicalize(Id(1))).IsEqualTo(Id(3));
        await Assert.That(map.Canonicalize(Id(3))).IsEqualTo(Id(3));
        await Assert.That(map.IsRenamed(Id(1))).IsTrue();
        await Assert.That(map.IsRenamed(Id(3))).IsFalse();
        await Assert.That(map.Count).IsEqualTo(2);
        await Assert.That(map.Select(static e => e.Key).Order().ToArray()).IsEquivalentTo([Id(1), Id(2)]);
    }

    [Test]
    public async Task AddRejectsEmptyConflictingAndCyclicRenames()
    {
        var map = new RenameMap();
        map.Add(Id(1), Id(2));
        map.Add(Id(2), Id(3));

        await Assert.That(() => map.Add(StableId.Empty, Id(3))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => map.Add(Id(4), StableId.Empty)).ThrowsExactly<ArgumentException>();
        await Assert.That(() => map.Add(Id(1), Id(4))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => map.Add(Id(3), Id(1))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => map.Add(Id(5), Id(5))).ThrowsExactly<ArgumentException>();
        await Assert.That(map.Count).IsEqualTo(2);
    }

    [Test]
    public async Task EnumeratesAsANonGenericSequence()
    {
        var map = new RenameMap();
        map.Add(Id(1), Id(2));

        var count = 0;
        foreach (var _ in (System.Collections.IEnumerable)map)
        {
            count++;
        }

        await Assert.That(count).IsEqualTo(1);
    }
}
