using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Pretzel.Logic.Extensions;
using System.Collections.ObjectModel;

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
            Regex termRelationshipRegex = new Regex(@"INSERT INTO `wp_term_relationships` VALUES \((?<postId>\d+), (?<termId>\d+),");
            Regex termTypeRegex = new Regex(@"INSERT INTO `wp_term_taxonomy` VALUES \((?<termId>\d+), \d+, '(?<type>[^']+)',");
            Regex termNameRegex = new Regex(@"INSERT INTO `wp_terms` VALUES \((?<termId>\d+), '.*', '(?<termName>[^']+)");

            PostCollection postList = new PostCollection();
            Dictionary<int, Relationship> terms = new Dictionary<int, Relationship>();
            string[] lines = fileSystem.File.ReadAllLines(pathToImportFile);

            foreach (string line in lines)
            {
                Match matchRelationship = termRelationshipRegex.Match(line);
                if (matchRelationship.Success)
                {
                    int termId = int.Parse(matchRelationship.Groups["termId"].Value);
                    int postId = int.Parse(matchRelationship.Groups["postId"].Value);
                    
                    // Check if the term is already in the list
                    if (terms.ContainsKey(termId))
                    {
                        terms[termId].postIds.Add(postId);
                    }
                    else // Create a new relationship between term and post in the list
                    {
                        terms.Add(termId, new Relationship()
                        {
                            postIds = new List<int> {postId}
                        });
                    }
                    continue;
                }

                Match matchType = termTypeRegex.Match(line);
                if (matchType.Success)
                {
                    int termId = int.Parse(matchType.Groups["termId"].Value);
                    
                    // Set the type for termId element in the list
                    if (terms.ContainsKey(termId))
                    {
                        if (matchType.Groups["type"].Value.Equals("category"))
                            terms[termId].termType = termTypeEnum.category;
                        else if (matchType.Groups["type"].Value.Equals("post_tag"))
                            terms[termId].termType = termTypeEnum.tag;
                    }
                    else
                    {
                        // TODO: Handle error case
                    }
                    continue;
                }

                Match matchName = termNameRegex.Match(line);
                if (matchName.Success)
                {
                    int termId = int.Parse(matchName.Groups["termId"].Value);

                    // Set the type for termId element in the list
                    if (terms.ContainsKey(termId))
                    {
                        terms[termId].termName = matchName.Groups["termName"].Value;
                    }
                    else
                    {
                        // TODO: Handle error case
                    }
                    continue;
                }

                Match matchPost = postRegex.Match(line);
                if (matchPost.Success)
                {
                    WordpressPost wpPost = new WordpressPost
                    {
                        authorId = int.Parse(matchPost.Groups["authorId"].Value),
                        postId = int.Parse(matchPost.Groups["postId"].Value),
                        Categories = new List<string>(),
                        Content = matchPost.Groups["content"].Value,
                        PostName = matchPost.Groups["postName"].Value,
                        Published = Convert.ToDateTime(matchPost.Groups["dategmt"].Value),
                        Tags = new List<string>(),
                        Title = matchPost.Groups["title"].Value,
                    };
                    postList.Add(wpPost);
                    continue;
                }
            }

            // Add categories and tags for each post
            foreach (int key in terms.Keys)
            {
                Relationship term = terms[key];
                foreach (int postId in term.postIds)
                {
                    if (term.termType == termTypeEnum.category)
                        postList[postId].Categories.Add(term.termName);
                    else if (term.termType == termTypeEnum.tag)
                        postList[postId].Tags.Add(term.termName);
                }
            }

            // Import each post
            foreach (WordpressPost wpPost in postList)
            {
                ImportPost(wpPost);
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

        protected class PostCollection : KeyedCollection<int, WordpressPost>
        {
            public PostCollection() : base() { }

            protected override int GetKeyForItem(WordpressPost item)
            {
                return item.postId;
            }
        }

        protected class WordpressPost
        {
            public int postId { get; set; }
            public int authorId { get; set; }
            public string Author { get; set; }
            public string Title { get; set; }
            public string PostName { get; set; }
            public DateTime Published { get; set; }
            public string Content { get; set; }
            public List<string> Tags { get; set; }
            public List<string> Categories { get; set; }
        }

        public enum termTypeEnum { category, tag };

        protected class Relationship
        {
            public string termName { get; set; }
            public termTypeEnum termType { get; set; }
            public List<int> postIds { get; set; }
        }
    }
}
