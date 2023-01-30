using System;
using NetEti.Globals;
using FileChecker;
using System.Threading;
using Vishnu.Interchange;

namespace FileCheckerDemo
{
    public class Program
    {
        private static void Main(string[] args)
        {
            FileChecker.FileChecker FileChecker = new FileChecker.FileChecker();
            FileChecker.NodeProgressChanged += SubNodeProgressChanged;
            try
            {
                bool? res;
                string inKey = "";
                do
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo ki = Console.ReadKey();
                        inKey = ki.KeyChar.ToString().ToUpper();
                        switch (inKey)
                        {
                            default:
                                break;
                        }
                    }
                    Console.WriteLine();
                    //res = FileChecker.Run(@"SIZE|TestFiles\FileCheckerDemo.exe.config|>|162|Prüft, ob die FileCheckerDemo.exe.config > 162 byte ist.", new TreeParameters("MainTree", null), null);
                    //res = FileChecker.Run(@"COUNT|TestFiles\.*\..+|<|4|Prüft, ob mehr als drei Testfiles vorhanden sind.", new TreeParameters("MainTree", null), null);
                    res = FileChecker.Run(@"TRACE|TestFiles\.*\..+|<|S:15|Prüft, dass Test-Files nicht älter als 15 Sekunden werden.", new TreeParameters("MainTree", null), null);
                    //res = FileChecker.Run(@"AGE|TestFiles\FileCheckerDemo.exe.config|<!|d:3|Prüft, ob die FileCheckerDemo.exe.config älter als 3 Tage ist.", new TreeParameters("MainTree", null), null);
                    Console.WriteLine("---------------------------------------------------------------------------------");
                    Console.WriteLine("Ergebnis: {0}", (FileChecker.ReturnObject as FileCheckerReturnObject).ToString());
                    Thread.Sleep(5000);
                }
                while (inKey.ToString().ToUpper() == "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        private static void SubNodeProgressChanged(object sender, CommonProgressChangedEventArgs args)
        {
            Console.WriteLine("{0}: {1} von {2}", args.ItemName, args.CountSucceeded, args.CountAll);
        }
    }
}
