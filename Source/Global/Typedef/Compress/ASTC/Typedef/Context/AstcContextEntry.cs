// Astc Context Entry

using System;
using System.Threading;

public sealed class AstcContextEntry
{
public IntPtr Context;

public readonly Lock Locker = new();
}