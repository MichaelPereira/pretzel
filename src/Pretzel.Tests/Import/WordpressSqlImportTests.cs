using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic;
using Pretzel.Logic.Import;
using Pretzel.Logic.Extensions;

namespace Pretzel.Tests.Import
{
    public class WordpressSqlImportTests
    {
        const string BaseSite = @"c:\site\";
        const string ImportFile = @"c:\import.sql";
        const string ImportContent = @"
INSERT INTO `wp_term_relationships` VALUES (28, 11, 0); 
INSERT INTO `wp_term_relationships` VALUES (28, 10, 0); 
...
INSERT INTO `wp_term_taxonomy` VALUES (10, 10, 'category', '', 0, 1); 
INSERT INTO `wp_term_taxonomy` VALUES (11, 11, 'post_tag', '', 0, 1); 
...
INSERT INTO `wp_terms` VALUES (10, 'Cloud', 'cloud', 0); 
INSERT INTO `wp_terms` VALUES (11, 'cloud', 'cloud-2', 0); 
...
INSERT INTO `wp_posts` VALUES (2, 1, '2010-09-28 15:16:26', '2010-09-28 15:16:26', 'This is an example of a WordPress page, you could edit this to put information about yourself or your site so readers know where you are coming from. You can create as many pages like this one or sub-pages as you like and manage all of your content inside of WordPress.', 'About', '', 'publish', 'open', 'open', '', 'about', '', '', '2010-09-28 15:16:26', '2010-09-28 15:16:26', '', 0, 'http://www.michaelpereira.fr/blog/?page_id=2', 0, 'page', '', 0); 
INSERT INTO `wp_posts` VALUES (29, 1, '2010-12-08 00:13:12', '2010-12-07 23:13:12', '', 'jolicloud-11-launcher', '', 'inherit', 'open', 'open', '', 'jolicloud-11-launcher', '', '', '2010-12-08 00:13:12', '2010-12-07 23:13:12', '', 28, 'http://www.michaelpereira.fr/blog/wp-content/uploads/2010/12/jolicloud-11-launcher.png', 1, 'attachment', 'image/png', 0); 
INSERT INTO `wp_posts` VALUES (27, 1, '2010-11-22 01:05:15', '2010-11-22 00:05:15', 'This is an example of a WordPress page, you could edit this to put information about yourself or your site so readers know where you are coming from. You can create as many pages like this one or sub-pages as you like and manage all of your content inside of WordPress.', 'About', '', 'inherit', 'open', 'open', '', '2-autosave', '', '', '2010-11-22 01:05:15', '2010-11-22 00:05:15', '', 2, 'http://www.michaelpereira.fr/blog/2010/11/22/2-autosave/', 0, 'revision', '', 0); 
INSERT INTO `wp_posts` VALUES (6, 1, '2010-09-28 16:54:37', '2010-09-28 16:54:37', 'http://www.michaelpereira.fr/blog/wp-content/uploads/2010/09/cropped-portrait.jpg', 'cropped-portrait.jpg', '', 'inherit', 'closed', 'open', '', 'cropped-portrait-jpg', '', '', '2010-09-28 16:54:37', '2010-09-28 16:54:37', '', 0, 'http://www.michaelpereira.fr/blog/wp-content/uploads/2010/09/cropped-portrait.jpg', 0, 'attachment', 'image/jpeg', 0); 
INSERT INTO `wp_posts` VALUES (28, 1, '2010-12-08 00:35:19', '2010-12-07 23:35:19', 'Vous possédez un vieil ordinateur de bureau ou portable que vous n\'utilisez plus car il est trop lent, même pour accéder au Web ? Vous pouvez maintenant lui redonner une seconde vie avec JoliCloud dont la version 1.1 vient de sortir !\r\n\r\n<a href=""http://www.michaelpereira.fr/blog/wp-content/uploads/2010/12/jolicloud-11-launcher.png""><img class=""aligncenter size-full wp-image-29"" title=""jolicloud-11-launcher"" src=""http://www.michaelpereira.fr/blog/wp-content/uploads/2010/12/jolicloud-11-launcher.png"" alt=""bureau Jolicloud"" width=""1024"" height=""600"" /></a>\r\n\r\nAlors certes l\'interface a une vague ressemblance avec celle de l\'iphone avec ses grosses icônes et ses pages, mais la ressemblance s\'arrête là. Basé sur les dernières versions d\'Ubuntu, Jolicloud cache tout le système derrière les applications, ce qui le rend donc très facile d\'utilisation pour les utilisateurs novices ou inexpérimentés.\r\n\r\nJolicloud est aussi très axé Web 2.0 et autres services utilisant le cloud, ce qui permet d\'utiliser le moins de place possible sur l\'ordinateur, et permet d\'avoir ses données disponibles en permanence sur le web. Ainsi on dispose des applications Google Docs, Dropbox, Flickr, Skype, et une page permet de suivre les flux d\'informations de vos contacts Facebook et Twitter.\r\n\r\nAvec cette <a href=""http://www.jolicloud.com/blog/2010/12/07/with-jolicloud-11-lets-reinvent-the-computing-experience-for-millions/"">nouvelle version 1.1 de Jolicloud</a>, les améliorations ont été apportées sur les performances de l\'OS, une plus grande simplicité d\'utilisation et une plus grande variété d\'appareils compatibles.\r\n\r\nJe l\'ai essayé en version 1.0  sur un vieux pc portable HP avec une carte wifi externe, et il marche parfaitement. De plus la mise à jour vers le version 1.1 se fait de manière totalement transparente, les nouvelles fonctionnalités apparaissent au fur et à mesure de leur téléchargement.\r\n\r\nPour les plus prudents, vous pouvez essayer cet OS avec le live CD intégré ou en parallèle de windows, les deux versions sont disponibles sur la <a href=""http://www.jolicloud.com/download"">page de téléchargement</a>.\r\n\r\nIl ne reste plus qu\'à mettre à jour l\'<a href=""http://fr.wikipedia.org/wiki/Jolicloud"">article Wikipédia</a> !', 'Recyclez votre vieux PC avec Jolicloud 1.1', 'Vous possédez un vieil ordinateur de bureau ou portable que vous n\'utilisez plus car il est trop lent, même pour accéder au Web ? Vous pouvez maintenant lui redonner une seconde vie avec JoliCloud dont la version 1.1 vient de sortir !', 'publish', 'open', 'open', '', 'recyclez-votre-vieux-pc-avec-jolicloud-1-1', '', '\nhttp://www.jolicloud.com/blog/2010/12/07/with-jolicloud-11-lets-reinvent-the-computing-experience-for-millions/', '2010-12-08 00:39:36', '2010-12-07 23:39:36', '', 0, 'http://www.michaelpereira.fr/blog/?p=28', 0, 'post', '', 1);";
        public WordpressSqlImportTests()
        {

        }
        [Fact]
        public void Posts_Are_Imported()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { ImportFile, new MockFileData(ImportContent) }
            });

            var wordpressImporter = new WordpressImportSql(fileSystem, BaseSite, ImportFile);
            wordpressImporter.Import();

            Assert.True(fileSystem.File.Exists(BaseSite + "_posts\\2010-09-28-about.md"));
            Assert.True(fileSystem.File.Exists(BaseSite + "_posts\\2010-12-07-recyclez-votre-vieux-pc-avec-jolicloud-1-1.md"));

            var postContentAbout = fileSystem.File.ReadAllText(BaseSite + "_posts\\2010-09-28-about.md");
            var headerAbout = postContentAbout.YamlHeader();

            Assert.Equal("About", headerAbout["title"].ToString());

            var postContentJolicloud = fileSystem.File.ReadAllText(BaseSite + "_posts\\2010-12-07-recyclez-votre-vieux-pc-avec-jolicloud-1-1.md");
            var headerJolicloud = postContentJolicloud.YamlHeader();

            Assert.Equal("Recyclez votre vieux PC avec Jolicloud 1.1", headerJolicloud["title"].ToString());
            Assert.NotNull(headerJolicloud["tags"]);
            Assert.NotNull(headerJolicloud["categories"]);
        }
    }
}