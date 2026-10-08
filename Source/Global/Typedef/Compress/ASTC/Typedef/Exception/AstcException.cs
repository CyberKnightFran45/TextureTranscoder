using System;

// Astc Exception

public sealed class AstcException(string message, AstcError error) : Exception(message)
{
public AstcError NativeError { get; } = error;
}