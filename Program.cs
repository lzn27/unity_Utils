using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> paths = new List<string>() 
            { 
                "E:\\unity_Learn\\ATerribleKingdom\\Assets\\Scenes",
                //"E:\\unity_Learn\\ATerribleKingdom\\Library",
                //"E:\\unity_Learn\\ATerribleKingdom\\Packages",

            };
            //string rgPath = "D:\\cmdTools\\ripgrep-13.0.0-x86_64-pc-windows-msvc\\rg.exe";
            ChangeGuid obj = new ChangeGuid();
            obj.ChangeOldGuid("E:\\unity_Learn\\ATerribleKingdom\\Assets\\Scripts\\Utilities", paths);
            //obj.ChangeDirectoryGuid(paths);
            
        }
    }




    public class ChangeGuid
    {
        private HashSet<string> ignoreFile = new HashSet<string>()
        {
            ".png","exr","asset",".cs"
        };
        private Stopwatch stw = new Stopwatch();
        private HashSet<string> ignoreDirs = new HashSet<string>() { ".vs", ".git", ".vscode" };
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        public void ChangeOldGuid(string path, List<string> searchPath)
        {
            Queue<string> paths = new Queue<string>();
            paths.Enqueue(path);
            while (paths.Count > 0)
            {
                var p = paths.Dequeue();
                var files = Directory.GetFiles(p);
                var dirs = Directory.GetDirectories(p);
                foreach (var f in files)
                {
                    if (f.EndsWith(".meta"))
                    {
                        string txt = File.ReadAllText(f);
                        var matches = Regex.Matches(txt, "(?<=guid: )[a-z,0-9]{32}");
                        foreach (Match mat in matches)
                        {
                            string guid = mat.Value;
                            if (!dict.ContainsKey(guid))
                            {
                                string newGuid = System.Guid.NewGuid().ToString("N");
                                dict.Add(guid, newGuid);
                            }
                        }
                    }
                }
                foreach (var dir in dirs)
                {
                    paths.Enqueue(dir);
                }
            }
            if (!searchPath.Contains(path))
            {
                searchPath.Add(path);
            }
            ChangeDirectoryGuid(searchPath, true);

        }

        public void ChangeDirectoryGuid(List<string> path, bool SpecifyDir)
        {
            int fileNum = 0;
            stw.Reset();
            stw.Start();
            Queue<string> paths = new Queue<string>(path);
            while (paths.Count > 0)
            {
                var p = paths.Dequeue();
                var files = Directory.GetFiles(p);
                var dirs = Directory.GetDirectories(p);
                foreach (var f in files)
                {
                    ChangeFileGuid(f, !SpecifyDir);
                    fileNum++;
                }
                foreach (var dir in dirs)
                {
                    if (!ignoreDirs.Contains(dir))
                    {
                        paths.Enqueue(dir);
                    }
                }
            }
            stw.Stop();
            Console.WriteLine("总共扫描文件数量：" + fileNum+"\n耗费总时长："+stw.ElapsedMilliseconds+"ms");
        }

        public void ChangeFileGuid(string filePath, bool generateNewGuid)
        {
            foreach(var endstr in ignoreFile)
            {
                if (filePath.EndsWith(endstr))
                {
                    return;
                }
            }
            string txt = File.ReadAllText(filePath);
            var matches = Regex.Matches(txt, "(?<=guid: )[a-z,0-9]{32}");
            HashSet<string> set = new HashSet<string>();
            bool needWrite = false;
            foreach (Match mat in matches)
            {
                if (set.Contains(mat.Value))
                {
                    continue;
                }
                else
                {
                    set.Add(mat.Value);
                }
                string guid = mat.Value;
                if (dict.ContainsKey(guid))
                {
                    string newGuid = dict[guid];
                    txt = txt.Replace(guid, newGuid);
                    needWrite = true;
                }
                else
                {
                    if (generateNewGuid)
                    {
                        string newGuid = System.Guid.NewGuid().ToString("N");
                        dict.Add(guid, newGuid);
                        txt = txt.Replace(guid, newGuid);
                        needWrite = true;
                    }
                }
            }
            if (needWrite)
            {
                File.WriteAllText(filePath, txt);
            }
        }

        public void ChangeFileGuidCMD(string filePath)
        {
            //PowerShell.Create().AddCommand("Get-Process")
            //.AddParameter("Name", "PowerShell")
            //.Invoke();
        }
    }

}
