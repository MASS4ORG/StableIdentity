namespace MASS4.StableIdentity.Tests;

public class IdentityRegistryTests
{
    sealed record Entry(StableId Id, string DisplayName) : ITypeIdentity;

    static StableId Id(int value) => new(new Guid(value, 0, 0, new byte[8]));

    [Test]
    public async Task ResolveFollowsRenameChains()
    {
        var registry = new IdentityRegistry<Entry>();
        var current = new Entry(Id(3), "float");
        registry.Register(current);
        registry.AddRename(Id(1), Id(2));
        registry.AddRename(Id(2), Id(3));

        await Assert.That(registry.Canonicalize(Id(1))).IsEqualTo(Id(3));
        await Assert.That(registry.Resolve(Id(1))).IsSameReferenceAs(current);
        await Assert.That(registry.Count).IsEqualTo(1);
        await Assert.That(registry.Entries.Single()).IsSameReferenceAs(current);
        await Assert.That(registry.Renames.Count).IsEqualTo(2);
    }

    [Test]
    public async Task RegisterRejectsNullEmptyDuplicateAndRenamedIdentities()
    {
        var registry = new IdentityRegistry<Entry>();
        registry.Register(new(Id(1), "a"));
        registry.AddRename(Id(2), Id(1));

        await Assert.That(() => registry.Register(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => registry.Register(new(StableId.Empty, "empty"))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => registry.Register(new(Id(1), "again"))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => registry.Register(new(Id(2), "renamed"))).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task AddRenameRejectsRegisteredCyclicConflictingAndEmptyIdentities()
    {
        var registry = new IdentityRegistry<Entry>();
        registry.Register(new(Id(9), "registered"));
        registry.AddRename(Id(1), Id(2));

        await Assert.That(() => registry.AddRename(Id(2), Id(1))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => registry.AddRename(Id(1), Id(3))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => registry.AddRename(Id(9), Id(3))).ThrowsExactly<ArgumentException>();
        await Assert.That(() => registry.AddRename(StableId.Empty, Id(3))).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task ResolveThrowsForUnknownIdentity()
    {
        var registry = new IdentityRegistry<Entry>();

        await Assert.That(registry.TryResolve(Id(1), out _)).IsFalse();
        await Assert.That(() => registry.Resolve(Id(1))).ThrowsExactly<KeyNotFoundException>();
    }
}
