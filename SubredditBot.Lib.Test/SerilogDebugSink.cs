using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace SubredditBot.Lib.Tests
{
    public class SerilogDebugSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        private static List<string> MemoryStore { get; set; } = new List<string>();

        public SerilogDebugSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            MemoryStore.Add(logEvent.RenderMessage(_formatProvider));
        }
    }

    public static class SerilogDebugSinkExtensions
    {
        public static LoggerConfiguration SerilogDebugSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new SerilogDebugSink(formatProvider));
        }
    }
}
