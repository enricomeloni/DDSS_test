using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CurlieIndex
{
    public class Curlie
    {
        private const string CurlieHomepage = "http://curlie.org";

        private HtmlWeb CurlieWeb { get; } = new HtmlWeb();

        //todo: should be fast to index to find already existing categories
        private List<Category> Categories { get; } = new List<Category>();

        public void Begin()
        {
            var curlieHomePageDoc = CurlieWeb.Load(CurlieHomepage);
            var rootNode = curlieHomePageDoc.DocumentNode;

            var categorySectionNode = rootNode.SelectSingleNode("//section[@id='category-section']");
            var categoryAsides = categorySectionNode.SelectNodes("aside");

            foreach (var categoryAside in categoryAsides)
            {
                ParseRootCategory(categoryAside);
                break;
            }

        }

        private void ParseRootCategory(HtmlNode categoryNode)
        {
            var aNode = categoryNode.SelectSingleNode("div/h2/a");


            var name = aNode.InnerText;
            var url = aNode.Attributes["href"].Value;

            var category = new Category()
            {
                Name = name,
                Url = url,
                Parent = null
            };
            Categories.Add(category);

            ParseSubCategories(category);

        }

        private void ParseSubCategories(Category rootCategory)
        {
            //read category page to find subcategories, related and other languages
            var categoryRoot = CurlieWeb.Load(GetCategoryUrl(rootCategory.Url)).DocumentNode;
            var subcategoriesSection = categoryRoot.SelectSingleNode("//div[@id='cat-list-content-main']");

            var catItems = subcategoriesSection.SelectNodes("div[@class='cat-item']");

            foreach (var catItem in catItems)
            {
                var aNode = catItem.SelectSingleNode("a");
                var url = aNode.Attributes["href"].Value;

                var name = aNode.SelectSingleNode("div/i").NextSibling.InnerText.Trim();

                var category = new Category()
                {
                    Name = name,
                    Url = url,
                    Parent = rootCategory
                };
                Categories.Add(category);
            }
        }

        private string GetCategoryUrl(string url)
        {
            return CurlieHomepage + url;
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.  
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}