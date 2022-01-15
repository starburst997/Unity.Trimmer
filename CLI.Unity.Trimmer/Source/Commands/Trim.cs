using System;
using System.IO;

namespace Unity.Trimmer.Cli.Commands
{
    public static class Trim
    {
        public static void Execute(string input, string output)
        {
            var bytes = File.ReadAllBytes(input);
            
            File.WriteAllText(output, "allo");
            
            Console.WriteLine($"Test !!! {bytes.Length}");
        }
    }
}