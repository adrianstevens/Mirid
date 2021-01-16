using System;
using System.Collections.Generic;
using System.IO;

namespace Mirid
{
    public class Validations
    {
        public Validations()
        {
        }

        static void CheckForMatchingClass(List<FileInfo> driverProjectFiles)
        {
            foreach (var file in driverProjectFiles)
            {
                if (DoesProjectContainMatchingClass(file) == false)
                {
                    Console.WriteLine($"Missing matching driver class for {file}");
                }
            }
        }

        static bool DoesProjectContainMatchingClass(FileInfo projectFile)
        {
            var driverName = projectFile.Name.Substring(0, projectFile.Name.IndexOf(".csproj"));
            driverName = driverName.Substring(driverName.LastIndexOf(".") + 1);

            var directory = projectFile.Directory;

            bool exists = File.Exists(Path.Combine(directory.FullName, driverName + ".cs"));

            if (exists == false)
            {
                exists = File.Exists(Path.Combine(directory.FullName, driverName + "Base.cs"));
            }
            if (exists == false)
            {
                exists = File.Exists(Path.Combine(directory.FullName, driverName + "Core.cs"));
            }
            return exists;
        }

        static bool IsProjectInMatchingFolder(FileInfo projectFile)
        {
            return false;
        }
    }
}
