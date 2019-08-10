using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CurlieIndex
{
    public class Curlie
    {
        private const string CurlieHomepage = "http://curlie.org";

        private HtmlWeb CurlieWeb { get; } = new HtmlWeb();
        private CurlieWebClient CurlieWebClient { get; } = new CurlieWebClient();

        //todo: should be fast to index to find already existing categories
        private ConcurrentDictionary<string, Category> Categories { get; } = new ConcurrentDictionary<string, Category>();

        public async Task Begin()
        {
            //var curlieHomePageDoc = CurlieWeb.Load(CurlieHomepage);
            CurlieWebClient.Start();
            var curlieHomePageDoc = await CurlieWebClient.LoadPage(CurlieHomepage);
            var rootNode = curlieHomePageDoc.DocumentNode;

            var categorySectionNode = rootNode.SelectSingleNode("//section[@id='category-section']");
            var categoryAsides = categorySectionNode.SelectNodes("aside");

            //for demo purposes skip Arts which is extra big
            bool skipped = false;
            foreach (var categoryAside in categoryAsides)
            {
                if (!skipped)
                {
                    skipped = true;
                    continue;
                }
                await ParseRootCategory(categoryAside);
                break;
            }

            try
            {
                WriteToCsv();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return;
        }

        private async Task ParseRootCategory(HtmlNode categoryNode)
        {
            var aNode = categoryNode.SelectSingleNode("div/h2/a");
            
            var name = aNode.InnerText;
            var url = aNode.Attributes["href"].Value;

            Console.WriteLine($"Extracting root category {url}");
            var category = new Category
            {
                Name = name,
                Url = url
            };
            if (Categories.TryAdd(category.Url, category))
            {
                await ParseSubCategories(category);
            }
        }

        private async Task ParseSubCategories(Category rootCategory)
        {
            Console.WriteLine($"Extracting subcategories for {rootCategory.Url}");
            //read category page to find subcategories, related and other languages
            var categoryPage = await CurlieWebClient.LoadPage(GetCategoryFullUrl(rootCategory.Url));
            var categoryRoot = categoryPage.DocumentNode;


            //parse subcategories
            var subcategoriesDiv = categoryRoot.SelectSingleNode("//div[@id='subcategories-div']");

            //if category has no subcategory, just return
            if (subcategoriesDiv == null) return;

            var subcategoriesSections = subcategoriesDiv.SelectNodes("section[@class='children']");

            var catItems = new List<HtmlNode>();
            foreach (var subcategoriesSection in subcategoriesSections)
            {
                catItems.AddRange(subcategoriesSection.SelectNodes("div/div[@class='cat-item']"));
            }
            
            //for demo purposes, we will extract only 1 subcategory per category
            int categoriesExtracted = 0;
            int maxSubcategories = 4;

            foreach (var catItem in catItems)
            {
                if (categoriesExtracted >= maxSubcategories)
                    break;

                var aNode = catItem.SelectSingleNode("a");
                var url = aNode.Attributes["href"].Value;

                var name = aNode.SelectSingleNode("div/i").NextSibling.InnerText.Trim();

                var category = new Category()
                {
                    Name = name,
                    Url = url
                };

                category.Parents.Add(rootCategory);
                if (Categories.TryAdd(category.Url, category))
                {
                    await ParseSubCategories(category);
                }
                else
                {
                    Categories[category.Url].Parents.Add(rootCategory);
                }

                ++categoriesExtracted;
                Console.WriteLine($"Progress for {rootCategory.Url}: {categoriesExtracted}/{maxSubcategories}");
            }
        }

        private void WriteToCsv()
        {
            using (var stream = new StreamWriter(@"./output.csv"))
            {
                //write header
                stream.WriteLine("id,name,url,primary_parent,secondary_parents");

                var categories = Categories.Values.ToList();
                categories.Sort((category1, category2) => 
                    category1.Id.CompareTo(category2.Id));

                foreach (var category in categories)
                {
                    string primaryParent = "null";
                    string parents = "null";
                    if (category.Parents.Count != 0)
                    {
                        primaryParent = category.PrimaryParent?.Id.ToString();
                        var secondaryParents = category.Parents
                            .Where(parent => parent.Id != category.PrimaryParent?.Id)
                            .Select(parent => parent.Id);

                        parents = string.Join(";", secondaryParents);
                    }

                    stream.WriteLine($"{category.Id},{category.Name},{category.Url},{primaryParent},{parents}");
                }
            }
        }

        private string GetCategoryFullUrl(string url)
        {
            return CurlieHomepage + url;
        }
    }
}