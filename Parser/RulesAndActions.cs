///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.1                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Inspiron, Win 8.1                               //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Aman Gupta                                           //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 * It also defines the following rules for detection of Relationships 
 *   - DetectInheritance
 *   - DetectAggregation
 *   - DetectInheritance
 *   - Detect Composition
 * 
 *   Four actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 *   - StoreRelations
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeAnalysis
{
    public class Relationships
    {
        public string type { get; set; }
        public string typeRelated { get; set; }
        public string relation { get; set; }


    }

    public class Elem  // holds scope information
    {
        public string type { get; set; }
        public string name { get; set; }
        public int begin { get; set; }
        public int end { get; set; }

        public int complexity { get; set; }

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append(String.Format("{0,-5}", begin.ToString()));  // line of scope start
            temp.Append(String.Format("{0,-5}", end.ToString()));    // line of scope end
            temp.Append("}");
            return temp.ToString();
        }
    }

    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        static Repository instance;
        static List<Elem> types_ = new List<Elem>();
        //int currentParse;
        List<Relationships> relationStorage_ = new List<Relationships>();

        public Repository()
        {
            instance = this;

        }

        public static Repository getInstance()
        {
            return instance;
        }
        // provides all actions access to current semiExp

        public bool functionIsPresent
        {
            get;
            set;
        }
        public CSsemi.CSemiExp semi
        {
            get;
            set;
        }

        public int currentParse
        {
            get;
            set;
        }




        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount; }

        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }
        // enables recursively tracking entry and exit from scopes

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }
        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
        }
        public List<Elem> types
        {
            get { return types_; }
        }


        public List<Relationships> relationStorage
        {
            get { return relationStorage_; }
        }
    }

    /////////////////////////////////////////////////////////
    // Store detected relations in the repository
    public class StoreRelations : AAction
    {
        Repository repo_;
        public StoreRelations(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Relationships relation = new Relationships();
            relation.type = semi[0];
            relation.typeRelated = semi[1];
            relation.relation = semi[2];
            bool contains = repo_.relationStorage.Any(r => r.type == relation.type && r.typeRelated == relation.typeRelated && r.relation == relation.relation);
            if (!contains)
                repo_.relationStorage.Add(relation);
        }

    }


    /////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope

    public class PushStack : AAction
    {
        Repository repo_;

        public PushStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;
            if (elem.type != "braceless")
            {
                repo_.stack.push(elem);
            }
            if (repo_.currentParse == 1)
                if (elem.type == "class" || elem.type == "struct" || elem.type == "interface" || elem.type == "enum")
                    repo_.types.Add(elem);
            if (elem.type == "control" || elem.type == "braceless")
            {
                Elem temp = new Elem();
                for (int i = repo_.locations.Count - 1; i >= 1; i--)
                {
                    temp = repo_.locations[i];
                    if (temp.type == "function" && repo_.functionIsPresent)
                    {
                        repo_.locations[i].complexity++;
                        break;
                    }
                }
                return;
            }
            repo_.locations.Add(elem);
            if (elem.type == "function")
            {
                repo_.functionIsPresent = true;
            }

        }
    }
        /////////////////////////////////////////////////////////
        // pops scope info from stack when leaving scope

        public class PopStack : AAction
        {
            Repository repo_;

            public PopStack(Repository repo)
            {
                repo_ = repo;
            }
            public override void doAction(CSsemi.CSemiExp semi)
            {
                Elem elem;
                try
                {
                    elem = repo_.stack.pop();
                    if (elem.type == "function")
                    {
                        repo_.functionIsPresent = false;
                    }
                    for (int i = 0; i < repo_.locations.Count; ++i)
                    {
                        Elem temp = repo_.locations[i];
                        if (elem.type == temp.type)
                        {
                            if (elem.name == temp.name)
                            {
                                if ((repo_.locations[i]).end == 0)
                                {
                                    (repo_.locations[i]).end = repo_.semi.lineCount;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Console.Write("popped empty stack on semiExp: ");
                    semi.display();
                    return;
                }
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add(elem.type).Add(elem.name);
                if (local[0] == "control")
                    return;

                if (AAction.displaySemi)
                {
                    Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
                    Console.Write("leaving  ");
                    string indent = new string(' ', 2 * (repo_.stack.count + 1));
                    Console.Write("{0}", indent);
                    this.display(local); // defined in abstract action
                }
            }
        }
        ///////////////////////////////////////////////////////////
        // action to print function signatures - not used in demo

        public class PrintFunction : AAction
        {
            Repository repo_;

            public PrintFunction(Repository repo)
            {
                repo_ = repo;
            }
            public override void display(CSsemi.CSemiExp semi)
            {
                Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
                Console.Write("\n    ");
                for (int i = 0; i < semi.count; ++i)
                    if (semi[i] != "\n" && !semi.isComment(semi[i]))
                        Console.Write("{0} ", semi[i]);
            }
            public override void doAction(CSsemi.CSemiExp semi)
            {
                this.display(semi);
            }
        }
        /////////////////////////////////////////////////////////
        // concrete printing action, useful for debugging

        public class Print : AAction
        {
            Repository repo_;

            public Print(Repository repo)
            {
                repo_ = repo;
            }
            public override void doAction(CSsemi.CSemiExp semi)
            {
                Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
                this.display(semi);
            }
        }
        /////////////////////////////////////////////////////////
        // rule to detect namespace declarations

        public class DetectNamespace : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("namespace");
                if (index != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // create local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add(semi[index]).Add(semi[index + 1]);
                    doActions(local);
                    return true;
                }
                return false;
            }
        }
        /////////////////////////////////////////////////////////
        // rule to dectect class definitions

        public class DetectClass : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int indexCL = semi.Contains("class");
                int indexIF = semi.Contains("interface");
                int indexST = semi.Contains("struct");
                int indexEN = semi.Contains("enum");

                int index = Math.Max(indexCL, indexIF);
                index = Math.Max(index, indexST);
                index = Math.Max(index, indexEN);
                if (index != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add(semi[index]).Add(semi[index + 1]);
                    doActions(local);
                    return true;
                }
                return false;
            }
        }
        /////////////////////////////////////////////////////////
        // rule to dectect function definitions


        public class DetectFunction : ARule
        {
            public static bool isSpecialToken(string token)
            {
                string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
                foreach (string stoken in SpecialToken)
                    if (stoken == token)
                        return true;
                return false;
            }
            public override bool test(CSsemi.CSemiExp semi)
            {
                if (semi[semi.count - 1] != "{")
                    return false;

                int index = semi.FindFirst("(");
                if (index > 0 && !isSpecialToken(semi[index - 1]))
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    local.Add("function").Add(semi[index - 1]);
                    doActions(local);
                    return true;
                }
                return false;
            }
        }
        /////////////////////////////////////////////////////////
        // detect entering anonymous scope
        // - expects namespace, class, and function scopes
        //   already handled, so put this rule after those
        public class DetectAnonymousScope : ARule
        {

            public static int containsSpecialToken(CSsemi.CSemiExp semi)
            {
                
                int count =0;
                string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using", "try", "else" };
                foreach (string stoken in SpecialToken)
                    if (semi.Contains(stoken) != -1)
                   {
                       List<int> locations = semi.FindAllOccurances(stoken);
                       if (locations.Count > 0)
                       {
                           count = count + locations.Count;
                       }
                       else
                           count++;
                        
                    }
                        
                return count;
            }
            public int checkForBracelessScope(CSsemi.CSemiExp semi)
            {
                int count = containsSpecialToken(semi);
                if (count>0)
                    if (semi[semi.count - 1] != "{")
                        return count;
                return 0;

            }



            public override bool test(CSsemi.CSemiExp semi)
            {

                int index = semi.Contains("{");
                int count = checkForBracelessScope(semi);
                if (count>0)
                {
                    for (int i = 0; i < count; i++) {
                        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                        // create local semiExp with tokens for type and name
                        local.displayNewLines = false;
                        local.Add("braceless").Add("anonymous");
                        doActions(local);
                    
                    
                    }
                    
                    return true;
                }
                else{

                
                    if (index != -1)
                    {
                        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                        // create local semiExp with tokens for type and name
                        local.displayNewLines = false;
                        local.Add("control").Add("anonymous");
                        doActions(local);
                        return true;
                    }
                }
                return false;
            }
        }

        /////////////////////////////////////////////////////////
        // detect leaving scope

        public class DetectLeavingScope : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("}");
                if (index != -1)
                {
                    doActions(semi);
                    return true;
                }
                return false;
            }
        }

        //////////////////////////////////////////////
        //Check if type exists in types repository 
        
        public abstract class CheckExistingTypes
        {
            public static bool checkIfTypeExists(string type)
            {
                Repository rep = Repository.getInstance();
                List<Elem> existingTypes = rep.types;


                foreach (Elem e in existingTypes)
                {
                    if (e.name == type)
                    {
                        return true;

                    }

                }
                return false;

            }
            /////////////////////////////////////////////
            //Gets the class which has a active scope 
            public static string getClass()
            {
                Elem temp = new Elem();
                Repository repo = Repository.getInstance();
                for (int i = repo.locations.Count - 1; i >= 0; i--)
                {
                    temp = repo.locations[i];

                    if (temp.type == "class")
                    {

                        return temp.name;

                    }


                }
                return "";
            }

        }
        //////////////////////////////////////////
        //Rules to detect inheritance        
 
        public class DetectInheritance : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("class");
                int index1 = semi.Contains("interface");
                index = Math.Max(index, index1);
                if (index != -1)
                {

                    index = semi.Contains(":");
                    if (index != -1)
                    {
                        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                        local.Add(semi[index - 1]).Add(semi[index + 1]).Add("Inheritance");
                        if (CheckExistingTypes.checkIfTypeExists(semi[index + 1]))
                        {
                            doActions(local);
                        }

                        List<int> indexes = semi.FindAllOccurances(",");
                        if (indexes.Count != 0)
                        {
                            foreach (int i in indexes)
                            {
                                if (CheckExistingTypes.checkIfTypeExists(semi[i + 1]))
                                {
                                    local = new CSsemi.CSemiExp();
                                    local.Add(semi[index - 1]).Add(semi[i + 1]).Add("Inheritance");
                                    doActions(local);
                                }
                            }
                        }
                        return false;


                    }
                    return false;
                }
                return false;


            }




        }

        //////////////////////////////////
        //Rules to detect aggregation
        
        public class DetectAggregation : ARule
        {

            public override bool test(CSsemi.CSemiExp semi)
            {
                int index1 = semi.Contains("new");
                int index2 = semi.Contains(".");
                int index3 = semi.Contains("<");
                int index = Math.Max(index1, index2);
                index = Math.Max(index, index3);

                if (index1 != -1 && index != -1)
                {

                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    if (CheckExistingTypes.checkIfTypeExists(semi[index + 1]))
                    {
                        string typeName = CheckExistingTypes.getClass();
                        if (typeName == semi[index + 1])
                        {
                            return false;
                        }
                        local.Add(CheckExistingTypes.getClass()).Add(semi[index + 1]).Add("Aggregation");
                        doActions(local);
                    }

                    return false;
                }
                return false;
            }

        }
        //////////////////////////////////
        //Rules to detect using relationship
        public class DetectUsing : ARule
        {

            public static bool isSpecialToken(string token)
            {
                string[] SpecialToken = { "ref", "in", "out", "(", ")", "," };
                foreach (string stoken in SpecialToken)
                    if (stoken == token)
                        return true;
                return false;
            }
            public override bool test(CSsemi.CSemiExp semi)
            {
                if (semi[semi.count - 1] != "{")
                    return false;
                int index = semi.FindFirst("(");
                if (index > 0 && !DetectFunction.isSpecialToken(semi[index - 1]))
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    for (int i = index; i < semi.count - 1; i++)
                    {
                        if (!isSpecialToken(semi[i]))
                        {
                            local.Add(semi[i]);

                        }
                    }
                    CSsemi.CSemiExp semiExp = new CSsemi.CSemiExp();
                    for (int i = 0; i < local.count - 1; i++)
                    {
                        if (CheckExistingTypes.checkIfTypeExists(local[i]))
                        {
                            string typeName = CheckExistingTypes.getClass();

                            if (typeName == semi[index + 1])
                            {
                                return false;
                            }
                            semiExp.Add(CheckExistingTypes.getClass()).Add(local[i]).Add("Using");
                            doActions(semiExp);
                        }


                    }


                    return false;
                }
                return false;

            }
        }

        ////////////////////////////////////
        //Rules to detect using Composition
        public class DetectComposition : ARule
        {
            public List<string> findValType()
            {
                Repository rep = Repository.getInstance();
                List<Elem> typesList = rep.types;
                List<string> valType = new List<string>();
                foreach (Elem e in typesList)
                {
                    if (e.type == "struct" || e.type == "enum")
                    {
                        valType.Add(e.name);
                    }
                }
                return valType;

            }
            public override bool test(CSsemi.CSemiExp semi)
            {
                List<string> valtype = findValType();


                foreach (string s in valtype)
                {

                    if (semi.Contains(s) != -1)
                    {
                        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                        local.Add(CheckExistingTypes.getClass()).Add(s).Add("Composition");
                        doActions(local);
                    }


                }
                /*int index1 = semi.Contains("struct");
                int index2 = semi.Contains("enum"); 
                int index = Math.Max(index1, index2);
            
                if (index != -1)
                {
                    Console.Write("Checking for composition {0}",semi[index+1]);
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    local.Add(CheckExistingTypes.getClass()).Add(semi[index+1]).Add("Composition");
                    doActions(local);
                }*/

                return false;


            }


        }

        public class BuildCodeAnalyzer
        {
            Repository repo = new Repository();

            public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
            {
                repo.semi = semi;
            }
            public virtual Parser build()
            {
                Parser parser = new Parser();
                repo.currentParse = 1;

                // decide what to show
                AAction.displaySemi = false;
                AAction.displayStack = false;  // this is default so redundant

                // action used for namespaces, classes, and functions
                PushStack push = new PushStack(repo);

                // capture namespace info
                DetectNamespace detectNS = new DetectNamespace();
                detectNS.add(push);
                parser.add(detectNS);

                // capture class info
                DetectClass detectCl = new DetectClass();
                detectCl.add(push);
                parser.add(detectCl);

                // capture function info
                DetectFunction detectFN = new DetectFunction();
                detectFN.add(push);
                parser.add(detectFN);

                // handle entering anonymous scopes, e.g., if, while, etc.
                DetectAnonymousScope anon = new DetectAnonymousScope();
                anon.add(push);
                parser.add(anon);

                // handle leaving scopes
                DetectLeavingScope leave = new DetectLeavingScope();
                PopStack pop = new PopStack(repo);
                leave.add(pop);
                parser.add(leave);



                // parser configured
                return parser;
            }

            public virtual Parser bulidForRelations()
            {
                Parser parser = new Parser();
                repo.currentParse = 2;


                PushStack push = new PushStack(repo);
                StoreRelations store = new StoreRelations(repo);

                DetectInheritance inheritance = new DetectInheritance();
                inheritance.add(store);
                parser.add(inheritance);

                DetectComposition detectComp = new DetectComposition();
                detectComp.add(store);
                parser.add(detectComp);

                DetectClass detectCl = new DetectClass();
                detectCl.add(push);
                parser.add(detectCl);

                DetectAggregation detectAgg = new DetectAggregation();
                detectAgg.add(store);
                parser.add(detectAgg);



                DetectUsing detectUsing = new DetectUsing();
                detectUsing.add(store);
                parser.add(detectUsing);

                DetectFunction detectFN = new DetectFunction();
                detectFN.add(push);
                parser.add(detectFN);


                DetectAnonymousScope anon = new DetectAnonymousScope();
                anon.add(push);
                parser.add(anon);


                DetectLeavingScope leave = new DetectLeavingScope();
                PopStack pop = new PopStack(repo);
                leave.add(pop);
                parser.add(leave);
                return parser;



            }
        }

    }


