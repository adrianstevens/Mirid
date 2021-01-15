using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration.Attributes;

namespace Mirid.Models
{
    public class MeadowDriver
    {
        [Index(0)]
        public string PackageName { get; set; }
        [Index(1)]
        public int NumberOfDrivers => DriverFiles?.Length ?? 0;
        [Index(2)]
        public bool IsTested => CsProjMetadata?.GeneratePackageOnBuild == "true";
        [Index(3)]
        public bool HasCompleteMetaData => IsMetadataComplete();
        [Index(4)]
        public bool HasDataSheet { get; set; }
        [Index(5)]
        public int NumberOfSamples { get; set; }
        [Index(6)]
        public bool HasTestSuite { get; set; }
        [Index(7)]
        public bool HasDocOverride { get; set; }

        [Ignore]
        public ProjectMetaData CsProjMetadata { get; set; } = new ProjectMetaData();

        [Ignore]
        public string ProjectPath { get; set; }
        [Ignore]
        public string Namespace { get; set; }
        [Ignore]
        public List<string> Samples { get; private set; } = new List<string>();
        [Ignore]
        public FileInfo[] DriverFiles { get; set; }

        public bool IsMetadataComplete()
        {
            if(CsProjMetadata == null) { return false; }
            if(string.IsNullOrWhiteSpace(CsProjMetadata.AssemblyName)) { return false; }
            if (string.IsNullOrWhiteSpace(CsProjMetadata.CompanyName)) { return false; }
            if (string.IsNullOrWhiteSpace(CsProjMetadata.Description)) { return false; }
            if (string.IsNullOrWhiteSpace(CsProjMetadata.PackageId)) { return false; }

            return true;

        }
    }
}