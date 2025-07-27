using System;

namespace ResvgSharp.Exceptions;

public class ResvgException : Exception
{
    public ResvgException()
    {
    }

    public ResvgException(string message) : base(message)
    {
    }

    public ResvgException(string message, Exception innerException) : base(message, innerException)
    {
    }
}