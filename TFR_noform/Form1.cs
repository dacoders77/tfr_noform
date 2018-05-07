using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using IBApi;

using OpenQA.Selenium.Chrome; 
using OpenQA.Selenium;

namespace TFR_noform
{
	public partial class Form1 : Form
	{
		// Parser 
		public static ChromeDriver ChromeDriver = new ChromeDriver();
		public static IJavaScriptExecutor js = (IJavaScriptExecutor)ChromeDriver; // JS instance for JS execution 

		// Thread
		public static Thread messageTrackingThread;
		public Thread emailThread;

		// API variables
		private IBClient ibClient;
		private EReaderMonitorSignal signal;

		private bool isConnected = false; // Connection flag. Prevents connect button click when connected
		public Contract contract;

		public Parser parser;

		public Form1()
		{
			InitializeComponent();

			// listView1 setup
			listView1.View = View.Details;
			listView1.GridLines = true; // Horizoltal lines
			listView1.Columns.Add("Time:");
			listView1.Columns[0].Width = 60;
			listView1.Columns.Add("Source:", -2, HorizontalAlignment.Left);
			listView1.Columns.Add("Message:");
			listView1.Columns[2].Width = 400;

			// listView2 setup
			listView2.View = View.Details;
			listView2.GridLines = true; // Horizoltal lines
			listView2.Columns.Add("Time:");
			listView2.Columns[0].Width = 60;
			listView2.Columns.Add("Source:", -2, HorizontalAlignment.Left);
			listView2.Columns.Add("Message:");
			listView2.Columns[2].Width = 400;

			// IB API instances
			signal = new EReaderMonitorSignal();
			ibClient = new IBClient(signal);
			
			// Trade messages parser
			parser = new Parser(this);

			// Events
			ibClient.CurrentTime += IbClient_CurrentTime; // Exchange current time
			ibClient.MarketDataType += IbClient_MarketDataType; // Request market data type callback (Delaed, Frozen etc.)
			ibClient.Error += IbClient_Error; // Errors handling
			ibClient.TickPrice += IbClient_TickPrice; // reqMarketData
			ibClient.NextValidId += IbClient_NextValidId; // Fires when api is connected (connect button clicked)
			ibClient.OrderStatus += IbClient_OrderStatus; // Order status
		}

		private void IbClient_OrderStatus(IBSampleApp.messages.OrderStatusMessage obj) // Order status. Fires up when the order is executed
		{
			ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "Order status. AvgFillPrice: " + obj.AvgFillPrice, "white");
			Email.Send("Order status. AvgFillPrice: " + obj.AvgFillPrice);
		}

