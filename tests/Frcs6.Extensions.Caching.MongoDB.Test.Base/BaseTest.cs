using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Base;

[ExcludeFromCodeCoverage]
public abstract class BaseTest
{
    public const string DatabaseName = "TestDatabase";
    public const string CollectionName = "CacheCollection";

    protected IFixture Fixture => _fixture ?? CreateFixture();

    protected string DefaultKey { get; }
    protected byte[] DefaultValue { get; }
    protected CancellationToken DefaultToken { get; } = CancellationToken.None;
    protected DateTimeOffset UtcNow => _utcNow;
    protected MongoCacheOptions MongoCacheOptions { get; } = new() { DatabaseName = DatabaseName, CollectionName = CollectionName, AllowNoExpiration = true };

    private readonly FakeTimeProvider _timeProvider = new();
    protected TimeProvider TimeProvider => _timeProvider ;

    private Fixture? _fixture;
    private DateTimeOffset _utcNow;

    protected BaseTest()
    {
        DefaultKey = Fixture.Create<string>();
        DefaultValue = Fixture.CreateMany<byte>().ToArray();
        Fixture.Register(() => Options.Create(MongoCacheOptions));
        ConfigureUtcNow(DateTimeOffset.UtcNow);
        Fixture.Register(() => (TimeProvider)_timeProvider);
    }

    protected void ConfigureUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
        _timeProvider.SetUtcNow(utcNow);
    }

    private Fixture CreateFixture()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        return _fixture;
    }
}