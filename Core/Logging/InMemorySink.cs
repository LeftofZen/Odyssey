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

		public ConcurrentQueue<GuiLog> Events { get; } = new ConcurrentQueue<GuiLog>();

		public void Emit(LogEvent logEvent)
		{
			if (logEvent == null)
			{
				throw new ArgumentNullException(nameof(logEvent));
			}

			var renderSpace = new StringWriter();
			_textFormatter.Format(logEvent, renderSpace);
			Events.Enqueue(new GuiLog(renderSpace.ToString(), logEvent.Timestamp, 5000));
		}
	}
}
