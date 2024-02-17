namespace Frcs6.Extensions.Caching.MongoDB.Tests;

public class MongoCacheTest
{
    private readonly MongoCache _sut = new();

    [Fact]
    public void Test()
    {
        var act = () => _sut.Refresh("");
        act.Should().Throw<NotImplementedException>();
    }
}