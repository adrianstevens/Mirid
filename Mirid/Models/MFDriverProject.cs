using System.IO;

namespace Mirid.Models
{
    public class MFDriverProject
    {
        public string AssemblyName { get; private set; }
        public string CompanyName { get; private set; }
        public string PackageId { get; private set; }
        public string Description { get; private set; }
        public string GeneratePackageOnBuild { get; private set; }
        public string Version { get; private set; }

        //private
        string projectText;

        public MFDriverProject(string path)
        {
            LoadDriverText(path);
            ParseElements();
        }

        public MFDriverProject(FileInfo fileInfo)
        {
            LoadDriverText(fileInfo.FullName);
            ParseElements();
        }

        public bool IsMetadataComplete()
        {
            if (string.IsNullOrWhiteSpace(AssemblyName)) { return false; }
            if (string.IsNullOrWhiteSpace(CompanyName)) { return false; }
            if (string.IsNullOrWhiteSpace(Description)) { return false; }
            if (string.IsNullOrWhiteSpace(PackageId)) { return false; }

            return true;
        }

        //could do this on demand but I'm not really worried about memory
        void ParseElements()
        {
            AssemblyName = GetElement("AssemblyName");
            CompanyName = GetElement("CompanyName");
            Version = GetElement("Version");
            PackageId = GetElement("PackageId");
            Description = GetElement("Description");
        }

        void LoadDriverText(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return; //for now
            }

            if (File.Exists(path) == false)
            {
                throw new FileNotFoundException($"Couldn't find driver project {path}");
            }

            projectText = File.ReadAllText(path);
        }

        string GetElement(string element)
        {
            int index = projectText.IndexOf(element);

            if (index == -1)
            {
                //    Console.WriteLine($"Could not find {element} for {name}");
                return string.Empty;
            }

            int start = projectText.IndexOf(">", index) + 1;
            int end = projectText.IndexOf("<", start);

            return projectText.Substring(start, end - start);
        }
    }
}
