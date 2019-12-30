using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace src
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("准备更新存储过程，按Enter键确认开始……");
            var keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                var sw = Stopwatch.StartNew();
                Console.WriteLine("开始更新……");
                Console.WriteLine("---------------------------------------------------");
                new ProcUpdater().Run();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine($"更新完毕，耗时：{sw.ElapsedMilliseconds}ms");
            }
            Console.WriteLine("按任意键退出程序");
            Console.ReadKey();
        }
    }
}
