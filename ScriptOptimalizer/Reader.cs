using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScriptOptimalizer
{
    class Reader
    {
        bool SETTINGS_DeleteComments = Program.A_DeleteComments;
        bool SETTINGS_ComprimeCode = Program.A_Comprime;
        int SETTINGS_ComprimeCodePerLine = 2;
        string SETTINGS_Profile = Program.A_Profile;
        //const bool SETTINGS_ReadOnly = false;

        bool C_Writed = false;
        public int C_IncludeDepth;

        string C_FileNameFull = "";
        public string C_FileName = "";
        string C_FileNameShort = "";
        string C_SourceStr = "";
        string C_FinalStr = "";

        bool STATE_InComment = false;

        bool STATE_PreCompileArg = false;

        ConstantList LIST_Constants = new ConstantList();
        ReplaceList LIST_Replaces = new ReplaceList();
        FunctionList LIST_Functions = new FunctionList();
        public IncludeList LIST_Includes = new IncludeList();
        List<XMLArg> LIST_XMLArgs = new List<XMLArg>();

        bool STATE_InFunction = false;
        string STATE_FunctionName = "";
        string STATE_FunctionSignature = "";
        string STATE_FunctionContent = "";

        bool STATE_InBlock = false;
        List<int> STATE_BlockStart = new List<int>();

        bool STATE_InRoundBracket = false;
        List<int> STATE_RoundBracketStart = new List<int>();

        bool STATE_WhiteSpace = false;
        bool STATE_InString = false;

        bool STATE_InControlArg = false;

        int ARG_GoToEnd = 0;
        int ARG_IgnoreChar = 0;

        string P_Line;
        int P_LineI = 0;
        string P_LinePrev;
        string P_LineNext;
        int P_Char = 0;
        int P_CharI = 0;
        int P_CharPrev = 0;
        int P_CharNext = 0;
        int P_CharNextBlack = 0;

        public Reader(string fileName, int incDepth)
        {
            C_IncludeDepth = incDepth;
            Program.DEBUG_IncludeDepth = C_IncludeDepth;

            C_FileName = fileName;
            C_FileNameFull = Program.A_TargetDir + "\\" + fileName + ".gsc";

            int index = C_FileName.LastIndexOf('\\');
            if (index == -1)
                C_FileNameShort = C_FileName;
            else
                C_FileNameShort = C_FileName.Substring(index+1, C_FileName.Length - index - 1);

            if (File.Exists(C_FileNameFull))
            {
                ReadFile();

                ScriptReader();

                WriteFile();
            }
            else
                Debug.Error("Could not open file \'"+C_FileName+"\'");

            if (Program.DEBUG_IncludeDepth >= 1)
                Program.DEBUG_IncludeDepth--;
        }

        private void ScriptReader()
        {
            P_LinePrev = "";
            P_Line = ReaderHelper.GetLine(C_SourceStr, 0);
            P_LineNext = ReaderHelper.GetNextLine(C_SourceStr, 0, P_Line);

            while( true )
            {
                if (P_CharI == C_SourceStr.Length)
                    break;

                if (P_CharI != 0)
                    P_CharPrev = C_SourceStr[P_CharI - 1];

                P_Char = C_SourceStr[P_CharI];

                if (P_CharI + 1 != C_SourceStr.Length)
                    P_CharNext = C_SourceStr[P_CharI + 1];
                else
                    P_CharNext = 0;

                P_CharNextBlack = ReaderHelper.GetNextBlackChar(C_SourceStr, P_CharI + 1);

                if (P_Char == '\n')
                {
                    P_LinePrev = P_Line;
                    P_LineI++;
                    P_Line = ReaderHelper.GetLine(C_SourceStr, P_CharI + 1);
                    P_LineNext = ReaderHelper.GetNextLine(C_SourceStr, P_CharI + 1, P_Line);
                }

                Program.DEBUG_Line = P_Line;
                Program.DEBUG_LineI = P_LineI;

                if (ARG_GoToEnd == 0) ReadString();
                if (ARG_GoToEnd == 0) ReadComments();
                if (ARG_GoToEnd == 0) CheckXMLArgs();
                if (ARG_GoToEnd == 0) ReadControlSymbols();
                if (ARG_GoToEnd == 0) ReadPreCompileArgs();
                if (ARG_GoToEnd == 0) ReadFunctionDefinitions();
                if (ARG_GoToEnd == 0) ReplaceConstants();
                if (ARG_GoToEnd == 0) ReadControlArgs();
                if (ARG_GoToEnd == 0) ReadDoWhile();
                if (ARG_GoToEnd == 0) ComprimeCode();

                if (ARG_IgnoreChar == 0)
                {
                    C_FinalStr += ((char)P_Char).ToString();
                    charsInLine++;
                }

                if (ARG_IgnoreChar > 0) ARG_IgnoreChar--;
                if (ARG_GoToEnd > 0) ARG_GoToEnd--;
                P_CharI++;
            }

            ReplaceReplaces();

            //LIST_Functions.ToString();
            LIST_Constants.ToString();
            LIST_Replaces.ToString();
            LIST_Includes.ToString();
        }

        private void ReadString()
        {
            //if (STATE_InComment) return;

            if (P_Char == '"' /*&& P_CharPrev != '\\'*/)
            {
                STATE_InString = !STATE_InString;
                //ARG_GoToEnd = 1;
                return;
            }
        }

        bool CommentMulti = false;
        bool CommentSingle = false;
        bool XMLComment = false;
        private void ReadComments()
        {
            bool specialChar = false;

            if ((CommentSingle || XMLComment) && P_Char == '\n')
            {
                specialChar = true;
                STATE_InString = false;

                if (CommentSingle)
                    CommentSingle = false;

                if (XMLComment)
                    XMLComment = false;
                //ARG_GoToEnd = 1;

                //if (SETTINGS_DeleteComments)
                //ARG_IgnoreChar = 1;
            }

            if (!STATE_InString && CommentMulti && P_Char == '*' && P_CharNext == '/')
            {
                specialChar = true;

                CommentMulti = false;
                ARG_GoToEnd = 2;

                if (SETTINGS_DeleteComments)
                    ARG_IgnoreChar = 2;
            }

            if (!STATE_InString && P_Char == '/' && P_CharNext == '*')
            {
                specialChar = true;

                ARG_GoToEnd = 2;
                CommentMulti = true;

                if (SETTINGS_DeleteComments)
                    ARG_IgnoreChar = 2;
            }

            if (!STATE_InString && P_Char == '/' && P_CharNext == '/')
            {
                specialChar = true;

                if (P_CharI + 2 < C_SourceStr.Length && C_SourceStr[P_CharI + 2] == '/')
                {
                    ARG_GoToEnd = 3;
                    XMLComment = true;

                    ReadXMLArg(P_Line.Substring(P_Line.IndexOf("///") + 3));

                    if (SETTINGS_DeleteComments)
                        ARG_IgnoreChar = 3;
                }
                else
                {
                    ARG_GoToEnd = 2;
                    CommentSingle = true;

                    if (SETTINGS_DeleteComments)
                        ARG_IgnoreChar = 2;
                }
            }

            if (CommentSingle || CommentMulti || XMLComment)
                STATE_InComment = true;
            else
                STATE_InComment = false;

            if (STATE_InComment)
            {
                if (SETTINGS_DeleteComments && !specialChar)
                    ARG_IgnoreChar = 1;
            }
        }

        private void ReadXMLArg(string line)
        {
            if (ReaderHelper.GetNextBlackChar(line, 0) != '<')
                return;

            if (line.IndexOf("/>") == -1) // start arg
            {
                int argStartIndex = line.IndexOf('<') + 1;
                int argEndIndex = line.IndexOf(' ', argStartIndex);
                string argName = line.Substring(argStartIndex, argEndIndex - argStartIndex);

                List<XMLAttribute> atts = new List<XMLAttribute>();
                int attEquals = 0;
                while (true)
                {
                    attEquals = line.IndexOf('=', attEquals + 1);
                    if (attEquals == -1)
                        break;

                    int attNameStart = line.LastIndexOf(' ', attEquals) + 1;
                    string attName = line.Substring(attNameStart, attEquals - attNameStart);

                    int attValueEnd = line.IndexOf('"', attEquals + 2);
                    string attValue = line.Substring(attEquals + 2, attValueEnd - (attEquals + 2));
                    atts.Add(new XMLAttribute(attName, attValue));
                }

                LIST_XMLArgs.Add(new XMLArg(argName, atts));
            }
            else
            {
                int argStartIndex = line.IndexOf('<') + 1;
                int argEndIndex = line.IndexOf("/>", argStartIndex);
                string argName = line.Substring(argStartIndex, argEndIndex-argStartIndex);

                int argIndex = LIST_XMLArgs.FindLastIndex(a => a.Name == argName);
                LIST_XMLArgs.RemoveAt(argIndex);
            }
        }

        private void CheckXMLArgs()
        {
            if (LIST_XMLArgs.Count == 0) return;

            int index = LIST_XMLArgs.FindIndex(a => (a.Name == "Delete" && a.Attributes[0].Value == Program.A_Profile));
            if (index != -1)
            {
                if (ARG_GoToEnd < 1)
                    ARG_GoToEnd = 1;

                if (ARG_IgnoreChar < 1)
                {
                    if (!(P_Char == '\n' || P_Char == '\t'))
                        ARG_IgnoreChar = 1;
                }

                ComprimeCode();
            }
        }

        string normalText = "";
        private void ReadControlSymbols()
        {
            if (STATE_InString) return;
            if (STATE_InComment) return;

            switch (P_Char)
            { 
                case '{':
                    STATE_BlockStart.Add(P_CharI);
                    STATE_InBlock = true;

                    break;
                case '}':
                    if (STATE_BlockStart.Count == 0)
                        Debug.BadSyntax((char)P_Char);

                    STATE_BlockStart.RemoveAt(STATE_BlockStart.Count-1);
                    if (STATE_BlockStart.Count == 0)
                        STATE_InBlock = false;

                    break;
                case '(':
                    STATE_RoundBracketStart.Add(P_CharI);
                    STATE_InRoundBracket = true;

                    break;
                case ')':
                    if (STATE_RoundBracketStart.Count == 0)
                        Debug.BadSyntax((char)P_Char);

                    STATE_RoundBracketStart.RemoveAt(STATE_RoundBracketStart.Count - 1);
                    if (STATE_RoundBracketStart.Count == 0)
                        STATE_InRoundBracket = false;

                    break;
                default:
                    break;
            }

            if (Char.IsWhiteSpace((char)P_Char))
                STATE_WhiteSpace = true;
            else
                STATE_WhiteSpace = false;

            if (((P_Char >= 'A' && P_Char <= 'Z')
            || (P_Char >= 'a' && P_Char <= 'z')
            || P_Char == '_')
            && ((normalText == "" && !(P_Char >= '0' && P_Char <= '9')) || normalText != ""))
                normalText += ((char)P_Char).ToString();
            else
                normalText = "";
        }

        string argType = "";
        bool argTypeFinished = false;
        bool ignorePreCompileArg = false;
        bool definePartFinished = false;
        private void ReadPreCompileArgs()
        {
            if (STATE_InComment) return;

            if (!STATE_InString)
            {
                if (P_Char == '#')
                {
                    STATE_PreCompileArg = true;
                    ignorePreCompileArg = true;

                    argType = "";
                    argTypeFinished = false;
                }
                else if (STATE_PreCompileArg && !STATE_WhiteSpace && !argTypeFinished)
                    argType += ((char)P_Char).ToString();
                else if (STATE_PreCompileArg && !argTypeFinished)
                {
                    argTypeFinished = true;
                    definePartFinished = true;
                }
            }

            if (STATE_PreCompileArg && argTypeFinished)
            {
                switch (argType)
                {
                    case "define":
                        ReadConstants();
                        ignorePreCompileArg = true;
                        break;
                    case "replace":
                        ReadReplaces();
                        ignorePreCompileArg = true;
                        break;
                    case "include":
                        ReadIncludes();
                        ignorePreCompileArg = false;
                        break;
                    case "includeuse":
                        ignorePreCompileArg = false;
                        break;
                    default:
                        ignorePreCompileArg = false;
                        break;
                }
            }

            if (STATE_PreCompileArg && ignorePreCompileArg)
            {
                ARG_GoToEnd++;
                ARG_IgnoreChar++;
            }
            else if (STATE_PreCompileArg && !ignorePreCompileArg && definePartFinished)
            {
                definePartFinished = false;
                C_FinalStr += "#" + argType;
            }
        }

        string constName = "";
        bool constNameFinished = false;
        string constValue = "";
        private void ReadConstants()
        {
            if (!STATE_InString && normalText != "" && !constNameFinished)
                constName = normalText;
            else if (!STATE_InString && STATE_WhiteSpace && !constNameFinished && constName != "")
                constNameFinished = true;
            else if (constNameFinished && (STATE_InString || !STATE_WhiteSpace))
                constValue += ((char)P_Char).ToString();
            else if (P_Char == '\n')
            {
                LIST_Constants.Add(constName, constValue);

                constName = "";
                constNameFinished = false;
                constValue = "";
                STATE_PreCompileArg = false;
            }
        }

        string replaceName = "";
        bool replaceNameFinished = false;
        string replaceValue = "";
        private void ReadReplaces()
        {
            if (!STATE_InString && normalText != "" && !replaceNameFinished)
                replaceName = normalText;
            else if (!STATE_InString && STATE_WhiteSpace && !replaceNameFinished && replaceName != "")
                replaceNameFinished = true;
            else if (replaceNameFinished && (STATE_InString || !STATE_WhiteSpace))
                replaceValue += ((char)P_Char).ToString();
            else if (P_Char == '\n')
            {
                LIST_Replaces.Add(replaceName, replaceValue);

                replaceName = "";
                replaceNameFinished = false;
                replaceValue = "";
                STATE_PreCompileArg = false;
            }
        }

        string fileName = "";
        private void ReadIncludes()
        {
            if (!STATE_InString && !STATE_WhiteSpace && P_Char != ';')
                fileName += ((char)P_Char).ToString();
            else if (!STATE_InString && P_Char == ';')
            {
                Reader r = LIST_Includes.Add(fileName, C_IncludeDepth + 1);
                if (r != null)
                {
                    LIST_Constants.AddRange(r.LIST_Constants);
                    LIST_Replaces.AddRange(r.LIST_Replaces);
                }

                fileName = "";
                STATE_PreCompileArg = false;
            }
        }

        bool inSignature = false;
        private void ReadFunctionDefinitions()
        {
            if (!STATE_InBlock && !STATE_InString && !STATE_InComment)
            {
                if (P_Char == '}')
                {
                    STATE_FunctionContent += '}';

                    LIST_Functions.Add(STATE_FunctionName, STATE_FunctionSignature, STATE_FunctionContent);

                    STATE_InFunction = false;
                    STATE_FunctionContent = "";
                    STATE_FunctionName = "";
                    STATE_FunctionSignature = "";
                }
                else if (normalText != "" && !inSignature)
                {
                    STATE_FunctionName = normalText;
                }
                else if (STATE_InRoundBracket)
                {
                    inSignature = true;
                    STATE_FunctionSignature += ((char)P_Char).ToString();
                }
                else if (P_Char == ')')
                {
                    STATE_FunctionSignature += ')';
                    inSignature = false;
                }
            }
            else if( STATE_InFunction || P_Char == '{' )
            {
                STATE_FunctionContent += ((char)P_Char).ToString();
                STATE_InFunction = true;
            }
        }

        private void ReplaceConstants()
        {
            if (STATE_InComment) return;
            if (STATE_InString) return;
            if (!STATE_InFunction) return;

            Constant constant;
            string value;

            int localConstsIndex = LIST_Constants.FindIndex(c => c.Name == normalText);
            int permConstsIndex = Program.A_Constants.FindIndex(c => c.Name == normalText);
            if (permConstsIndex != -1)
            {
                constant = Program.A_Constants[permConstsIndex];
                value = GetPermConstantValue(constant);
            }
            else if (localConstsIndex != -1)
            {
                constant = LIST_Constants[localConstsIndex];
                value = constant.Value;
            }
            else
                return;

            C_FinalStr = C_FinalStr.Substring(0, C_FinalStr.Length - constant.Name.Length + 1);
            C_FinalStr += value;

            ARG_GoToEnd++;
            ARG_IgnoreChar++;
        }

        private string GetPermConstantValue(Constant constant)
        {
            switch (constant.Name)
            { 
                case "__FILE__":
                    return "\"" + C_FileNameShort + "\"";
                case "__FILEFULL__":
                    return "\"" + C_FileName + "\"";
                case "__LINE__":
                    return (P_LineI + 1).ToString();
                case "__LINEFULL__":
                    return P_Line;
                case "__FUNCTION__":
                    return "\"" + STATE_FunctionName + "\"";
                case "__FUNCTIONFULL__":
                    return "\"" + STATE_FunctionName + STATE_FunctionSignature + "\"";
                default:
                    return constant.Value;
            }
        }

        string controlArgType = "";
        private void ReadControlArgs()
        {
            if (STATE_InComment) return;
            if (STATE_InString) return;
            if (!STATE_InFunction) return;

            if (controlArgType == "" && normalText != "" && P_CharNextBlack == '(')
            {
                controlArgType = normalText;
                STATE_InControlArg = true;
            }

            if (controlArgType != "")
            {
                switch (controlArgType)
                {
                    case "foreach":
                        STATE_InControlArg = true;
                        ReadForeach();
                        break;
                    case "if":
                    case "while":
                    case "for":
                    case "switch":
                        STATE_InControlArg = false;
                        break;
                    default:
                        ProcessFunctionCall(controlArgType);
                        STATE_InControlArg = false;
                        break;
                }
            }

            if(!STATE_InControlArg)
                controlArgType = "";
        }

        string foreachVarName = "";
        bool foreachVarNameEnd = false;
        bool foreachInWordEnd = false;
        string foreachArrayName = "";
        bool foreachArrayNameEnd = false;
        bool foreachArgEnd = false;
        int foreachCounter = 0;
        private void ReadForeach()
        {
            if (normalText == "foreach")
            {
                C_FinalStr = C_FinalStr.Substring(0, C_FinalStr.Length - normalText.Length + 1);
                ARG_IgnoreChar++;
                return;
            }

            if (!foreachVarNameEnd && normalText != "")
                foreachVarName = normalText;
            else if (!foreachVarNameEnd && foreachVarName != "" && STATE_WhiteSpace)
                foreachVarNameEnd = true;
            else if (!foreachInWordEnd && normalText == "in")
                foreachInWordEnd = true;
            else if (foreachInWordEnd && !foreachArrayNameEnd && !(STATE_WhiteSpace || P_Char == ')'))
                foreachArrayName += (char)P_Char;
            else if (foreachInWordEnd && !foreachArrayNameEnd && foreachArrayName != "")
                foreachArrayNameEnd = true;

            if (foreachInWordEnd && foreachArrayNameEnd && P_Char == ')')
                foreachArgEnd = true;

            if (foreachArgEnd)
            {
                if (P_CharI >= doWhileIgnore)
                {
                    if (foreachVarName == "" || foreachArrayName == "")
                        Debug.Error("foreach syntax error Line: " + Program.DEBUG_LineI);

                    string counterName = "foreachCounter" + foreachCounter;
                    string forArgOutput = "for (" + counterName + " = 0; " + counterName + " < " + foreachArrayName + ".size; " + counterName + "++)";
                    string forContentOutput = " " + foreachVarName + " = " + foreachArrayName + "[" + counterName + "]; if( !IsDefined( " + foreachVarName + " ) ) continue;";

                    bool isBlock = false;
                    if (P_CharNextBlack == '{')
                        isBlock = true;

                    C_SourceStr = C_SourceStr.Insert(P_CharI + 1, forArgOutput);

                    if (!isBlock)
                    {
                        C_SourceStr = C_SourceStr.Insert(P_CharI + 1 + forArgOutput.Length, " {" + forContentOutput);
                        int argEndIndex = C_SourceStr.IndexOf(';', P_CharI + 1 + forArgOutput.Length + forContentOutput.Length + 2);
                        C_SourceStr = C_SourceStr.Insert(argEndIndex + 1, " }");
                    }
                    else
                    {
                        int blockStartIndex = C_SourceStr.IndexOf('{', P_CharI + 1 + forArgOutput.Length);
                        C_SourceStr = C_SourceStr.Insert(blockStartIndex + 1, forContentOutput);
                    }
                }

                foreachVarName = "";
                foreachVarNameEnd = false;
                foreachInWordEnd = false;
                foreachArrayName = "";
                foreachArrayNameEnd = false;
                foreachArgEnd = false;
                foreachCounter++;

                STATE_InControlArg = false;
            }

            ARG_IgnoreChar++;
        }

        List<int> doWhileStart = new List<int>();
        bool inWhileStatement = false;
        int doWhileIgnore = 0;
        private void ReadDoWhile()
        {
            if (STATE_InComment) return;
            if (STATE_InString) return;
            if (!STATE_InFunction) return;

            if (normalText == "do" && Char.IsWhiteSpace((char)P_CharNext))
            {
                if (P_CharI >= doWhileIgnore)
                    doWhileStart.Add(P_CharI + 1);

                C_FinalStr = C_FinalStr.Substring(0, C_FinalStr.Length - 1);
                ARG_IgnoreChar++;
                return;
            }

            if (normalText == "while" && P_CharNextBlack == '(')
            {
                inWhileStatement = true;
                return;
            }

            if (inWhileStatement && !STATE_InRoundBracket)
            {
                if (P_CharNextBlack == ';')
                {
                    if (doWhileStart.Count == 0)
                        Debug.Error( "Bad do...while syntax Line: "+Program.DEBUG_LineI );

                    int startWhileI = C_SourceStr.LastIndexOf("while", P_CharI);
                    int endWhileI = C_SourceStr.IndexOf(';', P_CharI);

                    int doStart = doWhileStart[doWhileStart.Count - 1];
                    doWhileStart.RemoveAt(doWhileStart.Count - 1);

                    string args = C_SourceStr.Substring(doStart, startWhileI - doStart);
                    string finalArgs = " " + ReaderHelper.ScriptToOneLine(args);

                    doWhileIgnore = P_CharI + finalArgs.Length + 1;

                    //Debug.WriteLine("args: "+args);
                    C_SourceStr = C_SourceStr.Remove(endWhileI, 1);
                    C_SourceStr = C_SourceStr.Insert(endWhileI, finalArgs);
                }

                inWhileStatement = false;
            }
        }

        private void ProcessFunctionCall(string name)
        { 
            
        }

        private void ReplaceReplaces()
        {
            foreach(Replace r in LIST_Replaces)
                C_FinalStr = C_FinalStr.Replace(r.Name, r.Value);
        }

        int charsInLine = 0;
        bool isPrevCharWhite = false;
        private void ComprimeCode()
        {
            if (STATE_InString) return;
            if (ARG_IgnoreChar != 0) return;
            if (!SETTINGS_ComprimeCode) return;

            if (Char.IsWhiteSpace((char)P_Char))
            {
                if (charsInLine >= SETTINGS_ComprimeCodePerLine)
                {
                    P_Char = '\n';
                    charsInLine = 0;
                }
                else if (isPrevCharWhite)
                {
                    ARG_IgnoreChar = 1;
                }

                isPrevCharWhite = true;
            }
            else
                isPrevCharWhite = false;
        }

        public void ReadFile()
        {
            try
            {
                Debug.Separator();
                Debug.WriteLine("Reading file \t" + C_FileName, true);
                Debug.Separator();

                FileStream fs = new FileStream(C_FileNameFull, FileMode.Open, FileAccess.Read, FileShare.Delete);
                StreamReader sr = new StreamReader(fs);

                C_SourceStr = sr.ReadToEnd();

                sr.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                Debug.Error(e.Message);
            }
        }

        public void WriteFile()
        {
            if (C_Writed)
                return;

            try
            {
                Debug.Separator();
                Debug.WriteLine("Writting file \t" + C_FileName);
                Debug.Separator();

                FileStream fs = new FileStream(C_FileNameFull, FileMode.Create, FileAccess.Write, FileShare.Delete);
                StreamWriter sw = new StreamWriter(fs);

                sw.Write(C_FinalStr);

                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                Debug.Error(e.Message);
            }

            C_Writed = true;
        }
    }

    class ReaderHelper
    { 
        public static string GetLine(string sourceString, int startIndex)
        {
            string line = "";

            if (startIndex >= sourceString.Length)
                return line;

            for (int i = startIndex; i < sourceString.Length; i++)
            {
                if (sourceString[i] == '\n' || sourceString[i] == '\r')
                    break;

                line += sourceString[i];
            }
            return line;
        }

        public static string GetNextLine(string sourceString, int startIndex, string currentLine)
        {
            int i = startIndex + currentLine.Length;
            return GetLine(sourceString, i);
        }

        public static int GetNextBlackChar(string sourceString, int nextCharI)
        {
            for (int i = nextCharI; i < sourceString.Length; i++)
            {
                if (Char.IsWhiteSpace(sourceString[i]))
                    continue;

                return sourceString[i];
            }

            return -1;
        }

        public static string ScriptToOneLine(string sourceString)
        {
            string newString = "";
            bool start = false;
            int prevChar = 0;
            int curChar = 0;
            for (int i = 0; i < sourceString.Length; i++)
            {
                curChar = sourceString[i];
                if(i > 0)
                    prevChar = sourceString[i-1];

                if (Char.IsWhiteSpace((char)curChar) && !start)
                    continue;

                start = true;

                if (Char.IsWhiteSpace((char)curChar) && Char.IsWhiteSpace((char)prevChar))
                    continue;

                if (GetNextBlackChar(sourceString, i) == -1)
                    continue;

                if (curChar == '\t' || curChar == '\n' || curChar == '\r')
                    curChar = ' ';

                newString += (char)curChar;
            }

            return newString;
        }
     }

    class ReaderList : List<Reader>
    {
        public Reader Add(string fileName, int incDepth)
        {
            Reader r = new Reader(fileName, incDepth);
            this.Add(r);
            return r;
        }

        public Reader GetReader(string fileName, int incDepth)
        {
            int index = this.FindIndex(a => a.C_FileName == fileName);
            if (index == -1)
            {
                if (!Program.A_ScriptFiles.Contains(fileName))
                {
                    string sourceFile = Program.A_SourceDir + "\\" + fileName + ".gsc";
                    string targetFile = Program.A_TargetDir + "\\" + fileName + ".gsc";
                    if (File.Exists(sourceFile))
                        File.Copy(sourceFile, targetFile, true);
                    else
                        Debug.Error("Could not load include file \'" + fileName + "\'");
                }

                return this.Add(fileName, incDepth);
            }

            return this[index];
        }
    }
}
