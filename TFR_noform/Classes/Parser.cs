using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using IBApi;

namespace TFR_noform
{
	// Tracking messages on the page 
	public class Parser
	{
		public List<TraderMessage> tradeMessage = new List<TraderMessage> { }; // List of objects - TradeMessages
		public bool boughtFirstTIme = true; // Flag for passing through Bought message if it occures first in list. We need to start only form Sold message
		public bool soldFlag = true; // This flag is used for determin the series of sold messages: bought ones - sold few times. If so we add ticker only onece and increment volume each time
		public int messageSoldVolume = 0; // Value for sold stock quantity. 0 by default. If we get series of sold messages this value is increment each time and recorded to DB 

		public string messageTicker = "";
		public bool bougtMessageFlag = true; // Once recevied a Bought message - wait for Sold

		public bool firstRunFlag = true; // Flag for first run and getting the last date of the message

		public DateTime lastMessageDate; // Date of the last message
		private bool firstMessageDisplayed = false;

		private DateTime goToFavTabDateTime; // Variables for counting time for clicking on Favorite tab and going back to trades tab. Otherwise you will get disconnected after a while
		private DateTime goToTradesDateTime;
		private bool dateTimerFlag = true; // Record date for tab click flag  
		private bool startTracking = true; // False by default. When set to true - start tracking messages at the page

		private string message_text;
		private Form1 form;
		public Contract contractParser;

		public Parser(Form1 Form) // Constructor
		{
			form = Form;

			// Create a contract. The rest of the properties will be defined further in the code
			contractParser = new Contract();
			contractParser.SecType = "STK";
			contractParser.Currency = "USD";
			//In the API side, NASDAQ is always defined as ISLAND in the exchange field
			contractParser.Exchange = "SMART"; // SMART ISLAND
		}

