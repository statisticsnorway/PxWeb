
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
        
        //CustomLogicGoesHere(textWriter);
        //textWriter.WriteLine("{severity: \"ERROR\", timestamp: \"2023-06-14T07:49:16.978372Z\", jsonPayload: {msg: \"my mess\" level: \"error\"}}");
        //textWriter.WriteLine("logtesting: {severity: \"ERROR\", timestamp: \"2023-06-14T07:49:16.978372Z\", jsonPayload: {msg: \"my mess\" level: \"error\"}}");
        textWriter.WriteLine("{\"severity\":\"ERROR\",\"message\":\"There was an error in the application.\", \"httpRequest\":{\"requestMethod\":\"GET\" }, \"times\":\"2020-10-12T07:20:50.52Z\", \"logging.googleapis.com/insertId\":\"42\",  \"logging.googleapis.com/labels\":{    \"user_label_1\":\"value_1\",    \"user_label_2\":\"value_2\"  },  \"logging.googleapis.com/operation\":{    \"id\":\"get_data\",    \"producer\":\"github.com/MyProject/MyApplication\",     \"first\":\"true\"  },  \"logging.googleapis.com/sourceLocation\":{    \"file\":\"get_data.py\",    \"line\":\"142\",    \"function\":\"getData\"  },  \"logging.googleapis.com/spanId\":\"000000000000004a\",  \"logging.googleapis.com/trace\":\"projects/my-projectid/traces/06796866738c859f2f19b7cfb3214824\",  \"logging.googleapis.com/trace_sampled\":false}");


        //textWriter.WriteLine("applog: { severety:\""+ logEntry.LogLevel+"\" , Hei:"+message+ "}");
    }

    private void CustomLogicGoesHere(TextWriter textWriter)
    {
        textWriter.Write(_formatterOptions.CustomPrefix);
    }

    public void Dispose() => _optionsReloadToken?.Dispose();
}