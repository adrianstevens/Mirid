using System;
using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFDriverAssets
    {
        public bool HasDataSheet { get; private set; }

        public int NumberOfSamples { get; private set; }

        public MFDriverAssets(DirectoryInfo directory)
        {
            var datasheetDir = directory.GetDirectories("Datasheet*").FirstOrDefault();

            if (datasheetDir != null)
            {
                if (datasheetDir.GetFiles().Count() > 0)
                {
                    HasDataSheet = true;
                }
            }

            var samplesDir = directory.GetDirectories("Samples").FirstOrDefault();

            if (samplesDir != null)
            {
                NumberOfSamples = samplesDir.GetDirectories().Count();
            }
        }
    }
}