using Testcontainers.MongoDb;

namespace Frcs6.Extensions.Caching.MongoDB.Tests.Common;

public sealed class MongoDatabaseFixture : IDisposable
{
    private bool _isDisposed;
    private readonly MongoDbContainer _container;

    public MongoDatabaseFixture()
    {
        _container = new MongoDbBuilder().Build();
        _container.StartAsync().Wait();
    }

    ~MongoDatabaseFixture()
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