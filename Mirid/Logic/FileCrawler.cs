using System;
using System.Collections.Generic;
using System.IO;

namespace Mirid
{
    public static class FileCrawler
    {
        public static FileInfo[] GetAllProjectsInFolders(string path)
        {
            //check if path exists first
            if (Directory.Exists(path))
            {
                return GetCsProjFiles(path);
            }
            else
            {
                return new FileInfo[0];
            }
        }

        static FileInfo[] GetCsProjFiles(string path)
        {
            return (new DirectoryInfo(path)).GetFiles("*.csproj", SearchOption.AllDirectories);
        }

        public static List<FileInfo> GetSampleProjects(FileInfo[] projects)
        {
            var samples = new List<FileInfo>();

            foreach (var file in projects)
            {
                if (file.Name.Contains("Sample"))
                {
                    samples.Add(file);
                }
            }

            return samples;
        }

        public static List<FileInfo> GetDriverProjects(FileInfo[] projects)
        {
            var drivers = new List<FileInfo>();

            foreach (var file in projects)
            {
                if (file.Name.Contains("Sample") == false)
                {
                    drivers.Add(file);
                }
            }

            return drivers;
        }

    }
}
