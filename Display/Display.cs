/*
 * Display.cs - Displays output to the user
 * Author : Aman Gupta
 * Language:    C#, 2008, .Net Framework 4.0                         
 * Platform:    Dell Inspiron Win 8.1  
 */
/*
 * Module Operations:
 * ------------------
 *  This module defines the following class:
 *   Display : Displays the output obtained after analysis on console line 
 *              and also stores result in XML file (if option is provided).
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class Display
    {
        public static void displayTypes(Dictionary<string, List<Elem>> data, bool xmlRequired)
        {
            int totalComplexity=0, totalLines =0;
            Console.WriteLine("\n\n--------------------------------------");
            Console.WriteLine("***Types and Functions Analysis***");
            Console.WriteLine("--------------------------------------");
            foreach (KeyValuePair<string, List<Elem>> item in data)
            {
                totalComplexity = 0; totalLines = 0;
                Console.WriteLine("\n\nFile Analysed : {0}\n", item.Key);
                List<Elem> types = item.Value;
                Console.WriteLine("{0, -20}{1, -25}{2, -20}{3, -20}", "Type ", "Type Name", "Lines of Code ", "Complexity");
                Console.WriteLine("---------------------------------------------------------------------------");
                if (types.Count == 0)
                {
                    Console.WriteLine("*** No Types or Functions Found ***");
                }
                else
                {

                    foreach (Elem e in types)
                    {
                        if (e.type != "function")
                        {
                            Console.WriteLine("{0, -20}{1, -30}", e.type.ToUpper(), e.name);
                        }
                        else
                        {
                            Console.WriteLine("{0,-20}{1,-30}{2, -20}{3}", e.type, e.name, e.type == "function" ? (e.end - e.begin+1).ToString() : "", e.type == "function" ? (e.complexity + 1).ToString() : "");
                            totalComplexity = totalComplexity + (e.complexity + 1);
                            totalLines = totalLines + (e.end - e.begin + 1);

                        }
                        
                    }
                    
                }
                Console.WriteLine("\n\t**Total Functional Lines of Code : {0}", totalLines);
                Console.WriteLine("\t**Total Functional Complexity : {0}",totalComplexity);
                Console.WriteLine("\n\n\tPress Enter for Next File.");
                Console.ReadLine();
            }
            if (xmlRequired)
            {
                XMLCreator.CreateTypesXML(data);
            }
            


            
            
        }


        public static void displayRelations(Dictionary<string, List<Relationships>> data, bool xmlRequired)
        {
            Console.WriteLine("\n\n--------------------------------------");
            Console.WriteLine("***Relationship Analysis***");
            Console.WriteLine("--------------------------------------");
            foreach (KeyValuePair<string, List<Relationships>> item in data)
            {

                Console.WriteLine("\n\nFile Analysed : {0}\n", item.Key);
                List<Relationships> rel = item.Value;
                List<Relationships> sortedRel = rel.OrderBy(x => x.type).ThenBy(x => x.relation).ToList(); 
                Console.WriteLine("{0, -25}{1, -25}{2, -20}", "Type ", "Relation", "Related Type");
                Console.WriteLine("---------------------------------------------------------------------------");
                if (sortedRel.Count == 0)
                {
                    Console.WriteLine("*** No Relations Found ***");
                }
                else
                {
                    foreach (Relationships r in sortedRel)
                    {
                        Console.WriteLine("{0,-25}{1,-25}{2,-20}", r.type, r.relation, r.typeRelated);
                    }
                }
                Console.WriteLine("\n\n\tPress Enter for Next File.");
                Console.ReadLine();

            }
            if (xmlRequired)
            {
                XMLCreator.CreateRelationshipXml(data);
            }




        }



#if(TEST_XML)
        static void Main(string[] args)
        {
            List<Elem> list = new List<Elem>();
            Elem e = new Elem();
            e.type = "namespace";
            e.name = "codeanalysis";
            list.Add(e);
            e = new Elem();
            e.type = "class";
            e.name = "analyzer";
            list.Add(e);
            e = new Elem();
            e.type = "function";
            e.name = "analysis";
            e.complexity = 2; e.end = 20; e.begin = 2;
            list.Add(e);
            e = new Elem();
            e.type = "function";
            e.name = "parser";
            e.complexity = 2; e.end = 20; e.begin = 2;
            list.Add(e);
            Dictionary<string, List<Elem>> dict = new Dictionary<string, List<Elem>>();
            dict.Add("filename",list);

            displayTypes(dict,true);

            Relationships r = new Relationships();
            r.type = "A"; r.relation = "inheritance"; r.typeRelated = "B";
            List<Relationships> rList = new List<Relationships>();
            rList.Add(r);
            Dictionary<string, List<Relationships>> rel = new Dictionary<string, List<Relationships>>();
            rel.Add("filename", rList);
            displayRelations(rel,true);
        }
#endif


    }
}
