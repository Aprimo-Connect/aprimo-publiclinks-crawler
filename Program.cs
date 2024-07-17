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

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var domainClean = new Uri(domain).Host.Replace(".", "_");
            var publicLinksDomainClean = aprimoPublicLinksDomain.Replace(".", "_");

            var resultsFileName = $"{domainClean}_{publicLinksDomainClean}_{timestamp}_results.csv";
            var errorsFileName = $"{domainClean}_{publicLinksDomainClean}_{timestamp}_errors.csv";

            ExportToCsv(results, resultsFileName);
            ExportToCsv(crawler.Errors, errorsFileName);
            Console.WriteLine($"Results have been exported to {resultsFileName} and {errorsFileName}");
        }

        private static void ExportToCsv<T>(List<T> results, string filePath)
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
        private readonly List<PageResult> _pageResults;
        private readonly List<ErrorResult> _errors;
        private readonly HashSet<string> _visitedUrls; // Used to track visited URLs and avoid cyclical links
        private readonly string _aprimoPublicLinksDomain;
        private int _totalItemsFound = 0;

        public List<ErrorResult> Errors => _errors;

        public Crawler(string aprimoPublicLinksDomain)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _visitedUrls = new HashSet<string>();
            _pageResults = new List<PageResult>();
            _errors = new List<ErrorResult>();
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
                    var imageUrl = HttpUtility.HtmlDecode(imageNode.GetAttributeValue("src", ""));
                    if (imageUrl.Contains(_aprimoPublicLinksDomain))
                    {
                        var fileSize = await GetFileSizeAsync(imageUrl, url, "Image");
                        _pageResults.Add(new PageResult
                        {
                            PageUrl = url,
                            PageTitle = pageTitle,
                            ItemType = "Image",
                            ItemUrl = imageUrl,
                            FileSize = fileSize
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
                    var videoUrl = HttpUtility.HtmlDecode(videoNode.GetAttributeValue("src", ""));
                    if (videoUrl.Contains(_aprimoPublicLinksDomain))
                    {
                        var fileSize = await GetFileSizeAsync(videoUrl, url, "Video");
                        _pageResults.Add(new PageResult
                        {
                            PageUrl = url,
                            PageTitle = pageTitle,
                            ItemType = "Video",
                            ItemUrl = videoUrl,
                            FileSize = fileSize
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
                    var anchorUrl = HttpUtility.HtmlDecode(anchorNode.GetAttributeValue("href", ""));
                    if (anchorUrl.Contains(_aprimoPublicLinksDomain))
                    {
                        var fileSize = await GetFileSizeAsync(anchorUrl, url, "Anchor");
                        _pageResults.Add(new PageResult
                        {
                            PageUrl = url,
                            PageTitle = pageTitle,
                            ItemType = "Anchor",
                            ItemUrl = anchorUrl,
                            FileSize = fileSize
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
                    var href = HttpUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));
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

        private async Task<long?> GetFileSizeAsync(string url, string pageUrl, string itemType)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength.HasValue)
                    {
                        return response.Content.Headers.ContentLength.Value;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _errors.Add(new ErrorResult
                        {
                            PageUrl = pageUrl,
                            ErrorMessage = "404 Not Found",
                            ItemType = itemType,
                            ItemUrl = url
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file size for {url}: {ex.Message}");
                _errors.Add(new ErrorResult
                {
                    PageUrl = pageUrl,
                    ErrorMessage = ex.Message,
                    ItemType = itemType,
                    ItemUrl = url
                });
            }
            return null;
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
        public long? FileSize { get; set; }
    }

    public class ErrorResult
    {
        public string PageUrl { get; set; }
        public string ItemType { get; set; }
        public string ItemUrl { get; set; }
        public string ErrorMessage { get; set; }
    }
}