		public void MessageSearch()
		{

			while (true) // Continues cycle. Each iteration reads the page content
			{
				System.Threading.Thread.Sleep(1000); // The delay between each page parse. 1000 - 1 sec
				//ListViewLogging.log_add(form, "GetAndTrackMessages.cs", "Page parsed at: " + DateTime.Now.ToString("hh.mm.ss.fff"), "white");

				form.BeginInvoke(new Action(delegate ()
				{
					form.progressBar1.Minimum = 0;
					form.progressBar1.Maximum = 100;
					form.progressBar1.Step = 10;

					form.label2.Text = DateTime.Now.ToString("hh.mm.ss.fff");
					if (form.progressBar1.Value == 100)
					{
						//form.progressBar1.CreateGraphics().DrawString(DateTime.Now.ToString("hh.mm.ss.fff").ToString(), new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black, new PointF(form.progressBar1.Width / 2 - 10, form.progressBar1.Height / 2 - 7));
						form.progressBar1.Value = 0;
					}
					form.progressBar1.PerformStep();
				}
				)); // Invoke

				if (firstMessageDisplayed)  // Last line is not cleared at the first run
				{
					//ListViewLogging.log_add(form, "GetAndTrackMessages.cs", "Console.SetCursorPosition(0, Console.CursorTop - 1); // Returned cursor to the previous line", "white");
					//Helpers.ClearCurrentConsoleLine(); // Clear last line with parsed time
				}

				firstMessageDisplayed = true;

				if (dateTimerFlag)
				{
					goToFavTabDateTime = DateTime.Now.AddSeconds(30);
					goToTradesDateTime = DateTime.Now.AddSeconds(32);
					dateTimerFlag = false; // When date is recorded set flag to false until going to the Favorites tab
				}

				if ((DateTime.Compare(DateTime.Now, goToFavTabDateTime) > 0) && startTracking)
				{
					goToFavTabDateTime = DateTime.Now;
					Navigation.FavoriteTabClick(); // Go to favorite tab and back in order to prevent outomatic logoff due to inactivity
				}

				if ((DateTime.Compare(DateTime.Now, goToTradesDateTime) > 0) && startTracking) // Go back to trades tab after a short delay
				{
					goToTradesDateTime = DateTime.Now;
					Navigation.TradesTabClick();
					dateTimerFlag = true;
				}

				int filter_couner = 0; // Counter for filtering messages. We need to get rid of messages with 2017 year mentined. There is no year in message and 12/28/17 date is considered the latest while parsing throught the whole page. Logic breaks in this case. Last message is the only one which is last on the screen.

				try
				{
					var InputString = Form1.ChromeDriver.FindElementsByClassName("GLS-JUXDFAD");

					foreach (var z in InputString) // Run cycle through all found elements on the page
					{
						try
						{
							Match match = Regex.Match(z.Text, @"(.+?PM|.+?AM)\s-\s(.+?)\s(\d+)\s.+?\s\$(.+)\sat\s(.+?)\s-\s(.+\s*.+)"); // Run through all found groups. Group - (). Regex online: https://regexr.com/																								
							string price = match.Groups[5].Value; // Need to get rid of (,) otherwise double.parse throws a error
							//price = price.Replace('.', ','); // , Replace. Can be used in differen cultures enviroment
							message_text = match.Groups[6].Value; // Need to get rid of (') because when sentence "i don't need it" goes to SQL query (') it interpreted as a an escape symbol
							message_text = message_text.Replace('\'', '*'); // Remove \ symbols from the parsed string. Otherwise - SQL error
							//message_text = message_text.Truncate_x(255); // Crop the string to not longer than 255 sybols. Otherwise - SQL error when string is longer than 255 symbols
							
							// Last message date detection. When new message appears - its date must be > than current. For this purpuse we need to record the date ol last detected message
							// ! = "" There are other empty classes on the page. There are no message in them. Pass them
							if (match.Groups[1].Value != "" && filter_couner < 5) // Read only 5 messages. Messgaes that are later than 5th belong to 2017. We do not need to read 2017
							{
								//Console.WriteLine("msg num***: " + filter_couner + "\nTRACE\nDate: " + match.Groups[1].Value + "\nDirection: " + match.Groups[2].Value + "\nQuantity: " + match.Groups[3].Value + "\nTicker: " + match.Groups[4].Value + "\nPrice: " + Double.Parse(price));
								filter_couner++;

								if (firstRunFlag)
								{
									lastMessageDate = DateTime.ParseExact(match.Groups[1].Value, "M/d h:mm:ss tt", CultureInfo.InvariantCulture);
									//Console.WriteLine("Tracking messages started. First run. Last message date: " + match.Groups[1].Value);
									ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "Tracking messages started. First run. Last message date: " + match.Groups[1].Value, "white");
									firstRunFlag = false;

									ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "Date added. New added message with date later than this is considered as new: " + lastMessageDate.AddMinutes(1).ToString("h:mm:ss tt"), "white");
									ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "Last message parsing example. This output must be the same as the message at the page. Is everything correct?\nDate: " + match.Groups[1].Value + "\nDirection: " + match.Groups[2].Value + "\nQuantity: " + match.Groups[3].Value + "\nTicker: " + match.Groups[4].Value + "\nPrice: " + Double.Parse(price), "white");

									//Console.WriteLine("Date added. New added message with date later than this is considered as new: " + GetAndTrackMMessages.lastMessageDate.AddMinutes(1).ToString("h:mm:ss tt")); // DateTime.Now.Date.ToString("MM/dd/yyyy")
									//Console.WriteLine("Last message parsing example. This output must be the same as the message at the page. Is everything correct?\nDate: " + match.Groups[1].Value + "\nDirection: " + match.Groups[2].Value + "\nQuantity: " + match.Groups[3].Value + "\nTicker: " + match.Groups[4].Value + "\nPrice: " + Double.Parse(price));
								}

								// New message detected. Message date > lastMessageDate
								DateTime h = DateTime.ParseExact(match.Groups[1].Value, "M/d h:mm:ss tt", CultureInfo.InvariantCulture);

								//Console.WriteLine("TRACE: DateTime.Compare(h, lastMessageDate) > 0: " + (DateTime.Compare(h, lastMessageDate) > 0) + ". h: " + h + ". lastMessageDate: " + lastMessageDate);
								if (DateTime.Compare(h, lastMessageDate) > 0)
								{
									ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "New message on the page is detected!", "white");
									lastMessageDate = DateTime.ParseExact(match.Groups[1].Value, "M/d h:mm:ss tt", CultureInfo.InvariantCulture);

									// Bought
									if (match.Groups[2].Value == "Bought" && bougtMessageFlag == true)
									{
										ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "********************ACTION: Bought", "green");
										bougtMessageFlag = false;
										messageTicker = match.Groups[4].Value;
										//form.textBox2.Text = messageTicker; // Error is thrown. When this line is uncommet - page can not be parsed using existing regex

										// DB Actions
										//DataBase.InsertTicker(match.Groups[4].Value); // Added ticker, record created then update it
										//DataBase.UpdateRecordOpenPosition("open", "long", DateTime.ParseExact(match.Groups[1].Value, "M/d h:mm:ss tt", CultureInfo.InvariantCulture), Double.Parse(price), 0, 0, 0, 0, 0, message_text);
										messageSoldVolume = Int32.Parse(match.Groups[3].Value);

										// Boroker actions

										//form.placeOrder.SendOrder("buy", match.Groups[4].Value); // Send BUY order to the exchange
										Email.Send("Bought action. Ticker: " + match.Groups[4].Value + ". Parsed price: " + Double.Parse(price));

										// Add ticker to the contract
										contractParser.Symbol = match.Groups[4].Value;

										// Call request market data method. Created contract with a ticker is passed as a parameter 
										form.reqMarketDataParser(contractParser);

									}

									//Console.WriteLine("TRACE: sold: " + match.Groups[2].Value + " ticker: " + messageTicker + " current ticker: " + match.Groups[4].Value + " boughMessageFlag: " + bougtMessageFlag);

									// Sold
									if (match.Groups[2].Value == "Sold" && messageTicker == match.Groups[4].Value && bougtMessageFlag == false)
									{
										ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "********************ACTION: Sold", "red");
										bougtMessageFlag = true;
										//form.textBox2.Text = "";

										// DB Actions 
										//DataBase.UpdateRecordClosePosition(DateTime.ParseExact(match.Groups[1].Value, "M/d h:mm:ss tt", CultureInfo.InvariantCulture), messageSoldVolume, Double.Parse(price), message_text);
										//DataBase.UpdateProfit(); // Uptade  and calculate values
										messageSoldVolume = 0; // Set volume to profit0. If Sold signal occured - we need to start increment volume from scratch

										// Boroker actions
										//form.placeOrder.SendOrder("sell", match.Groups[4].Value); // Send SELL order to the exchange
										Email.Send("Sold action. Ticker: " + match.Groups[4].Value + ". Parsed price: " + Double.Parse(price));

										// Add ticker to the contract
										contractParser.Symbol = match.Groups[4].Value;
										
										// Call request market data method. Created contract with a ticker is passed as a parameter 
										form.reqMarketDataParser(contractParser);
									}
								}
							}
						}
						catch
						{
							ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "Regex error. Nothing to parse or content can't be parsed using existing regex", "red");
						}
					}
				}
				catch // var InputString
				{
					ListViewLog.AddRecord(form, "parserListBox", "GetAndTrackMessages.cs", "Regex error. Chrome browser window is closed? Restart the program", "red");
				}
			}
		}

		/*
		// Function to crop string to 255 symbols. If longer - getting a error while adding value to DB
		public static string Truncate_x(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}
		*/
	}
}


