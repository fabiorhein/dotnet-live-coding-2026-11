namespace Claims.Tests;

public class MyTests
{
    private static ClaimsRepository Repo() => new(Db.CreateSeeded());

    private static long[] Ids(IEnumerable<Claim> claims) => claims.Select(c => c.Id).ToArray();

    [Fact]
    public void Search_FullDescription_ReturnsThatClaim()
    {
        Assert.Equal(new long[] { 101 }, Ids(Repo().Search(1, "Dental cleaning")));
    }

    [Fact]
    public void Search_PartialDescription_ReturnsMatchingClaimsNewestFirst()
    {
        Assert.Equal(new long[] { 103, 101 }, Ids(Repo().Search(1, "dental")));
    }

    [Fact]
    public void Search_ApostropheInProvider_FindsOBrien()
    {
        Assert.Equal(new long[] { 102 }, Ids(Repo().Search(1, "O'Brien")));
    }

    [Fact]
    public void Search_LiteralPercent_MatchesOnlyThatClaim()
    {
        Assert.Equal(new long[] { 104 }, Ids(Repo().Search(1, "100%")));
    }

    [Fact]
    public void Search_SortsByAmountAscending()
    {
        Assert.Equal(new long[] { 104, 105 }, Ids(Repo().Search(1, "City Pharmacy", "amount", "asc")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Search_NullOrEmptyText_ReturnsOnlyThatMembersClaims(string? text)
    {
        var ids = Ids(Repo().Search(1, text));
        Assert.Equal(new long[] { 105, 104, 103, 102, 101 }, ids);
        Assert.DoesNotContain(201L, ids);
    }

    [Fact]
    public void Search_WhitespaceText_ReturnsNoClaims()
    {
        Assert.Empty(Repo().Search(1, "   "));
    }

    [Fact]
    public void Search_TextAtMaxLength_ReturnsNoClaims()
    {
        Assert.Empty(Repo().Search(1, new string('a', 4000)));
    }

    [Fact]
    public void Search_OtherMemberClaim_IsNotReturned()
    {
        Assert.Empty(Repo().Search(1, "crown"));
        Assert.Equal(new long[] { 201 }, Ids(Repo().Search(2, "crown")));
    }

    [Theory]
    [InlineData("' OR 1=1--")]
    [InlineData("'; DROP TABLE Users--")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("../../etc/passwd")]
    [InlineData("_")]
    public void Search_UntrustedText_ReturnsNoRowsAndLeavesOtherMembersIntact(string text)
    {
        var repo = Repo();
        Assert.Empty(repo.Search(1, text));
        Assert.Equal(new long[] { 201 }, Ids(repo.Search(2, "Dental crown")));
    }

    [Fact]
    public void Search_LiteralPercentSign_DoesNotMatchEveryClaim()
    {
        Assert.Equal(new long[] { 104 }, Ids(Repo().Search(1, "%")));
    }

    [Theory]
    [InlineData("amount, (SELECT name FROM members)")]
    [InlineData("amount;DROP TABLE claims")]
    [InlineData("'; DROP TABLE Users--")]
    [InlineData("submitted_at")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../../etc/passwd")]
    public void Search_SortByNotAllowlisted_ThrowsArgumentException(string sortBy)
    {
        var repo = Repo();
        Assert.Throws<ArgumentException>(() => repo.Search(1, "dental", sortBy, "asc"));
        Assert.Equal(new long[] { 103, 101 }, Ids(repo.Search(1, "dental")));
    }

    [Theory]
    [InlineData("asc; DROP TABLE claims")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("<script>alert(1)</script>")]
    public void Search_SortDirNotAllowlisted_ThrowsArgumentException(string sortDir)
    {
        Assert.Throws<ArgumentException>(() => Repo().Search(1, "dental", "date", sortDir));
    }
}
