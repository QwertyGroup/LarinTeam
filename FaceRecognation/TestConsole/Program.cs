using FaceRecognition.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
	class Program
	{
		private static GroupManager _gm = GroupManager.GroupManagerInstance;
		static void Main(string[] args)
		{
			MessageManager.MsgManagerInstance.OnMessageSended += (s, e) => Console.WriteLine("Log >> " + e);
			Test();
			Console.ReadLine();
		}

		private static async void Test()
		{
			await _gm.Clear();
			await _gm.Train();
		}
	}
}
