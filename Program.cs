using System;
using System.IO;
using System.Linq;

namespace HoloArchiver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("HoloArchiver v1.0");
            if (args.Length != 1)
            {
                Console.WriteLine("usage: h3d.exe file.h3d");
                Console.ReadLine();
                return;
            }
            string file = args[0];
            string fileName = Path.GetFileNameWithoutExtension(file);
            using (var archive = SimpleArchive.OpenOrCreate(file))
            {
                if (args.Length == 1)
                {
                    Directory.CreateDirectory(fileName);
                    foreach (var entryName in archive.GetEntryNames())
                    {
                        Console.WriteLine(entryName);
                        File.WriteAllBytes(fileName + "\\" + entryName, archive.GetEntry(entryName));
                    }
                    Console.WriteLine("All files extracted.");
                }
            }
            Console.ReadLine();
        }
    }
}