# Right way to run external app (for example bash command) from C#

Use static class ProcessHelper:

```csharp
    try
    {
        string result = await ProcessLauncher.RunAsync("dotnet", "build");
        Console.WriteLine(result);
    }
    catch (Exception exc)
    {
        Console.WriteLine(exc.Message);
    }

```