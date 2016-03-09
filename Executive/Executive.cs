///////////////////////////////////////////////////////////////////////////////////////////
//Executive.cs  Executive provides entry to CodeAnalyzer and gets commandLine parameters.//
//version 1.0                                                                           //
//Language: C#, .Net Framework 4.0                                                      //
//Platform: Dell Inspiron, Windows 8.1                                                  //
//Author : Aman Gupta                                                                   //
//                                                                                      //
///////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    class Executive
    {
        //------< Get files from path and patterns entered>--------
        static public string[] getFiles(string path, List<string> patterns, bool recurse )
        {
            FileMgr fm = new FileMgr(recurse);

            foreach (string pattern in patterns)
                fm.addPattern(pattern);
            fm.findFiles(path);
            return fm.getFiles().ToArray();
         }

        //-------<Perform analysis on files depending upon options provided>------------
        static public void doAnalysis(string[] files, bool relationsCheck,bool xmlCheck)
        {
            Analyzer analyzer = new Analyzer();
            if (files.Count() != 0)
            {
                if (relationsCheck)
                {
                    analyzer.doAnalysisForTypes(files);
                    Display.displayRelations(analyzer.doAnalysisForRelations(files), xmlCheck);
                }
                else
                    Display.displayTypes(analyzer.doAnalysisForTypes(files), xmlCheck);


            }
        }

        
        static void Main(string[] args)
        {
            bool relationsCheck = false;
            bool xmlCheck = false;
            bool recurseCheck = false;
            string path =null;
            if(args.Length<2||args.Length>4)
            {
                Console.WriteLine("Invalid Format for Command \nCorrect format : [path] [pattern1,pattern2,...] [options]");
                Console.ReadLine();
                return;
            }
            try
            {
                path = Path.GetFullPath(args[0]);
                Console.WriteLine("Path Entered : {0} ", path);
            }
            catch(Exception e)
            {
                Console.WriteLine("Entered Path Not Found. {0}",e.Message);
                Console.ReadLine();
                return;
            }
            List<string> patterns = CommandLineParser.getPatterns(args[1]);
            if (patterns.Count != 0) { 
            Console.Write("Patterns entered : ");
            foreach (string s in patterns)
            {
                Console.Write("{0} ",s);
            }
           }
            else
            {
                Console.WriteLine("*** No Pattern was found. Analyzing for [*.*] ***");
            }
            
            if (args.Length == 3)
            {
                relationsCheck = CommandLineParser.CheckForRelation(args[2]);
                xmlCheck = CommandLineParser.CheckForXML(args[2]);
                recurseCheck = CommandLineParser.CheckForRecurse(args[2]);
            }
            doAnalysis(getFiles(path, patterns,recurseCheck),relationsCheck,xmlCheck);
        }
    }
}
