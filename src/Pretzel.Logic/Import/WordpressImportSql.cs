using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Pretzel.Logic.Extensions;

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
            // Capture information for published posts
            Regex postRegex = new Regex(@"INSERT INTO `wp_posts` VALUES \((?<postId>\d+), (?<authorId>\d+), '[^']+', '(?<dategmt>[^']+)', '(?<content>.*)', '(?<title>.*)', '.*', 'publish', '[^']*', '[^']*', '[^']*', '(?<postName>[^']*)'");
            IList<WordpressPost> postList = new List<WordpressPost>();
            
            string[] lines = fileSystem.File.ReadAllLines(pathToImportFile);

            foreach (string line in lines)
            {
                Match match = postRegex.Match(line);
                if (match.Success)
                {
                    WordpressPost wpPost = new WordpressPost{
                        authorId = int.Parse(match.Groups["authorId"].Value),
                        postId = int.Parse(match.Groups["postId"].Value),
                        Content = match.Groups["content"].Value,
                        PostName = match.Groups["postName"].Value,
                        Published = Convert.ToDateTime(match.Groups["dategmt"].Value),
                        Title = match.Groups["title"].Value
};                    
                    postList.Add(wpPost);
                    ImportPost(wpPost);
                }
            }
        }

        private void ImportPost(WordpressPost p)
        {
            var header = new
            {
                title = p.Title,
                author = p.Author,
                date = p.Published,
                layout = "post",
                categories = p.Categories,
                tags = p.Tags
            };

            var yamlHeader = string.Format("---\r\n{0}---\r\n\r\n", header.ToYaml());
            var postContent = yamlHeader + p.Content; //todo would be nice to convert to proper md
            var fileName = string.Format(@"_posts\{0}-{1}.md", p.Published.ToString("yyyy-MM-dd"), p.PostName.Replace(' ', '-')); //not sure about post name

            fileSystem.File.WriteAllText(Path.Combine(pathToSite, fileName), postContent);
        }


        protected class WordpressPost
        {
            public int postId { get; set; }
            public int authorId { get; set; }
            public String Author { get; set; }
            public string Title { get; set; }
            public string PostName { get; set; }
            public DateTime Published { get; set; }
            public string Content { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public IEnumerable<string> Categories { get; set; }
        }
    }
}
