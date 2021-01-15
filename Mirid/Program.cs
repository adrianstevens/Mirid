using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Mirid.Models;

namespace Mirid
{
    class Program
    {
        static string MeadowFoundationPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        static string MeadowFoundationDocsPath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation";

        static FileInfo[] projectFiles;
        static List<FileInfo> driverProjectFiles = new List<FileInfo>();
        static List<FileInfo> sampleProjectFiles = new List<FileInfo>();

        static List<MeadowDriver> drivers = new List<MeadowDriver>();

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            //check if path exists first
            if (Directory.Exists(MeadowFoundationPath))
            {
                projectFiles = GetCsProjFiles(MeadowFoundationPath);
            }

            SortProjects(projectFiles);

            foreach(var driver in driverProjectFiles)
            {
                drivers.Add(GetDriverFromProjectFile(driver));
            }

            drivers = drivers.OrderBy(x => x.PackageName).ToList();

            ReadNamespaces(drivers);

            ReadDocsOverride(drivers);

            WriteCSVs();
        }

        static void ReadDocsOverride(List<MeadowDriver> drivers)
        {
            foreach(var d in drivers)
            {
                //  var override = Path.Combine()
                var simpleName = d.PackageName.Split('.').LastOrDefault();

                var fileName = d.Namespace + "." + simpleName + ".md";
                var filePath = Path.Combine(MeadowFoundationDocsPath, fileName);

                if(File.Exists(filePath))
                {
                    d.HasDocOverride = true;
                }
            }
        }

        static string GetSimpleName(FileInfo file)
        {
            var nameChunks = file.Name.Split('.');

             return nameChunks[nameChunks.Length - 2];
        }

        static void ReadNamespaces(List<MeadowDriver> drivers)
        {
            foreach(var d in drivers)
            {
                var file = d.DriverFiles[0];

                if(file == null) { continue; }

                //read the file contents
                var lines = File.ReadAllLines(file.FullName);

                foreach(var line in lines)
                {
                    if(line.Contains("namespace"))
                    {
                        d.Namespace = line.Substring("namespace ".Length);
                        Console.WriteLine("Found namespace" + line.Substring("namespace ".Length));
                        break;
                    }
                }
            }
        }

        static void WriteCSVs()
        {
            using (var writer = new StreamWriter("AllDrivers.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers);
                }
            }

            using (var writer = new StreamWriter("InProgressDrivers.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers.Where(d => d.IsTested == false));
                }
            }

            using (var writer = new StreamWriter("TestedDrivers.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers.Where(d => d.IsTested == true));
                }
            }
        }

        static MeadowDriver GetDriverFromProjectFile(FileInfo driverProjectFile)
        {
            if(File.Exists(driverProjectFile.FullName) == false)
            {
                return null;
            }

            var project = File.ReadAllText(driverProjectFile.FullName);

            //metadata
            var driver = new MeadowDriver();
            driver.PackageName = driverProjectFile.Name.Substring(0, driverProjectFile.Name.IndexOf(".csproj"));
            driver.CsProjMetadata.AssemblyName = GetElement(project, "AssemblyName", driver.PackageName);
            driver.CsProjMetadata.CompanyName = GetElement(project, "Company", driver.PackageName);
            driver.CsProjMetadata.PackageId = GetElement(project, "PackageId", driver.PackageName);
            driver.CsProjMetadata.Description = GetElement(project, "Description", driver.PackageName);
            driver.CsProjMetadata.AssemblyName = GetElement(project, "AssemblyName", driver.PackageName);
            driver.CsProjMetadata.GeneratePackageOnBuild = GetElement(project, "GeneratePackageOnBuild", driver.PackageName);

            var parentDir = driverProjectFile.Directory.Parent.Parent;

            //number of drivers
            var driverDir = driverProjectFile.Directory.GetDirectories("Drivers").FirstOrDefault();
            if(driverDir != null)
            {
                driver.DriverFiles = driverDir.GetFiles();
            }
            else
            {
                var fileName = GetSimpleName(driverProjectFile) + ".cs";

                var file = Path.Combine(driverProjectFile.DirectoryName, fileName);

                if(File.Exists(file) == false)
                {
                    Console.WriteLine($"Couldn't find {file}");
                }

                driver.DriverFiles = new FileInfo[] { new FileInfo(file) };
            }

            //datasheet
            var datasheetDir = parentDir.GetDirectories("Datasheet*").FirstOrDefault();

            if (datasheetDir != null)
            {
                if (datasheetDir.GetFiles().Count() > 0)
                {
                    driver.HasDataSheet = true;
                }
            }

            //samples
            var samplesDir = parentDir.GetDirectories("Samples").FirstOrDefault();

            if (samplesDir != null)
            {
                driver.NumberOfSamples = samplesDir.GetDirectories().Count();
            }

            return driver;
        }

        static string GetElement(string project, string element, string name)
        {
            int index = project.IndexOf(element);

            if(index == -1)
            {
            //    Console.WriteLine($"Could not find {element} for {name}");
                return string.Empty;
            }

            int start = project.IndexOf(">", index) + 1;
            int end = project.IndexOf("<", start);

            return project.Substring(start, end - start);
        }

        static void CheckForMatchingClass(List<FileInfo> driverProjectFiles)
        { 
            foreach(var file in driverProjectFiles)
            {
                if(DoesProjectContainMatchingClass(file) == false)
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

            if(exists == false)
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

        static void SortProjects(FileInfo[] projectFiles)
        { 
            foreach(var file in projectFiles)
            {
                //   Console.WriteLine($"Found {file.Name}");
                if(file.Name.Contains("Sample"))
                {
                    sampleProjectFiles.Add(file);  
                }
                else
                {
                    driverProjectFiles.Add(file);
                }
            }
        }

        static FileInfo[] GetCsProjFiles(string path)
        {
            return (new DirectoryInfo(path)).GetFiles("*.csproj", SearchOption.AllDirectories);
        }
    }
}
