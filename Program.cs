using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using CsvHelper;
using CsvHelper.Configuration;
using HtmlAgilityPack;

namespace WebCrawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the domain you want to crawl (without https://):");
            string domainInput = Console.ReadLine();
            string domain = $"https://{domainInput}";
            Console.WriteLine($"The final URL being crawled is: {domain}");

            Console.WriteLine("Are you using a custom domain for public links? (yes/no):");
            string customDomainResponse = Console.ReadLine()?.Trim().ToLower();

            string aprimoPublicLinksDomain;
            if (customDomainResponse == "yes")
            {
                Console.WriteLine("Enter the custom domain for public links:");
                aprimoPublicLinksDomain = Console.ReadLine();
            }
            else
            {
                aprimoPublicLinksDomain = "aprimocdn.net";
            }

            var crawler = new Crawler(aprimoPublicLinksDomain);
            var results = await crawler.CrawlAsync(domain);

            ExportToCsv(results, "crawl_results.csv");
            Console.WriteLine("Results have been exported to crawl_results.csv");
        }

        private static void ExportToCsv(List<PageResult> results, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(results);
            }
        }
    }

    public class Crawler
    {
        private readonly HttpClient _httpClient;
        private readonly HashSet<string> _visitedUrls; // Used to track visited URLs and avoid cyclical links
        private readonly List<PageResult> _pageResults;
        private readonly string _aprimoPublicLinksDomain;
        private int _totalItemsFound = 0;

        public Crawler(string aprimoPublicLinksDomain)
        {
            _httpClient = new HttpClient();
            _visitedUrls = new HashSet<string>();
            _pageResults = new List<PageResult>();
            _aprimoPublicLinksDomain = aprimoPublicLinksDomain;
        }

        public async Task<List<PageResult>> CrawlAsync(string domain)
        {
            await CrawlPageAsync(domain, domain);
            return _pageResults;
        }

        private async Task CrawlPageAsync(string baseUrl, string url)
        {
            if (_visitedUrls.Contains(url)) return; // Skip if URL has already been visited

            _visitedUrls.Add(url);
            Console.WriteLine($"Crawling: {url}");

            // Add a delay to avoid hammering the site
            await Task.Delay(300);

            var pageContent = await GetPageContentAsync(url);
            if (pageContent == null) return;

            var document = new HtmlDocument();
            document.LoadHtml(pageContent);

            var pageTitle = document.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "No Title";

            var itemsFound = 0;

            // Process images
            var imageNodes = document.DocumentNode.SelectNodes("//img");
            if (imageNodes != null)
            {
                foreach (var imageNode in imageNodes)
                {
                    var imageUrl = imageNode.GetAttributeValue("src", "");
                    if (imageUrl.Contains(_aprimoPublicLinksDomain))
                    {
                        _pageResults.Add(new PageResult
                        {
                            PageUrl = url,
                            PageTitle = pageTitle,
                            ItemType = "Image",
                            ItemUrl = HttpUtility.HtmlDecode(imageUrl)
                        });
                        itemsFound++;
                    }
                }
            }

            // Process videos
            var videoNodes = document.DocumentNode.SelectNodes("//video/source");
            if (videoNodes != null)
            {
                foreach (var videoNode in videoNodes)
                {
                    var videoUrl = videoNode.GetAttributeValue("src", "");
                    if (videoUrl.Contains(_aprimoPublicLinksDomain))
                    {
                        _pageResults.Add(new PageResult
                        {
                            PageUrl = url,
                            PageTitle = pageTitle,
                            ItemType = "Video",
                            ItemUrl = HttpUtility.HtmlDecode(videoUrl)
                        });
                        itemsFound++;
                    }
                }
            }

            // Process anchor links
            var anchorNodes = document.DocumentNode.SelectNodes("//a[@href]");
            if (anchorNodes != null)
            {
                foreach (var anchorNode in anchorNodes)
                {
                    var anchorUrl = anchorNode.GetAttributeValue("href", "");
                    if (anchorUrl.Contains(_aprimoPublicLinksDomain))
                    {
                        _pageResults.Add(new PageResult
                        {
                            PageUrl = url,
                            PageTitle = pageTitle,
                            ItemType = "Anchor",
                            ItemUrl = HttpUtility.HtmlDecode(anchorUrl)
                        });
                        itemsFound++;
                    }
                }
            }

            _totalItemsFound += itemsFound;
            Console.WriteLine($"Found {itemsFound} items on this page. Total items found: {_totalItemsFound}");

            // Crawl further links
            var linkNodes = document.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes != null)
            {
                foreach (var linkNode in linkNodes)
                {
                    var href = linkNode.GetAttributeValue("href", "");
                    var absoluteUrl = GetAbsoluteUrl(baseUrl, href);
                    if (absoluteUrl != null && absoluteUrl.StartsWith(baseUrl) && !_visitedUrls.Contains(absoluteUrl))
                    {
                        await CrawlPageAsync(baseUrl, absoluteUrl);
                    }
                }
            }
        }

        private async Task<string> GetPageContentAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error crawling {url}: {ex.Message}");
                return null;
            }
        }

        private string GetAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) &&
                Uri.TryCreate(baseUri, relativeUrl, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }
            return null;
        }
    }

    public class PageResult
    {
        public string PageUrl { get; set; }
        public string PageTitle { get; set; }
        public string ItemType { get; set; }
        public string ItemUrl { get; set; }
    }
}
