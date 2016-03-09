/*
 * FileMgr.cs - This file gets all the files that are found on the path provided and match the given patterns
 * Platform:    Dell Inspiron, Win 8.1 pro, Visual Studio 2013
 * Author:      Aman Gupta 
 */
/* Module Operations:
 * ------------------
 * This module defines the following class:
 *   FileMgr  - Gets the files from the system.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
    public class FileMgr
    {
        private List<string> files = new List<string>();
        private List<string> patterns = new List<string>();
        private bool recurse;
        public FileMgr(bool val){
            recurse = val;
        }

        public void findFiles(string path)
        {
            string[] newFiles = null;
            if (patterns.Count == 0)
                addPattern("*.*");
            foreach(string pattern in patterns)
            {
                try
                {
                    newFiles = Directory.GetFiles(path, pattern);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting file. {0}",e.Message);
                    return;
                }
                
                for (int i = 0; i < newFiles.Length; ++i)
                    newFiles[i] = Path.GetFullPath(newFiles[i]);
                files.AddRange(newFiles);
            }
            if(recurse)
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                    findFiles(dir);
            }
        }

        public void addPattern(string pattern)
        {
            patterns.Add(pattern);
        }

        public List<string> getFiles()
        {
            return files;
        }

#if(TEST_FILEMGR)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing FileMgr Class");
            Console.Write("\n =======================\n");
            
            FileMgr fm = new FileMgr(true);
            fm.addPattern("*.cs");
            fm.findFiles("../../");
            List<string> files = fm.getFiles();
            foreach (string file in files)
                Console.Write("\n  {0}", file);
            Console.Write("\n\n");
          
        }
#endif
    }
}
