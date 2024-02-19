global using AutoFixture;
global using FluentAssertions;
global using FluentAssertions.Execution;
global using Frcs6.Extensions.Caching.MongoDB.Internal;
global using MongoDB.Driver;
global using Xunit;
global using Microsoft.Extensions.Caching.Distributed;
global using Moq;

#if NET8_0_OR_GREATER
global using Microsoft.Extensions.Time.Testing;
#else
global using Microsoft.Extensions.Internal;
#endif