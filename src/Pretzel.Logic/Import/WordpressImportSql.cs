using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Import
{
    public class WordpressImportSql : WordpressImport
    {
        private readonly IFileSystem fileSystem;
        private readonly string pathToSite;
        private readonly string pathToImportFile;

        public WordpressImportSql(IFileSystem fileSystem, string pathToSite, string pathToImportFile)
            : base(fileSystem, pathToSite, pathToImportFile)
        {

            this.fileSystem = fileSystem;
            this.pathToSite = pathToSite;
            this.pathToImportFile = pathToImportFile;
        }

        public void Import()
        {
            int counter = 0;
            string line;
            Regex postRegex = new Regex(@"INSERT INTO `wp_posts` VALUES \((?<postId>\d+), (?<authorId>\d+), '[^']+', '(?<dategmt>[^']+)', '(?<content>.*)', '(?<title>.*)', '.*', 'publish'");
            
            IList<Tuple<int, int, WordpressPost>> postList = new List<Tuple<int, int, WordpressPost>>();
            System.IO.StreamReader file = new System.IO.StreamReader(pathToImportFile);
            while ((line = file.ReadLine()) != null)
            {
                Match match = postRegex.Match(line);
                if (match.Success)
                {                    
                    int postId = int.Parse(match.Groups["postId"].Value);
                    int authorId = int.Parse(match.Groups["authorId"].Value);
                    postList.Add(new Tuple<int, int, WordpressPost>(postId, authorId, new WordpressPost()));
                }
                counter++;
            }

            file.Close();
        }
    }
}
