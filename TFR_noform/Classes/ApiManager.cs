using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace TFR_noform
{
	class ApiManager
	{
		private IBClient iBClient;
		private Contract contract;
		private static Order order;

		public string symbol;
		public int volume;
		public string direction;

		public ApiManager(IBClient IBClient) // Constructor
		{
			iBClient = IBClient;

			// Contract
			contract = new Contract(); // A contract is created. Then while calling a class method contract.Symbol field is assigned
			//contract.Symbol = "AAPL";
			contract.SecType = "STK";
			contract.Currency = "USD";
			//In the API side, NASDAQ is always defined as ISLAND in the exchange field
			contract.Exchange = "SMART"; // SMART. If no exchange specefied - all availible exchanges will be listed

			// Order
			order = new Order();
			//if (orderId != 0)
			order.OrderId = 1;
			//order.Action = "BUY"; // BUY
			order.OrderType = "MKT"; // MARKET
									 //if (!lmtPrice.Text.Equals(""))
									 //order.LmtPrice = Double.Parse(lmtPrice.Text); // Limit price
									 //if (!quantity.Text.Equals(""))
									 //order.TotalQuantity = 1; // QUANTITY
									 //order.Account = account.Text;
									 //order.ModelCode = modelCode.Text;
									 //order.Tif = timeInForce.Text; // TIME IN FORCE DAY
									 //if (!auxPrice.Text.Equals(""))
									 //order.AuxPrice = Double.Parse(auxPrice.Text);
									 //if (!displaySize.Text.Equals(""))
									 //order.DisplaySize = Int32.Parse(displaySize.Text);
			order.Tif = "DAY";

		}

		public void Search(string symbol) // Ticker search
		{
			//contract.Symbol = symbol;
			//iBClient.ClientSocket.reqContractDetails((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, contract);
		}

		public void GetQuote(string symbol, int basketNum) // Get ticker qute
		{
			//basketNumber = basketNum;
			//symbolPass = symbol;

			//contract.Symbol = symbol;
			//iBClient.ClientSocket.reqMktData((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, contract, "", true, false, null); // Request market data for a contract https://interactivebrokers.github.io/tws-api/classIBApi_1_1EClient.html#a7a19258a3a2087c07c1c57b93f659b63
		}

		public void PlaceOrder() 
		{
			contract.Symbol = symbol;
			order.TotalQuantity = volume;
			order.Action = direction;

			//if (order.Action == "SELL")
			//{
			//	order.TotalQuantity = -order.TotalQuantity;
			//}

			Console.WriteLine("xxxxx: " + volume);

			iBClient.ClientSocket.placeOrder((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, contract, order);
		}
	}
}
