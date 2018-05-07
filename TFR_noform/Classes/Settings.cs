using System;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Schema;

namespace TFR_noform
{
	public static class Settings
	{
		public static int ibGateWayPort { get; }
		public static string smtpLogin { get; }
		public static string smtpPassword { get; }
		public static string emailFromName { get; }
		public static string emailFromEmail { get; }
		public static string emailTo { get; }
		public static string emailToName { get; }
		public static string emailCopy { get; }
		public static string emailCopyName { get; }
		public static int useFunds { get; }

		static Settings()
		{
			try
			{
				string jsonFromFile = File.ReadAllText("tfr_settings.json"); // File must be located in Debug folder or next whre .exe is placed

				JSchema jsonParsed = JSchema.Parse(jsonFromFile);
				var x = Newtonsoft.Json.Linq.JObject.Parse(jsonFromFile);

				ibGateWayPort = (int)x["ibGateWayPort"]; // Type cast 
				smtpLogin = (string)x["smtpLogin"];
				smtpPassword = (string)x["smtpPassword"];
				emailFromName = (string)x["emailFromName"];
				emailFromEmail = (string)x["emailFromEmail"];
				emailTo = (string)x["emailTo"];
				emailToName = (string)x["emailToName"];
				emailCopy = (string)x["emailCopy"];
				emailCopyName = (string)x["emailCopyName"];
				useFunds = (int)x["useFunds"];
			}
			catch (Exception exception)
			{
				MessageBox.Show("Form1.cs. Error opening Setting.json: " + exception.Message);
			}

		}
	}
}
