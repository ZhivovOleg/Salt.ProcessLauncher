namespace Salt.ProcessLauncher;

using System;

/// <summary>
/// Exception on run external command
/// </summary>
public class ProcessHelperException : Exception
{
    /// <inheritdoc/>
    public ProcessHelperException()
    {
    }

    /// <inheritdoc/>
    public ProcessHelperException(string message)
        : base(message)
    {
    }

    /// <inheritdoc/>
    public ProcessHelperException(string message, Exception inner)
        : base(message, inner)
    {
    }
}