using System;

namespace ResvgSharp.Exceptions;

public class ResvgPngRenderException : ResvgException
{
    public ResvgPngRenderException()
    {
    }

    public ResvgPngRenderException(string message) : base(message)
    {
    }

    public ResvgPngRenderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}