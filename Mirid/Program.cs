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
        public static string MeadowFoundationPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        public static string MeadowFoundationDocsPath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation";

        static List<MFDriver> drivers = new List<MFDriver>();

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            var projectFiles = FileCrawler.GetAllProjectsInFolders(MeadowFoundationPath);

            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);
            
            foreach(var file in driverProjectFiles)
            {
                drivers.Add(new MFDriver(file));
            }

            drivers = drivers.OrderBy(x => x.PackageName).ToList();

            CsvOutput.WriteCSVs(drivers);
        }
    }
}