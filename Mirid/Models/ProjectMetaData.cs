using System;
namespace Mirid.Models
{
    public class ProjectMetaData
    {
        public string AssemblyName { get; set; }
        public string CompanyName { get; set; }
        public string PackageId { get; set; }
        public string Description { get; set; }
        public string GeneratePackageOnBuild { get; set; }
        public string Version { get; set; }
    }
}
