using log4net;
using log4net.Config;
using System.Reflection;

namespace Common.Logging
{
	public sealed class LogManagerService : ILoggerService
	{
		private static readonly object _sync = new();
		private static bool _configured = false;

		private static ILog Log
		{
			get
			{
				EnsureConfigured();
				// IMPORTANT: create logger AFTER config, and bind to the SAME repository
				var repo = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly() ?? typeof(LogManagerService).Assembly);
				return log4net.LogManager.GetLogger(repo.Name, typeof(LogManagerService));
			}
		}

		private static void EnsureConfigured()
		{
			if (_configured) return;

			lock (_sync)
			{
				if (_configured) return;

				var entryAsm = Assembly.GetEntryAssembly() ?? typeof(LogManagerService).Assembly;
				var repo = log4net.LogManager.GetRepository(entryAsm);

				var configPath = Path.Combine(AppContext.BaseDirectory, "log4net.config");
				if (!File.Exists(configPath))
				{
					// Optional: fallback to a minimal config so you still get something
					// BasicConfigurator.Configure(repo);
					_configured = true;
					return;
				}

				XmlConfigurator.ConfigureAndWatch(repo, new FileInfo(configPath));
				_configured = true;
			}
		}

		public void Info(string message) => Log.Info(message);
		public void Warn(string message) => Log.Warn(message);
		public void Error(string message, Exception? ex = null) => Log.Error(message, ex);
		public void Fatal(string message, Exception? ex = null) => Log.Fatal(message, ex);
	}
}
