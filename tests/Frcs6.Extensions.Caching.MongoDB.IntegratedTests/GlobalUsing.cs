global using AutoFixture;
global using FluentAssertions;
global using Frcs6.Extensions.Caching.MongoDB.Internal;
global using MongoDB.Driver;
global using Xunit;

#if NET8_0_OR_GREATER
global using Microsoft.Extensions.Time.Testing;
#else
global using Moq;
global using Microsoft.Extensions.Internal;
#endif