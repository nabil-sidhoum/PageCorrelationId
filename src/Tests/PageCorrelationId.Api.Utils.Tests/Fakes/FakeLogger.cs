using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace PageCorrelationId.Api.Utils.Tests.Fakes
{
    public class FakeLogger<T> : ILogger<T>
    {
        private readonly List<FakeLogEntry> _entries = [];

        public IReadOnlyList<FakeLogEntry> Entries => _entries;

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            _entries.Add(new FakeLogEntry(logLevel, formatter(state, exception), exception));
        }

        // ── Helpers privés ────────────────────────────────────────────────────

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();
            public void Dispose() { }
        }
    }

    public sealed class FakeLogEntry
    {
        public LogLevel Level { get; }
        public string Message { get; }
        public Exception Exception { get; }

        public FakeLogEntry(LogLevel level, string message, Exception exception)
        {
            Level = level;
            Message = message;
            Exception = exception;
        }
    }
}