		private void IbClient_NextValidId(IBSampleApp.messages.ConnectionStatusMessage obj) // Api is connected
		{
			ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "API connected: " + obj.IsConnected, "white");
			isConnected = true;
			if (obj.IsConnected)
			{
				status_CT.Text = "Connected";
				button13.Text = "Disconnect";
			}
			// 1 - Realtime, 2 - Frozen, 3 - Delayed data, 4 - Delayed frozen
			ibClient.ClientSocket.reqMarketDataType(3); // https://interactivebrokers.github.io/tws-api/classIBApi_1_1EClient.html#ae03b31bb2702ba519ed63c46455872b6 
		}

		private void IbClient_TickPrice(IBSampleApp.messages.TickPriceMessage msg) // reqMktData event. Request market data event
		{
			//ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "TickPriceMessage. tick type: " + TickType.getField(msg.Field) + " price: " + msg.Price, "white");
			if (TickType.getField(msg.Field) == "delayedLast")
			{
				double calculatedVolume = Settings.useFunds / (double)msg.Price;
				ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "TickPriceMessage. Funds to use: " + Settings.useFunds + "$ Ticker: " + parser.contractParser.Symbol + " Price: " + msg.Price + " Calculated volume (not rounded): " + calculatedVolume, "white");
				Email.Send("Funds to use: " + Settings.useFunds + "$ Ticker: " + parser.contractParser.Symbol + " Price: " + msg.Price + " Calculated volume (no round): " + calculatedVolume);

				

				// SEND ORDER GOES HERE
				
			}
		}

		private void IbClient_Error(int arg1, int arg2, string arg3, Exception arg4) // Errors handling event
		{
			ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "IbClient_Error: arg 1,2,3: " + arg1 + " " + arg2 + " " + arg3 + "exception: " + arg4, "white");
		}

		private void IbClient_MarketDataType(IBSampleApp.messages.MarketDataTypeMessage obj) // Market data type request event
		{
			if (obj.MarketDataType == 3)
				ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", " IbClient_MarketDataType: " + obj.MarketDataType + ". Delayed market data type", "white");
		}

		private void IbClient_CurrentTime(long obj) // Current exchange time request event
		{
			ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "Exchange current time:" + UnixTimeStampToDateTime(obj).ToString(), "white");
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			ListViewLog.AddRecord(this, "parserListBox", "Form1.cs", "Current culture:" + CultureInfo.CurrentCulture.Name, "white");
			ListViewLog.AddRecord(this, "parserListBox", "Form1.cs", "Version: 05/05/2018 03:39PM", "white");

			ChromeDriver.Navigate().GoToUrl("file:///D:/1/profitly.html"); // Go to URL file:///D:/1/profitly.html https://profit.ly/profiding

		}

		private void button3_Click(object sender, EventArgs e) // Inject bought message
		{
			TestMessage.Inject("bought", this);
		}

		private void button4_Click(object sender, EventArgs e) // Inject Sold message
		{
			TestMessage.Inject("sold", this);
		}

		private void button5_Click(object sender, EventArgs e) // Inject other type message
		{
			TestMessage.Inject("else", this);
		}

		private void DelegateMethod() // new ThreadStart does not take any arguments. Local method passes the parameter - from
		{
			parser.MessageSearch();
		}

		private void button13_Click(object sender, EventArgs e) // Api connect
		{
			if (!isConnected) // False on startup
			{
				try
				{
					ibClient.ClientId = 1; // Client id. Multiple clients can be connected to the same gateway with the same login/password
					ibClient.ClientSocket.eConnect("127.0.0.1", Settings.ibGateWayPort, ibClient.ClientId);

					//Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
					var reader = new EReader(ibClient.ClientSocket, signal);
					reader.Start();

					//Once the messages are in the queue, an additional thread can be created to fetch them
					new Thread(() =>
					{ while (ibClient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } })
					{ IsBackground = true }.Start(); // https://interactivebrokers.github.io/tws-api/connection.html#gsc.tab=0
				}
				catch (Exception exception)
				{
					ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "Check your connection credentials. Exception: " + exception, "white");
				}

				try
				{
					ibClient.ClientSocket.reqCurrentTime();
				}
				catch (Exception exception)
				{
					ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "req time. Exception: " + exception, "white");
				}
			}
			else
			{
				isConnected = false;
				ibClient.ClientSocket.eDisconnect();
				status_CT.Text = "Disconnected";
				button13.Text = "Connect";
				ListViewLog.AddRecord(this, "brokerListBox", "Form1.cs", "Disconnecting..", "white");
			}
		}

		private void button12_Click(object sender, EventArgs e) // Get quote button click
		{
			contract = new Contract();
			contract.Symbol = textBox3.Text;
			contract.SecType = "STK";
			contract.Currency = "USD";
			//In the API side, NASDAQ is always defined as ISLAND in the exchange field
			contract.Exchange = "SMART"; // SMART ISLAND
			ibClient.ClientSocket.reqMktData((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, contract, "", true, false, null); ; // Request market data for a contract https://interactivebrokers.github.io/tws-api/classIBApi_1_1EClient.html#a7a19258a3a2087c07c1c57b93f659b63
		}

		public void reqMarketDataParser(Contract contractParser) // Request market data from Parser. Created contract with a ticker is passed as a parameter
		{
			// contractParser is created in Parser class and passed here as a parameter. It's ticker is parsed from the trade message on the page
			ibClient.ClientSocket.reqMktData((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, contractParser, "", true, false, null);
		}

		private void label5_Click(object sender, EventArgs e) // Listview clear link click
		{
			listView1.Clear();
			listView2.Clear();
		}

		private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
			return dtDateTime;
		}

		private void button1_Click(object sender, EventArgs e) // Start bot
		{
			messageTrackingThread = new Thread(new ThreadStart(DelegateMethod)); // A thread for message tracking. Message tracking exists in a parralell thread
			messageTrackingThread.IsBackground = true; // https://stackoverflow.com/questions/3360555/how-to-pass-parameters-to-threadstart-method-in-thread
			messageTrackingThread.Name = "MessageTrackingThread";
			messageTrackingThread.Start();
		}

		private void button2_Click(object sender, EventArgs e) // Stop bot button click
		{
			messageTrackingThread.Abort();
		}
	}
}
