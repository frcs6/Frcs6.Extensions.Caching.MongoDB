using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class CleanCacheJobsTest : BaseTest
{
    private readonly Mock<ICacheItemRepository> _cacheItemRepository;
    private readonly CleanCacheJobs _sut;

    public CleanCacheJobsTest()
    {
        _cacheItemRepository = Fixture.Freeze<Mock<ICacheItemRepository>>();
        MongoCacheOptions.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
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
        _sut.StartAsync(default);
#pragma warning disable CS8629 // Nullable value type may be null.
        Thread.Sleep(2 * MongoCacheOptions.RemoveExpiredDelay.Value);
#pragma warning restore CS8629 // Nullable value type may be null.
        _sut.StopAsync(default);

        _cacheItemRepository.Verify(r => r.RemoveExpired(true), Times.AtLeastOnce);
    }
}