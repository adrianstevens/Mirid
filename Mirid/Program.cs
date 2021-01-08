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
                drivers.Add(GetDriverFromFile(driver));
            }

            drivers = drivers.OrderBy(x => x.PackageName).ToList();

            WriteCSV();
        }

        static void WriteCSV()
        {
            using (var writer = new StreamWriter("MFDrivers.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers);
                }
            }
        }

        static MeadowDriver GetDriverFromFile(FileInfo driverFile)
        {
            if(File.Exists(driverFile.FullName) == false)
            {
                return null;
            }

            var project = File.ReadAllText(driverFile.FullName);

            //metadata
            var driver = new MeadowDriver();
            driver.PackageName = driverFile.Name.Substring(0, driverFile.Name.IndexOf(".csproj"));
            driver.CsProjMetadata.AssemblyName = GetElement(project, "AssemblyName", driver.PackageName);
            driver.CsProjMetadata.CompanyName = GetElement(project, "Company", driver.PackageName);
            driver.CsProjMetadata.PackageId = GetElement(project, "PackageId", driver.PackageName);
            driver.CsProjMetadata.Description = GetElement(project, "Description", driver.PackageName);
            driver.CsProjMetadata.AssemblyName = GetElement(project, "AssemblyName", driver.PackageName);
            driver.CsProjMetadata.GeneratePackageOnBuild = GetElement(project, "GeneratePackageOnBuild", driver.PackageName);

            var parentDir = driverFile.Directory.Parent.Parent;

            //datasheet
            var datasheetDir = parentDir.GetDirectories("Datasheet").FirstOrDefault();

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
