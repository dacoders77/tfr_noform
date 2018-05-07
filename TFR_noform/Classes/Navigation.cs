using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFR_noform 
{
	class Navigation
	{

		public static void FavoriteTabClick()
		{
			try
			{
				Form1.ChromeDriver.FindElementByXPath("/html/body/div[4]/div[2]/div/div/div[2]/div[2]/div[2]/div[1]/div/div[1]/div[1]/ul/li[4]").Click();
			}
			catch (Exception err)
			{
				Console.WriteLine("FavTabClick(). FindElementByXPath. Element not found: " + err);
			}
		}

		public static void TradesTabClick()
		{
			try
			{
				Form1.ChromeDriver.FindElementByXPath("/html/body/div[4]/div[2]/div/div/div[2]/div[2]/div[2]/div[1]/div/div[1]/div[1]/ul/li[2]").Click();
			}
			catch (Exception err)
			{
				Console.WriteLine("TradesTabClick(). FindElementByXPath. Element not found: " + err);
			}
		}

	}
}
