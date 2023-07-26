namespace Salt.ProcessLauncher;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

public static class ProcessLauncher
{
    private const string _errorTextTemplate = "{0} :: Command \"{1} {2}\" raise error:\r\n {3}";
    private const string _errorCodeTemplate = "{0} :: Command \"{1} {2}\" exited with exit code: '{3}'";

    /// <summary>
    /// TIP: For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError()'
    /// </summary>
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging"/>
    private static readonly Action<string, string, int> _raiseExceptionWithExitCode = (command, args, exitCode) =>
    {
        throw new ProcessHelperException(string.Format(CultureInfo.InvariantCulture, _errorCodeTemplate, DateTime.Now, command, args, exitCode));
    };
    /// <summary>
    /// TIP: For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError()'
    /// </summary>
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging"/>
    private static readonly Action<string, string, string> _raiseExceptionWithText = (command, args, error) =>
    {
        throw new ProcessHelperException(string.Format(CultureInfo.InvariantCulture, _errorTextTemplate, DateTime.Now, command, args, error));
    };

    private static readonly ProcessStartInfo _si = new()
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    private static void AddErrorEvent(Process process) =>
        process.ErrorDataReceived += (sender, error) =>
        {
            if (error.Data != null)
                _raiseExceptionWithText(process.StartInfo.FileName, process.StartInfo.Arguments, error.Data);
        };

    private static Process PrepareProcess(string command, string args)
    {
        Process process = new() { EnableRaisingEvents = true, StartInfo = _si };
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args.Replace("\"", "\\\"");
        AddErrorEvent(process);
        return process;
    }

    private static string ExecuteCommand(string command, string args)
    {
        using Process process = PrepareProcess(command, args);
        try
        {
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
                _raiseExceptionWithExitCode(command, args, process.ExitCode);
            return process.StandardOutput.ReadToEnd();
        }
        catch (Exception exc)
        {
            _raiseExceptionWithText(command, args, exc.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// Run console command in new Task. Raise exception when reseive data on StandartErrorOutput or command return ExitCode not "0".
    /// </summary>
    /// <param name="command">Console executable</param>
    /// <param name="args">Arguments</param>
    /// <returns>result</returns>
    /// <exception cref="System.Exception" />
    public static async Task<string> ExecuteAsync(string command, string args, CancellationToken ct = default) =>
        await Task.Run<string>(() => ExecuteCommand(command, args), ct);

    /// <summary>
    /// Run console command. Raise exception when reseive data on StandartErrorOutput or command return ExitCode not "0".
    /// </summary>
    /// <param name="command">Console executable</param>
    /// <param name="args">Arguments</param>
    /// <returns>result</returns>
    /// <exception cref="System.Exception" />
    public static string Execute(string command, string args) =>
        ExecuteCommand(command, args);
}