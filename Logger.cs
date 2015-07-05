using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Utils;
using SE_Script_Library.Reference;

namespace SE_Script_Library
{
    /*
    Place this in your program script somewhere outside any method:
    >>>Copy after this line>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    private IMyProgrammableBlock _logger;
    // CHANGE the string so it fits the name of your programable block holding the logger script. 
    private const string _loggerProgram = "Prog Logger";
    <<<End copy before this line<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
    
    Place the following as shown in the Main-method:
    void Main(string argument)
    {
        >>>Copy after this line>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        // init _logger
        if (_logger == null)
            _logger = (IMyProgrammableBlock)GridTerminalSystem.GetBlockWithName(_loggerProgram);
        <<<End copy before this line<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        ...
    }
     
    Place this method in your program script and use it to log messages.
    >>>Copy after this line>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="msg">The message to be logged.</param>
    void Log(string msg)
    {
        if (_logger != null)
            _logger.ApplyAction("Run",
                new List<TerminalActionParameter>() { 
                    TerminalActionParameter.Get("log="+msg)  
            });
    }
    <<<End copy before this line<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
    */
    class Logger : MyGridProgram
    {
        /// <summary>
        /// to log a message: log=message
        /// </summary>
        public const string Log = "log";
        /// <summary>
        /// to clear the log: clear
        /// </summary>
        public const string Clear = "clear";

        public const char Seperator = ';';
        public const char Definator = '=';


        private readonly char[] SplitSep = new char[] { Seperator };
        private readonly char[] SplitDef = new char[] { Definator };
        private const int MaxBufferLength = 7500;

        StringBuilder _buffer = null;
        int _curPage = 1;

        void Main(string argument)
        {
            // init from Storage
            if (_buffer == null)
                _buffer = new StringBuilder(Storage);

            ParseArgument(argument);
            Storage = _buffer.ToString();
        }

        private int CurrentPageLength()
        {
            int countPages = CountPages();
            return _curPage < countPages ? MaxBufferLength : _buffer.Length - (countPages - 1) * MaxBufferLength;
        }

        private int CurrentPageStartIndex()
        {
            return (_curPage - 1) * MaxBufferLength;
        }

        private int CountPages()
        {
            int d = (int)Math.Ceiling((double)_buffer.Length / (double)MaxBufferLength);
            return Math.Max(d, 1);
        }

        private string Info()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[[ ").Append(CurrentPageLength()).Append('/').Append(MaxBufferLength).Append(" Chars, ");
            sb.Append("Page ").Append(_curPage).Append('/').Append(CountPages()).Append(" ]]");
            return sb.ToString();
        }

        private void LogMsg(string message)
        {
            _buffer.Append("===[").Append(DateTime.Now).AppendLine("]===").Append(message).AppendLine();
        }

        /// <summary>
        /// Parse the argument to extract arguments for the Logger.
        /// </summary>
        /// <param name="argument">The argument of the Main</param>
        /// <returns>A List of log messages.</returns>
        private void ParseArgument(string argument)
        {
            // parse arguments
            string[] args = argument.Split(SplitSep);
            for (int i = 0; i < args.Length; i++)
            {
                string[] assignment = args[i].Split(SplitDef, 2);
                switch (assignment[0].ToLower())
                {
                    case Log:
                        if (assignment.Length == 2)
                        {
                            LogMsg(assignment[1]);
                        }
                        else
                        {
                            Echo("<Missing log message>");
                        }
                        break;
                    case Clear:
                        _buffer.Clear();
                        Echo(Info());
                        _curPage = 1;
                        break;
                    default:
                        if (int.TryParse(assignment[0], out _curPage))
                        {
                            _curPage = Math.Min(_curPage, CountPages());
                            _curPage = Math.Max(_curPage, 1);
                            Echo(Info());
                            Echo(_buffer.ToString(CurrentPageStartIndex(), CurrentPageLength()));
                        }
                        else
                        {
                            _curPage = 1;
                            Echo(Info());
                            Echo("Usage:");
                            Echo("-> Type '" + Log + Definator + "your message' for logging a message. "
                                + "Where 'your message' can be any string that should be logged.");
                            Echo("-> Type '" + Clear + "' to clear all logs.");
                            Echo("-> Type any number X to show logs of page X. (Every number is fine. ;)");
                            Echo("Commands are not case-sensitive and combinable by using '" + Seperator + "'.");
                            Echo("Example1: log=This is a simple log.");
                            Echo("Example2: log=This is my first log.;clear;log=My first log is gone.;1");
                        }
                        break;
                }
            }
        }


    }
}
