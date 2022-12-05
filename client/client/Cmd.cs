using System;
using System.Diagnostics;

namespace Alien
{
    class Cmd
    {
		public static string ShrinkCmdResult(string result)
		{
			string result2 = result;
			try
			{
				string text = string.Empty;
				string[] array = result.Split(new string[]
				{
					Environment.NewLine
				}, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i].Trim()
						.Replace("\t\t", "\t")
						.Replace("\t\t", "\t")
						.Replace("\t\t", "\t")
						.Replace("\t\t", "\t")
						.Replace("  ", " ")
						.Replace("  ", " ")
						.Replace("  ", " ")
						.Replace("  ", " ");
					if (!string.IsNullOrEmpty(text2))
					{
						if (!string.IsNullOrEmpty(text))
						{
							text += "\n";
						}
						text += text2;
					}
				}
				result2 = text;
			}
			catch (Exception ex)
			{
				Util.Log(ex.ToString());
			}
			return result2;
		}

		public static string ExecCmd(string cmd)
        {
            return Cmd.Exec("cmd", "/c " + cmd + " && exit");
        }

        public static string Exec(string fileName, string arguments)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName, arguments);
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            return process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        }
    }
}
