using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScriptOptimalizer
{
    class Debug
    {
        private const string FileName = "Console.log";

        public static void BadSyntax(char symbol)
        {
            throw new FormatException("Bad Syntax \'" + symbol.ToString() + "\' Line: " + Program.DEBUG_LineI);
        }

        public static void Error(string text)
        {
            WriteLine("[ERROR]: "+text, true);
        }

        public static void Warning(string text)
        {
            WriteLine("[WARNING]: "+text, true);
        }

        public static void Separator() { Separator(false); }
        public static void Separator(bool ignoreVerbose)
        {
            WriteLine("--------------------------", ignoreVerbose); 
        }

        public static void WriteLine(string text) { WriteLine(text, false); }
        public static void WriteLine(string text, bool ignoreVerbose)
        {
            if (!ignoreVerbose && !Program.A_Verbose)
                return;

            text = ProcessDepth(text);
            Console.WriteLine(text);
            Log(text + '\n');
        }

        public static void Write(string text) { Write(text, false); }
        public static void Write(string text, bool ignoreVerbose)
        {
            if (!ignoreVerbose && !Program.A_Verbose)
                return;

            Console.Write(text);
            Log(text);
        }

        private static string ProcessDepth(string text)
        {
            string fText = "";
            for (int i = 0; i < Program.DEBUG_IncludeDepth; i++)
                fText += "\t";

            return fText + text;           
        }

        static Debug()
        {
            LogDelete();
        }

        private static void Log(string text)
        {
            string fileDir = Directory.GetCurrentDirectory() + "\\" + FileName;

            FileStream fs = new FileStream(fileDir, FileMode.Append, FileAccess.Write, FileShare.None);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(text);

            sw.Close();
            fs.Close();
        }

        private static void LogDelete()
        {
            FileInfo fi = new FileInfo(Directory.GetCurrentDirectory() + "\\" + FileName);

            if (fi.Exists)
                fi.Delete();
        }
    }
}
