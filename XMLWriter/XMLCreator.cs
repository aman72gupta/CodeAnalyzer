/*
 * XMLCreator.cs - Creates XML files for analysis results
 * Author : Aman Gupta
 * Language:    C#, 2008, .Net Framework 4.0                         
 * Platform:    Dell Inspiron Win 8.1  
 */
/*
 * Module Operations:
 * ------------------
 *  This module defines the following class:
 *  XMLCreator : Creates XML files for relations analysis or type/function analysis.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class XMLCreator
    {
        public static void CreateTypesXML(Dictionary<string, List<Elem>> data)
        {
            FileStream fileStream = new FileStream(@"TypesAnalysis.xml", FileMode.Create);
            bool structDetected = false;
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);doc.AppendChild(docNode);
            XmlNode rootNode = doc.CreateElement("TypesAnalysis");doc.AppendChild(rootNode);
            XmlNode name_space = doc.CreateElement("TestNamespace"), classes = doc.CreateElement("TestClass");
            XmlNode structs = doc.CreateElement("TestStruct"),interfaces = doc.CreateElement("TestInterface"),function;
            foreach (KeyValuePair<string, List<Elem >> val in data)
            {
                if (val.Value.Count != 0)
                {

                    XmlNode fileNode = doc.CreateElement("File", val.Key); rootNode.AppendChild(fileNode);
                    foreach (Elem e in val.Value)
                    {
                        switch (e.type)
                        {
                            case "namespace": name_space = doc.CreateElement(e.type, e.name); fileNode.AppendChild(name_space); break;
                            case "class"    :  structDetected = false;
                                               classes = doc.CreateElement(e.type, e.name); name_space.AppendChild(classes);break;
                            case "struct"   :  structDetected = true;
                                               structs = doc.CreateElement(e.type, e.name); name_space.AppendChild(structs);break;
                            case "interface":  interfaces = doc.CreateElement(e.type, e.name); name_space.AppendChild(interfaces); break;
                            case "function":   function = doc.CreateElement(e.type, e.name + "()");
                                               if (structDetected)
                                                    structs.AppendChild(function);
                                               else
                                                    classes.AppendChild(function);
                                               XmlNode functionComp = doc.CreateElement("FunctionComplexity", (e.complexity + 1).ToString());
                                               XmlNode loc = doc.CreateElement("LinesOfCode", (e.end - e.begin+1).ToString());
                                               function.AppendChild(functionComp); function.AppendChild(loc); break;
                        }
                       
                    }
                }
            }
            doc.Save(fileStream);
            Console.WriteLine("***XML ouput saved to : {0} ", Path.GetFullPath("TypesAnalysis.xml"));
            Console.ReadLine();
        }
        public static void CreateRelationshipXml(Dictionary<string, List<Relationships>> data)
        {
            FileStream fileStream = new FileStream(@"RelationAnalysis.xml", FileMode.Create);
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlNode rootNode = doc.CreateElement("RelationAnalysis");
            doc.AppendChild(rootNode);
            XmlNode type, relation, relatedType, fileNode;
            foreach (KeyValuePair<string, List<Relationships>> item in data)
            {
                List<Relationships> sortedRel = item.Value.OrderBy(x => x.type).ThenBy(x => x.relation).ToList();
                if (sortedRel.Count != 0)
                {
                    fileNode = doc.CreateElement("File", item.Key);
                    rootNode.AppendChild(fileNode);
                    type = doc.CreateElement("Type", sortedRel[0].type);
                    fileNode.AppendChild(type);
                    relation = doc.CreateElement("Relation", sortedRel[0].relation);
                    type.AppendChild(relation);
                    relatedType = doc.CreateElement("RelatedType", sortedRel[0].typeRelated);
                    relation.AppendChild(relatedType);
                    for (int i = 1; i < sortedRel.Count; i++)
                    {
                        if (sortedRel[i].type != sortedRel[i - 1].type)
                        {
                            type = doc.CreateElement("Type", sortedRel[i].type);
                            fileNode.AppendChild(type);
                        }
                        if (sortedRel[i].relation != sortedRel[i - 1].relation)
                        {
                            relation = doc.CreateElement("Relation", sortedRel[i].relation);
                            type.AppendChild(relation);
                        }
                        relatedType = doc.CreateElement("RelatedType", sortedRel[i].typeRelated);
                        relation.AppendChild(relatedType);
                    }
                }
            }
            doc.Save(fileStream);
            Console.WriteLine("***XML ouput saved to : {0} ", Path.GetFullPath("RelationAnalysis.xml"));
            Console.ReadLine();
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

            CreateTypesXML(dict);

            Relationships r = new Relationships();
            r.type = "A"; r.relation = "inheritance"; r.typeRelated = "B";
            List<Relationships> rList = new List<Relationships>();
            rList.Add(r);
            Dictionary<string, List<Relationships>> rel = new Dictionary<string, List<Relationships>>();
            rel.Add("filename", rList);
            CreateRelationshipXml(rel);
        }
#endif
    }
}
