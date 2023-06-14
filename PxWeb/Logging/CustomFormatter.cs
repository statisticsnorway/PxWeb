
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.IO;
using System;

namespace PxWeb.Logging;

public sealed class CustomFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private CustomOptions _formatterOptions;

    public CustomFormatter(IOptionsMonitor<CustomOptions> options)
        // Case insensitive
        : base("customName") =>
        (_optionsReloadToken, _formatterOptions) =
            (options.OnChange(ReloadLoggerOptions), options.CurrentValue);

    private void ReloadLoggerOptions(CustomOptions options) =>
        _formatterOptions = options;

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        string? message =
            logEntry.Formatter?.Invoke(
                logEntry.State, logEntry.Exception);

        if (message is null)
        {
            return;
        }
        
        CustomLogicGoesHere(textWriter);
        textWriter.WriteLine("severety:"+ logEntry.LogLevel+" Hei:"+message);
    }

    private void CustomLogicGoesHere(TextWriter textWriter)
    {
        textWriter.Write(_formatterOptions.CustomPrefix);
    }

    public void Dispose() => _optionsReloadToken?.Dispose();
}