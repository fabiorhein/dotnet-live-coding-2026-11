namespace Claims.Tests;

// Tests written by the AI agent for MYT-3108.
public class AgentTests
{
    private static ClaimsRepository Repo() => new(Db.CreateSeeded());

    [Fact]
    public void Search_FindsClaimByDescription()
    {
        var ids = Repo().Search(1, "Dental cleaning").Select(c => c.Id);
        Assert.Equal(new long[] { 101 }, ids);
    }

    [Fact]
    public void Search_SortsByAmount()
    {
        var ids = Repo().Search(1, "City Pharmacy", "amount", "asc").Select(c => c.Id);
        Assert.Equal(new long[] { 104, 105 }, ids);
    }

    [Fact]
    public void Search_InjectionInText_ReturnsNothing()
    {
        Assert.Empty(Repo().Search(1, "' OR 1=1 --"));
    }

    [Fact]
    public void Search_PartialText_FindsClaimsContainingThatText()
    {
        // Product rule: "dental" matches "Dental cleaning" and "Dental X-ray".
        Assert.Equal(new long[] { 103, 101 }, Repo().Search(1, "dental").Select(c => c.Id));
    }
}
