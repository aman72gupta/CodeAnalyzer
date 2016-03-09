/*
 * CommandLineParser.cs - parses commnad line arguments entered by the user
 * Version : 1.0
 * Author : Aman Gupta
 * Language:    C#, 2008, .Net Framework 4.0                         
 * Platform:    Dell Inspiron, Win 8.1
 */
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   CommandLineParser  - Interprets the paramters enterd by user and returns patterns, options to the Executive.
 *   It has following functions :
 *   - getPatterns -> extracts patterns from command line args
 *   - checks whether options to analyse relations, store result to XML and Analyse sub directory files have been entered or not
 * 
  */     
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class CommandLineParser
    {

        static public List<string> getPatterns(string args)
        {
            string[] patterns = args.Split(',');
            List<string> patternList = new List<string>();
            foreach (string s in patterns)
            {
                if(s.Contains("*.")|| s.Contains("."))
                patternList.Add(s);
            }
            return patternList;

        }
        public static bool CheckForRelation(string arg)
        {
            if (arg.Contains("/R") || arg.Contains("/r"))
                return true;
            return false;

        }
        public static bool CheckForXML(string arg)
        {
            if (arg.Contains("/X")|| arg.Contains("/x"))
                return true;
            return false;

        }
        public static bool CheckForRecurse(string arg)
        {
            if (arg.Contains("/S") || arg.Contains("/s"))
                return true;
            return false;

        }
#if(TEST_COMMANDLINEPARSER)
        static void Main(string[] args)
        {
            List<string> patterns = getPatterns(args[1]);
            foreach (string pattern in patterns)
            {
                Console.Write(pattern);
            }
            CheckForRecurse(args[2]);
            CheckForRelation(args[2]);
            CheckForXML(args[2]);
        }
#endif
    }
}
