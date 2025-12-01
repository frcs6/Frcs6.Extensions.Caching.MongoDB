using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class CleanCacheJobsTest : BaseTest
{
    private readonly Mock<ICacheItemRepository> _cacheItemRepository;
    private readonly CleanCacheJobs _sut;

    public CleanCacheJobsTest()
    {
        _cacheItemRepository = Fixture.Freeze<Mock<ICacheItemRepository>>();
        MongoCacheOptions.RemoveExpiredDelay = TimeSpan.FromSeconds(1);
        _sut = Fixture.Create<CleanCacheJobs>();
    }

    [Fact]
    public void GivenNullRemoveExpiredDelay_WhenCtor_ThenArgumentNullException()
    {
        MongoCacheOptions.RemoveExpiredDelay = null;
        var act = () => new CleanCacheJobs(_cacheItemRepository.Object, Options.Create(MongoCacheOptions));
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenJobs_WhenExecute_ThenRemoveExpired()
    {
        _sut.StartAsync(CancellationToken.None);
        Thread.Sleep(2 * MongoCacheOptions.RemoveExpiredDelay!.Value);
        _sut.StopAsync(CancellationToken.None);

        _cacheItemRepository.Verify(r => r.RemoveExpired(true), Times.AtLeastOnce);
    }
}