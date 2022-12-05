using System;
using System.Diagnostics;

namespace Alien
{
    class Cmd
    {
        public static string ShrinkCmdResult(string result)
        {
            return "";
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
