using System;

namespace ResvgSharp.Exceptions;

public class ResvgParseException : ResvgException
{
    public ResvgParseException()
    {
    }

    public ResvgParseException(string message) : base(message)
    {
    }

    public ResvgParseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}