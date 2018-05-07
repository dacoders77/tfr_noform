using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace TFR_noform
{

	public static class TestMessage
	{
		private static string jsString; // String for JS
		private static List<string> elseMessages = new List<string>() { "Added", "Covered", "Shorted" }; // Collection other than bought and sold messages
		private static int addedHouh = 0;
		private static String symbol = "PHOT";

		public static void Inject(string messageType, Form1 form)
		{
			// US culture is used. Must be carefull with it! In DB Convert.ToDouble(command1.ExecuteScalar()) is used
			// It is sensetive to (,) and (.) symbold. If you turn all project into us-US culture sove feilds in DB like accumulated_sum_pcnt or
			// accumulated_balance can turn values 19.99999 to 1999999!
			// Experienced big trubble in DataBase.UpdateProfit() - Get the value of accumulated_sum_prcnt from the previous record then it will be used in a second query

			string appendedTime = form.parser.lastMessageDate.AddHours(++addedHouh).ToString("h:mm:ss tt"); // Add an hour to the date of last message and inject it to the page thus it will be read as a new one 

			switch (messageType)
			{
				case "bought":
					//Console.WriteLine("bought case detected");
					jsString = "var newItem = document.createElement('div'); newItem.style = ('background-color:green'); newItem.className = ('GLS-JUXDFAD'); newItem.innerHTML = ('<img src=\"./profitly_files/TimCover1_bigger.jpg\" width=50 height=50> 11/28 " + appendedTime + " - Bought 3000 of $" + symbol + " at 1.59 - text message'); var list = document.getElementById('x-auto-1'); list.insertBefore(newItem, list.childNodes[0]);";
					break;
				case "sold":
					//Console.WriteLine("sold case detected");
					jsString = "var newItem = document.createElement('div'); newItem.style = ('background-color:red'); newItem.className = ('GLS-JUXDFAD'); newItem.innerHTML = ('<img src=\"./profitly_files/TimCover1_bigger.jpg\" width=50 height=50> 11/28 " + appendedTime + " - Sold 2000 of $" + symbol + " at 1.60 - text message'); var list = document.getElementById('x-auto-1'); list.insertBefore(newItem, list.childNodes[0]);";
					break;
				case "else":
					//Console.WriteLine("else case detected");
					jsString = "var newItem = document.createElement('div'); newItem.style = ('background-color:gray'); newItem.className = ('GLS-JUXDFAD'); newItem.innerHTML = ('<img src=\"./profitly_files/TimCover1_bigger.jpg\" width=50 height=50> 11/28 " + appendedTime + " - " + elseMessages[(new Random()).Next(0, 3)] + " 2000 of $ELSESYMBOL at 2.24 - text message'); var list = document.getElementById('x-auto-1'); list.insertBefore(newItem, list.childNodes[0]);";
					break;
			}

			try
			{
				Form1.js.ExecuteScript(jsString);
				//Console.WriteLine("Test message injected to the page");
			}
			catch (Exception err)
			{
				ListViewLog.AddRecord(form, "parserListBox", "IngectTestMessage.cs", "Error: " + err, "red");
				//Console.WriteLine("JS error. Can't inject test message" + err);
			}

		}

	}
}
