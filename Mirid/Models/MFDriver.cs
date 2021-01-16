using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace Mirid.Models
{
    public class MFDriver
    {
        //indexes for writing CSV
        [Index(0)]
        public string PackageName => DriverProject.PackageId;
        [Index(1)]
        public int NumberOfDrivers => CodeFiles?.Count ?? 0;
        [Index(2)]
        public bool IsTested => DriverProject?.GeneratePackageOnBuild == "true";
        [Index(3)]
        public bool HasCompleteMetaData => DriverProject?.IsMetadataComplete() ?? false;
        [Index(4)]
        public bool HasDataSheet => Assets?.HasDataSheet ?? false;
        [Index(5)]
        public int NumberOfSamples => Assets?.NumberOfSamples ?? 0;
        [Index(6)]
        public bool HasTestSuite => false;
        [Index(7)]
        public bool HasDocOverride => Documentation?.HasOverride ?? false;

        [Ignore]
        public MFDriverProject DriverProject { get; private set; }

        [Ignore]
        public MFDriverAssets Assets { get; private set; }

        [Ignore]
        public MFDriverDocumentation Documentation { get; private set; }

        [Ignore]
        public List<MFDriverCode> CodeFiles { get; private set; } = new List<MFDriverCode>();


        [Ignore]
        public string Namespace => CodeFiles.First().Namespace;

        [Ignore]
        public List<string> Samples { get; private set; } = new List<string>();


        public MFDriver(FileInfo driverProjectFile)
        {
            if (File.Exists(driverProjectFile.FullName) == false)
            {
                throw new FileNotFoundException($"Driver project not found {driverProjectFile.FullName}");
            }

            //load driver project (for metadata)
            DriverProject = new MFDriverProject(driverProjectFile);

            //load driver code
            var driverDir = driverProjectFile.Directory.GetDirectories("Drivers").FirstOrDefault();
            if (driverDir != null)
            {
                var files = driverDir.GetFiles();
                foreach (var file in files)
                {
                    CodeFiles.Add(new MFDriverCode(file));
                }
            }
            else
            {
                var fileName = GetSimpleName(driverProjectFile) + ".cs";

                var file = Path.Combine(driverProjectFile.DirectoryName, fileName);

                CodeFiles.Add(new MFDriverCode(file));
            }

            //Load documentation
            Documentation = new MFDriverDocumentation(this, Program.MeadowFoundationDocsPath);

            //Load assets
             var parentDir = driverProjectFile.Directory.Parent.Parent;
            Assets = new MFDriverAssets(parentDir);
        }

        static string GetSimpleName(FileInfo file)
        {
            var nameChunks = file.Name.Split('.');

            return nameChunks[nameChunks.Length - 2];
        }
    }
}