global using Frcs6.Extensions.Caching.MongoDB.Internal.Compat;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Options;
global using MongoDB.Driver;
global using System;
global using System.Diagnostics.CodeAnalysis;

#if !NET8_0_OR_GREATER
global using Microsoft.Extensions.Internal;
#endif