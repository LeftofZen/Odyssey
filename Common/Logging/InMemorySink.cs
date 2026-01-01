using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Odyssey.Logging
{
	public class InMemorySink : ILogEventSink
	{
		private readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}{Exception}");

		public ConcurrentQueue<LogEvent> Events { get; } = new ConcurrentQueue<LogEvent>();

		public void Emit(LogEvent logEvent)
		{
			if (logEvent == null)
			{
				throw new ArgumentNullException(nameof(logEvent));
			}

			Events.Enqueue(logEvent);
		}
	}
}
