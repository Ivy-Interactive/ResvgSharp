using System;

namespace ResvgSharp.Exceptions;

public class ResvgFontLoadException : ResvgException
{
    public ResvgFontLoadException()
    {
    }

    public ResvgFontLoadException(string message) : base(message)
    {
    }

    public ResvgFontLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}