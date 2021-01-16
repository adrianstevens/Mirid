using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirid.Models;
using Mirid.Output;

namespace Mirid
{
    class Program
    {
        static string MeadowFoundationPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        static string MeadowFoundationDocsPath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation";

        static FileInfo[] projectFiles;

        //static List<MeadowDriverProject> driverProjects = new List<MeadowDriverProject>();

        static List<FileInfo> driverProjectFiles = new List<FileInfo>();
        //static List<FileInfo> sampleProjectFiles = new List<FileInfo>();

        static List<MFDriver> drivers = new List<MFDriver>();

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            projectFiles = FileCrawler.GetAllProjectsInFolders(MeadowFoundationPath);

            driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);
            //sampleProjectFiles = FileCrawler.GetSampleProjects(projectFiles);
            
            foreach(var driver in driverProjectFiles)
            {
                drivers.Add(GetDriverFromProjectFile(driver));
            }

            drivers = drivers.OrderBy(x => x.PackageName).ToList();

            ReadNamespaces(drivers);

            ReadDocsOverride(drivers);

            CsvOutput.WriteCSVs(drivers);
        }

        static void ReadDocsOverride(List<MFDriver> drivers)
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

        static void ReadNamespaces(List<MFDriver> drivers)
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

        static MFDriver GetDriverFromProjectFile(FileInfo driverProjectFile)
        {
            if(File.Exists(driverProjectFile.FullName) == false)
            {
                return null;
            }

            //metadata
            var driver = new MFDriver();
            driver.PackageName = driverProjectFile.Name.Substring(0, driverProjectFile.Name.IndexOf(".csproj"));

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
    }
}