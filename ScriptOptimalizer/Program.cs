using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// SCRIPT OPTIMALIZER
// 
// argument list
// -------------------------------------
// C -sourceDir="<source directory>"
// C -targetDir="<target directory>"
// C -profile="<profile name>"
// W -includeUse
// C -deleteComments
// C -comprime
// C -verbose
// C -ignoreModifyDate
// 
// 
// CODSCRIPT syntax
// C #define <constant name> <constant value>                       // normal definition
// C #replace <constant name> <constant value>                      // auto-replace
// C #include <file name>;                                          // normal include...
// W #includeuse <file name>;                                       // this will include only constants and function that will be used
// C foreach( <currentVariable> in <variablesArray> ) <command>;    // foreach directive from C# - only zero-based arrays
// C do <command>; while( <condition> );
//
//
// XML syntax
// C /// <Delete Profile="<profile name>"/>
//
//
// DEFAULT CONSTANTS
// __DATE__         // compile date '27. 3. 2013'
// __TIME__         // compile time '16:32'
// __FILE__         // file name '_ai'
// __FILEFULL__     // raw path + file name 'scripts\_ai'
// __LINE__         // current line number '73'
// __LINEFULL__     // current line '	if( !IsDefined( team ) )'
// __FUNCTION__     // current function name 'AI'
// __FUNCTIONFULL__ // current function name + signature 'AI( origin, angles )'
// __INT_MIN__      // min int number '-2147483647'
// __INT_MAX__      // max int number '2147483647'

namespace ScriptOptimalizer
{
    class Program
    {
        public static ReaderList A_Readers = new ReaderList();
        public static List<string> A_ScriptFiles = new List<string>();
        public static ConstantList A_Constants = new ConstantList();

        public static string A_SourceDir = Directory.GetCurrentDirectory();
        public static string A_TargetDir = "";
        public static string A_Profile = "";
        public static bool A_DeleteComments = false;
        public static bool A_Comprime = false;
        public static bool A_Verbose = false;
        public static bool A_IgnoreModifyDate = false;

        public static int DEBUG_LineI = 0;
        public static string DEBUG_Line = "";
        public static int DEBUG_IncludeDepth = 0;

        public static DateTime I_CompileTime = DateTime.Now;

        static void Main(string[] args)
        {
            if( args.Length == 0 )
                return;

            foreach (string s in args)
            {
                if (s.Contains("-sourceDir"))
                    A_SourceDir = s.Substring(s.IndexOf('=') + 1);
                else if (s.Contains("-targetDir"))
                    A_TargetDir = s.Substring(s.IndexOf('=') + 1);
                else if (s.Contains("-profile"))
                    A_Profile = s.Substring(s.IndexOf('=') + 1);
                else if (s.Contains("-deleteComments"))
                    A_DeleteComments = true;
                else if (s.Contains("-comprime"))
                    A_Comprime = true;
                else if (s.Contains("-verbose"))
                    A_Verbose = true;
                else if (s.Contains("-ignoreModifyDate"))
                    A_IgnoreModifyDate = true;
                else
                    Debug.Warning("Unknown exe arg \'"+s+"\'");
            }

            Debug.WriteLine("--- Settings ---");
            Debug.Separator();
            Debug.WriteLine("A_SourceDir: "+A_SourceDir);
            Debug.WriteLine("A_TargetDir: " + A_TargetDir);
            Debug.WriteLine("A_Profile: " + A_Profile);
            Debug.WriteLine("A_DeleteComments: " + A_DeleteComments);
            Debug.WriteLine("A_Comprime: " + A_Comprime);

            Debug.WriteLine( "--- Copying files ---", true );
            Debug.Separator(true);
            CopyDirectory(A_SourceDir, A_TargetDir, "*.iwd");
            CopyDirectory(A_SourceDir, A_TargetDir, "*.ff");
            CopyDirectory(A_SourceDir, A_TargetDir, "*.gsc");
            CopyDirectory(A_SourceDir, A_TargetDir, "*.cfg");

            ConstantList.InitPermanentConstants();

            ProcessScripts();

            if (A_Verbose)
                Console.ReadLine();
        }

        public static void CopyDirectory(string sourcePath, string targetPath, string searchOption)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            DirectoryInfo targetDir = new DirectoryInfo(targetPath);

            if (!targetDir.Exists)
                targetDir.Create();

            foreach (FileInfo sourceFile in sourceDir.GetFiles(searchOption))
                CopyFileByDates( targetDir, sourceFile );

            foreach (DirectoryInfo dir in sourceDir.GetDirectories())
                CopyDirectory(dir.FullName, targetPath + "\\" + dir.Name, searchOption);
        }

        private static void CopyFileByDates( DirectoryInfo targetDir, FileInfo sourceFile )
        {
            FileInfo testFile = new FileInfo(targetDir + "\\" + sourceFile.Name);
            if (!testFile.Exists)
            {
                ProcessFile(targetDir, sourceFile);
                return;
            }

            int state = DateTime.Compare(sourceFile.LastWriteTime, testFile.LastWriteTime);
            if ((!A_IgnoreModifyDate && state > 0) || A_IgnoreModifyDate)
            {
                ProcessFile(targetDir, sourceFile);
                return;
            }
        }

        private static void ProcessFile(DirectoryInfo targetDir, FileInfo sourceFile)
        {
            try
            {
                string sourceFilePath = sourceFile.FullName.Remove(0, A_SourceDir.Length + 1);
                string sourceFilePathNoExt = sourceFilePath.Remove(sourceFilePath.Length - 4, 4);

                Debug.WriteLine("Copying file \t" + sourceFilePath, true);
                sourceFile.CopyTo(targetDir + "\\" + sourceFile.Name, true);

                if (sourceFile.Extension == ".gsc")
                {
                    A_ScriptFiles.Add(sourceFilePathNoExt);
                    //Reader r = A_Readers.GetReader(sourceFilePathNoExt);
                    //r.WriteFile();
                }
            }
            catch (Exception e)
            {
                Debug.Error(e.Message);
            }
        }

        private static void ProcessScripts()
        {
            if (A_ScriptFiles.Count > 0)
            {
                Debug.Separator(true);
                Debug.Separator(true);
                Debug.WriteLine("--- Processing files ---", true);

                foreach (string file in A_ScriptFiles)
                    A_Readers.GetReader(file, 0);
            }
        }
    }
}
