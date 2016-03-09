/*
 * Analyzer.cs - Manages the code analysis operations and return the results to Executive
 * Author : Aman Gupta
 * Language:    C#, 2008, .Net Framework 4.0                         
 * Platform:    Dell Inspiron Win 8.1  
 */
/*
 * Module Operations:
 * ------------------
 *  This module defines the following class:
 *   Analyzer : does type/function or relationship analysis depending upon options entered
 */
/*
 * It has 2 functions 
 * - doAnalysisForTypes -> carries out analysis for finding types in the code and also function complexity and lines of code 
 * - doAnalysisForRelations -> carries out analysis for finding relations between the user defined types
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class Analyzer
    {
        public Dictionary<string, List<Elem>>typesForDisplay { get; set; }
        public Dictionary<string, List<Relationships>>relationsForDisplay { get; set; }

        static public string[] getFiles(string path, List<string> patterns)
        {
            FileMgr fm = new FileMgr(true);

            foreach (string pattern in patterns)
                fm.addPattern(pattern);
            fm.findFiles(path);
            return fm.getFiles().ToArray();
        }
        public Analyzer()
        {
            typesForDisplay = new Dictionary<string, List<Elem>>();
            relationsForDisplay = new Dictionary<string, List<Relationships>>();
        }
     
        

        public Dictionary<string,List<Elem>> doAnalysisForTypes(string[] files)
        {
            foreach (object file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                   //Console.Write("\n  Can't open \n\n");
                    return null;
                }
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                 
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository typesRep = Repository.getInstance();
                List<Elem> types =  typesRep.locations;
                typesForDisplay.Add(file.ToString(),types);
                semi.close();
               
                
            }
             return typesForDisplay;

        }
        public Dictionary<string, List<Relationships>> doAnalysisForRelations(string[] files)
        {
            foreach (object file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                   
                    return null;
                }
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.bulidForRelations();
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                   
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Relationships> table = rep.relationStorage;
                relationsForDisplay.Add(file.ToString(), table);


               /* foreach (Relationships r in table)
                {
                    Console.Write("\n  {0} {1,20} {2,20}\n", r.type, r.typeRelated, r.relation);
                }*/
                semi.close();
            }
            return relationsForDisplay;
        }


#if(Test_Analyzer)
        static void Main(string[] args)
        {
            string path = "C:\\Users\\Aman Gupta\\Desktop\\TestFiles";
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
            string[] files = Analyzer.getFiles(path, patterns);
            Analyzer an = new Analyzer();
            an.doAnalysisForTypes(files);
            an.doAnalysisForRelations(files);
            Console.ReadLine();
        }
#endif
    }

}
