using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Collections.Concurrent;

namespace KPD.DAL
{
	internal class Logger
	{
		string logPath;
		private static ConcurrentDictionary<string, object> pathCollection = new ConcurrentDictionary<string, object>();

		private Logger()
		{
			logPath = GetParameterFromAppConfig("LogPath");
			pathCollection.TryAdd(logPath, new object());
		}

		private static string GetParameterFromAppConfig(string param)
		{
			string value = ConfigurationManager.AppSettings[param];
			if (String.IsNullOrEmpty(value))
			{
				//throw new Exception("value " + param + " wasn't found in app.config!");
				return @"C:\temp\log.txt";
			}
			else
			{
				return value;
			}
		}

		private static Logger instance;

		internal static Logger Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Logger();
				}
				return instance;
			}
		}

		internal void WriteToLog(string message)
		{
			lock (pathCollection[logPath])
			{
				using (StreamWriter writer = new StreamWriter(logPath, true))
				{
					writer.WriteLine(DateTime.Now + ": " + message);
					writer.Close();
				}
			}
		}
	}
}