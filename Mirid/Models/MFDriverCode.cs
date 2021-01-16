using System.IO;

namespace Mirid.Models
{
    public class MFDriverCode
    {
        public int LineCount => lines?.Length ?? 0;

        public string Namespace
        {
            get
            {
                if(string.IsNullOrEmpty(_namespace))
                {
                    _namespace = GetNamespace();
                }
                return _namespace;
            }
        }
        string _namespace;

        string path;

        string[] lines;

        public MFDriverCode(FileInfo driverFile)
        {
            ReadCodeFile(path = driverFile.FullName);
        }

        public MFDriverCode(string filePath)
        {
            ReadCodeFile(path = filePath);
        }

        void ReadCodeFile(string filePath)
        {
            if(File.Exists(filePath) == false)
            {
                throw new FileNotFoundException($"Couldn't find driver file {filePath}");
            }

            lines = File.ReadAllLines(filePath);
        }

        string GetNamespace()
        {
            foreach (var line in lines)
            {
                if (line.Contains("namespace"))
                {
                    return line.Substring("namespace ".Length);
                }
            }

            return string.Empty;
        }
    }
}