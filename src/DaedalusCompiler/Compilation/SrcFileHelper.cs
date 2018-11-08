using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace DaedalusCompiler.Compilation
{
    public static class SrcFileHelper
    {

        public static string[] GetLines(string srcFilePath)
        {
            string[] lines = File.ReadAllLines(srcFilePath);
            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Contains("//"))
                {
                    lines[i] = lines[i].Split("//").First();
                }
            }
            return lines;
        }
        
        public static IEnumerable<string> LoadScriptsFilePaths(string srcFilePath)
        {
            Path.GetFileName(srcFilePath);
            return LoadScriptsFilePaths(srcFilePath, new HashSet<string>());
        }

        private static IEnumerable<string> LoadScriptsFilePaths(string srcFilePath, HashSet<string> alreadyLoadedFiles)
        {
            if (Path.GetExtension(srcFilePath).ToLower() != ".src")
                throw new Exception($"Invalid SRC file: '{srcFilePath}'.");
            
            if (alreadyLoadedFiles.Contains(srcFilePath.ToLower()))
                throw new Exception($"Cyclic dependency detected. SRC file '{srcFilePath}' is already loaded");
            
            alreadyLoadedFiles.Add(srcFilePath.ToLower());

            try
            {
                var lines = GetLines(srcFilePath);
                var basePath = Path.GetDirectoryName(srcFilePath);
                var result = LoadScriptsFilePaths(basePath, lines, alreadyLoadedFiles);
                return result;
            }
            catch (Exception exc)
            {
                throw new Exception($"Error while loading scripts file paths from SRC file '{srcFilePath}'", exc);
            }
        }

        private static IEnumerable<string> LoadScriptsFilePaths(string basePath, string[] srcLines, HashSet<string> alreadyLoadedFiles)
        {
            List<string> result = new List<string>();

            foreach (string line in srcLines.Where(x => String.IsNullOrWhiteSpace(x) == false).Select(item => Path.Combine(item.Trim().Split("\\").ToArray())))
            {
                try
                {
                    bool containsWildcard = line.Contains("*");
                    string relativePath = line; //Path.Combine(line.Split("\\").ToArray());
                    string fullPath = Path.Combine(basePath, relativePath);
                    string pathExtensionLower = Path.GetExtension(fullPath).ToLower();

                    if (containsWildcard && pathExtensionLower == ".d")
                    {
                        string dirPath = Path.GetDirectoryName(fullPath);
                        string filenamePattern = Path.GetFileName(fullPath);


                        EnumerationOptions options = new EnumerationOptions {MatchCasing = MatchCasing.CaseInsensitive};
                        // List<string> filePaths = Directory.GetFiles(dirPath, filenamePattern, options).ToList();
                        
                        List<string> filePaths = Directory.GetFiles(basePath, relativePath, options).ToList();
                        
                        // we make custom sort to achieve same sort results independent from OS 
                        filePaths.Sort((a, b) =>
                        {
                            if (a.StartsWith(b))
                            {
                                return a.Length > b.Length ? -1 : 1;
                            }

                            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
                        });

                        foreach (string filePath in filePaths)
                        {
                            string filePathLower = filePath.ToLower();
                            if (!alreadyLoadedFiles.Contains(filePathLower))
                            {
                                alreadyLoadedFiles.Add(filePathLower);
                                result.Add(filePath);
                            }
                        }
                    }
                    else if (pathExtensionLower == ".d")
                    {
                        
                        EnumerationOptions options = new EnumerationOptions {MatchCasing = MatchCasing.CaseInsensitive};
                        List<string> filePaths = Directory.GetFiles(basePath, relativePath + "*", options).ToList();
                        if (filePaths.Count != 1)
                        {
                            throw new Exception($"Unambigous path: {fullPath}. Possible paths: {string.Join(";", filePaths)}");
                        }

                        fullPath = filePaths.First();
                        
                        string fullPathLower = fullPath.ToLower();
                        if (!alreadyLoadedFiles.Contains(fullPathLower))
                        {
                            alreadyLoadedFiles.Add(fullPathLower);
                            result.Add(fullPath);
                        }
                    }
                    else if (pathExtensionLower == ".src")
                    {
                        result.AddRange(LoadScriptsFilePaths(fullPath, alreadyLoadedFiles));
                    }
                    else
                    {
                        throw new Exception("Unsupported script file format");
                    }
                }
                catch (Exception exc)
                {
                    throw new Exception($"Invalid line {Array.IndexOf(srcLines, line) + 1}: '{line}'", exc);
                }
            }

            return result;
        }
    }
}
