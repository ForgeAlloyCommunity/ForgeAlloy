using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Forge.ForgeAlloyUnity.Assets.ForgeNetworking.Utilities
{
	public class SampleUtil
	{
		public static T PromptInput<T>(string message, string defaultValue = "")
		{
			string response = string.Empty;
			while (string.IsNullOrEmpty(response))
			{
				Console.WriteLine(message);
				response = Console.ReadLine().Trim();
				if (string.IsNullOrEmpty(response) && !string.IsNullOrEmpty(defaultValue))
				{
					response = defaultValue;
					Console.WriteLine(response);
				}
			}

			var converter = TypeDescriptor.GetConverter(typeof(T));
			if (converter != null)
			{
				try
				{
					return (T)converter.ConvertFrom(response);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			throw new Exception("Invalid Input");
		}
	}
}
