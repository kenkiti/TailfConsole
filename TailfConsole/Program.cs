using System;
using System.Text;
using System.IO;

namespace tail
{
    class Program
    {
        static void Main(String[] args)
        {
            Action<String[]> f = Usage;
            if (args.Length == 0) {
                args = new String[] { @"\\MARKETSPEED005\log\" + DateTime.Now.ToString("yyyyMMdd") + "cs.txt" };
            }
            if (File.Exists(args[0]))
            {
                f = tail;
                Console.WriteLine(args[0]);
            }
            f.Invoke(args);
        }

        /// <summary>
        /// 使い方
        /// </summary>
        /// <param name="args"></param>
        static void Usage(String[] args)
        {
            Console.WriteLine("Error: File not found.");
            Console.WriteLine("Usage: tail(-f param type only) fileanme");
            Console.ReadKey();
        }

        /// <summary>
        /// コンソール版 tailf.exe
        /// </summary>
        /// <param name="args">監視対象のファイルパス</param>
        static void tail(String[] args)
        {
            FileInfo fi = new FileInfo(args[0]);
            using (FileStream stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Boolean state = false;
                int size = (int)fi.Length;
                stream.Seek(size, SeekOrigin.Current);
                using (FileSystemWatcher fw = new FileSystemWatcher(fi.DirectoryName, fi.Name))
                {
                    #region "イベントハンドラー用メソッド"
                    Action<Object, FileSystemEventArgs> ReadText = (sender, e) =>
                    {
                        fi.Refresh();
                        Byte[] al = new Byte[] { };
                        int RemainingSize = (int)fi.Length - size;
                        if (RemainingSize <= 0) return;
                        Array.Resize<Byte>(ref al, RemainingSize);
                        int result = stream.Read(al, 0, RemainingSize);
                        size = (int)fi.Length;
                        Console.Write(Encoding.GetEncoding("sjis").GetString(al));
                    };
                    Action<object, ConsoleCancelEventArgs> EventStop = (sender, e) =>
                    {
                        fw.EnableRaisingEvents = false;
                        state = true;
                    };
                    #endregion
                    fw.NotifyFilter = NotifyFilters.Size;
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(EventStop);
                    fw.Changed += new FileSystemEventHandler(ReadText);
                    fw.EnableRaisingEvents = true;
                    while (!state) ;
                }
            }
            Console.WriteLine("\nStopped.");
        }
    }
}