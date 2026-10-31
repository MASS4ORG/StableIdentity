namespace MASS4.StableIdentity.Tests;

public class StableIdTests
{
    [Test]
    public async Task RoundTripsTheCanonicalLowercaseHyphenatedForm()
    {
        var id = StableId.Parse("4D415353-0000-4000-8000-000000000001");

        await Assert.That(id.ToString()).IsEqualTo("4d415353-0000-4000-8000-000000000001");
        await Assert.That(StableId.Parse(id.ToString())).IsEqualTo(id);
        await Assert.That(StableId.TryParse(id.ToString(), out var parsed)).IsTrue();
        await Assert.That(parsed).IsEqualTo(id);
    }

    [Test]
    public async Task RejectsNonCanonicalText()
    {
        await Assert.That(StableId.TryParse("not-a-guid", out _)).IsFalse();
        await Assert.That(StableId.TryParse("{4d415353-0000-4000-8000-000000000001}", out _)).IsFalse();
        await Assert.That(StableId.TryParse(null, out _)).IsFalse();
        await Assert.That(() => StableId.Parse("4d41535300004000800000000000000001")).ThrowsExactly<FormatException>();
    }

    [Test]
    public async Task EmptyNewAndOrdering()
    {
        var a = new StableId(new Guid("00000000-0000-0000-0000-000000000001"));
        var b = new StableId(new Guid("00000000-0000-0000-0000-000000000002"));

        await Assert.That(StableId.Empty.IsEmpty).IsTrue();
        await Assert.That(StableId.New().IsEmpty).IsFalse();
        await Assert.That(a.CompareTo(b)).IsLessThan(0);
        await Assert.That(b.CompareTo(a)).IsGreaterThan(0);
    }
}
