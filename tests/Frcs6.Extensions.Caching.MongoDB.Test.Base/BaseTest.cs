using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Options;

#if NET8_0_OR_GREATER
using Microsoft.Extensions.Time.Testing;
#else
using Microsoft.Extensions.Internal;
using Moq;
#endif

namespace Frcs6.Extensions.Caching.MongoDB.Test.Base;

[ExcludeFromCodeCoverage]
public abstract class BaseTest
{
    protected const string DatabaseName = "TestDatabase";
    protected const string CollectionName = "CacheCollection";

    protected IFixture Fixture => _fixture ?? CreateFixture();

    protected string DefaultKey { get; }
    protected byte[] DefaultValue { get; }
    protected CancellationToken DefaultToken { get; } = CancellationToken.None;
    protected DateTimeOffset UtcNow => _utcNow;
    protected MongoCacheOptions MongoCacheOptions { get; } = new() { DatabaseName = DatabaseName, CollectionName = CollectionName, AllowNoExpiration = true };

#if NET8_0_OR_GREATER
    private readonly FakeTimeProvider _timeProvider = new();
#else
    private readonly Mock<ISystemClock> _timeProvider = new();
#endif

    private Fixture? _fixture;
    private DateTimeOffset _utcNow;

    protected BaseTest()
    {
        DefaultKey = Fixture.Create<string>();
        DefaultValue = Fixture.CreateMany<byte>().ToArray();
        Fixture.Register(() => Options.Create(MongoCacheOptions));
        ConfigureUtcNow(DateTimeOffset.UtcNow);
#if NET8_0_OR_GREATER
        Fixture.Register(() => (TimeProvider)_timeProvider);
#else
        Fixture.Register(() => _timeProvider.Object);
#endif
    }

    protected void ConfigureUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
#if NET8_0_OR_GREATER
        _timeProvider.SetUtcNow(utcNow);
#else
        _timeProvider.SetupGet(p => p.UtcNow).Returns(utcNow);
#endif
    }

    private Fixture CreateFixture()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        return _fixture;
    }
}