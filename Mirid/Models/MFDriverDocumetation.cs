using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFDriverDocumentation
    {
        public string DocsFileName { get; private set; }
        public string FullPath { get; private set; }

        public bool HasOverride => lines?.Length > 0;

        MFDriver driver;
        string documentationPath;

        string[] lines;


        public MFDriverDocumentation(MFDriver driver, string docsPath)
        {
            documentationPath = docsPath;
            this.driver = driver;

            ReadDocsFile();
        }

        public void ReadDocsFile()
        {
            //  var override = Path.Combine()
            var simpleName = driver.PackageName.Split('.').LastOrDefault();

            DocsFileName = driver.Namespace + "." + simpleName + ".md";
            FullPath = Path.Combine(documentationPath, DocsFileName);

            if(File.Exists(FullPath))
            {
                lines = File.ReadAllLines(FullPath); //ready for processing
            }
        }
    }
}