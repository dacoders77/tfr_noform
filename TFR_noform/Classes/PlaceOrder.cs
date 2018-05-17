using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace TFR_noform.Classes
{

	// DELETE THIS CLASS! 

	public static class PlaceOrder
	{
		private static IBClient ibClient;
		private static Contract contract;
		private static Order order;

		public static void Send(string orderDirection, string symbol) // int volume
		{
			

			Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; // Unix time in milleseconds is used as an order id

			contract = new Contract(); // New instance of the contract class
			contract.Symbol = symbol;
			contract.SecType = "STK";
			contract.Exchange = "SMART"; //  ISLAND
			contract.Currency = "USD";
			contract.LocalSymbol = symbol;

			/* a sample contract from CS_Testbed
			Contract contract = new Contract();
			contract.Symbol = "IBKR";
			contract.SecType = "STK";
			contract.Currency = "USD";
			//In the API side, NASDAQ is always defined as ISLAND in the exchange field
			contract.Exchange = "ISLAND";
			*/
			
			order = new Order();
			//if (orderId != 0)
			order.OrderId = 1;
			//order.Action = "BUY"; // BUY
			order.OrderType = "MKT"; // MARKET
									 //if (!lmtPrice.Text.Equals(""))
									 //order.LmtPrice = Double.Parse(lmtPrice.Text); // Limit price
									 //if (!quantity.Text.Equals(""))
			order.TotalQuantity = 1; // QUANTITY
									 //order.Account = account.Text;
									 //order.ModelCode = modelCode.Text;
									 //order.Tif = timeInForce.Text; // TIME IN FORCE DAY
									 //if (!auxPrice.Text.Equals(""))
									 //order.AuxPrice = Double.Parse(auxPrice.Text);
									 //if (!displaySize.Text.Equals(""))
									 //order.DisplaySize = Int32.Parse(displaySize.Text);
			order.Tif = "DAY";

			if (orderDirection == "buy")
			{
				order.Action = "BUY"; // BUY
				ibClient.ClientSocket.placeOrder(unixTimestamp, contract, order);
			}

			if (orderDirection == "sell")
			{
				order.Action = "SELL"; // BUY    
				ibClient.ClientSocket.placeOrder(unixTimestamp, contract, order);
			}
		}
	}
}
