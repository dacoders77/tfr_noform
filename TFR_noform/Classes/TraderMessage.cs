using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFR_noform
{
	public class TraderMessage
	{
		// Auto-implementer properties
		public DateTime TradeMessageDate { get; set; }
		public string TradeMessageDirection { get; set; }
		public int TradeMessageStockQuantity { get; set; }
		public string TradeMessageStockTicker { get; set; }
		public double TradeMessagePrice { get; set; }
		public string TradeMessageText { get; set; }
	}
}
