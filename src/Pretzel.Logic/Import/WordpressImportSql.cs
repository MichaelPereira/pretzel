using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Abstractions;

namespace Pretzel.Logic.Import
{
    public class WordpressImportSql
    {
        private readonly IFileSystem fileSystem;
        private readonly string pathToSite;
        private readonly string pathToImportFile;

        public WordpressImportSql(IFileSystem fileSystem, string pathToSite, string pathToImportFile)
        {
            this.fileSystem = fileSystem;
            this.pathToSite = pathToSite;
            this.pathToImportFile = pathToImportFile;
        }

        public void Import()
        {
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(pathToImportFile);
            while ((line = file.ReadLine()) != null)
            {
                
                counter++;
            }

            file.Close();
        }
    }
}
