using System.Diagnostics.CodeAnalysis;
using Testcontainers.MongoDb;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Base;

[ExcludeFromCodeCoverage]
public sealed class MongoDatabaseTest : IDisposable
{
    private bool _isDisposed;
    private readonly MongoDbContainer _container;

    public MongoDatabaseTest()
    {
        _container = new MongoDbBuilder("mongo:6.0").Build();
        _container.StartAsync().Wait();
    }

    ~MongoDatabaseTest()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            _container.StopAsync().Wait();
            _container.DisposeAsync().AsTask().Wait();
        }

        _isDisposed = true;
    }

    public string GetConnectionString() => _container.GetConnectionString();
}