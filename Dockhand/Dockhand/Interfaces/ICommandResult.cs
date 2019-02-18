namespace Dockhand.Interfaces
{
    internal interface ICommandResult
    {
        int ExitCode { get; }

        bool Success { get; }

        string StandardOutput { get; }

        string StandardError { get; }

    }
}