using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Cms;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class CleanCacheJobsTest : BaseTest
{
    private readonly Mock<ICacheItemRepository> _cacheItemRepository;
    private readonly CleanCacheJobs _sut;

    public CleanCacheJobsTest()
    {
        _cacheItemRepository = Fixture.Freeze<Mock<ICacheItemRepository>>();
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
        MongoCacheOptions.RemoveExpiredDelay = TimeSpan.FromSeconds(10);

        _sut.StartAsync(default);
        Thread.Sleep(MongoCacheOptions.RemoveExpiredDelay.Value);
        _sut.StopAsync(default);

        _cacheItemRepository.Verify(r => r.RemoveExpired(true), Times.AtLeastOnce);
    }
}