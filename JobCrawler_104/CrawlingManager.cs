using HtmlAgilityPack;
using JobCrwaler_104;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace JobCrawler_104
{
    public class CrawlingManager
    {
        private readonly HttpClient _httpClient;
        private readonly HtmlDocument _htmlDocument;
        private readonly SeleniumTool _seleniumTool;
        private readonly ExclusionList _exclusionList;
        private readonly CrawlingSettings _settings;

        public CrawlingManager(HtmlDocument htmlDocument, ExclusionList exclusionList, HttpClient httpClient, SeleniumTool seleniumTool, CrawlingSettings settings)
        {
            _exclusionList = exclusionList;
            _httpClient = httpClient;
            _htmlDocument = htmlDocument;
            _seleniumTool = seleniumTool;
            _settings = settings;
        }

        public async Task<(int OriginCount, int FilteredCount)> ProcessAsync(string url, string browserPreference)
        {
            await Console.Out.WriteLineAsync($"正在蒐集 {_settings.StartDate:MMdd} 至 {_settings.EndDate:MMdd} 的資料...\n");

            int totalPages = await FetchTotalPageCountAsync(url);
            if (totalPages <= 0)
            {
                return default;
            }

            var pageUrls = GeneratePageUrls(url, totalPages);
            await Console.Out.WriteLineAsync("正在進行爬蟲作業...\n");

            var (originCount, filteredJobList) = await CrawlWithFilterAsync(pageUrls);
            await Console.Out.WriteLineAsync($"\n原始資料共 {originCount} 筆，篩選後 {filteredJobList.Count} 筆\n");

            var outputHtml = GenerateOutputHtml(filteredJobList);
            await OpenBrowserSimulation(outputHtml, url, browserPreference);

            return (originCount, filteredJobList.Count);
        }

        private async Task<int> FetchTotalPageCountAsync(string targetUrl)
        {
            try
            {
                var firstPageContent = await _httpClient.GetStringAsync(targetUrl);
                var totalPageMatch = Regex.Match(firstPageContent, "\"totalPage\":(\\d+)");

                if (totalPageMatch.Success)
                {
                    return int.Parse(totalPageMatch.Groups[1].Value);
                }

                throw new InvalidOperationException("無法解析總頁數");
            }
            catch (Exception ex) when (ex is HttpRequestException or FormatException or InvalidOperationException)
            {
                await Console.Out.WriteLineAsync("URL 錯誤！請輸入正確的 104 職缺搜尋連結。\n程序將強制終止...");
                return -1;
            }
        }

        private IEnumerable<string> GeneratePageUrls(string targetUrl, int totalPages)
        {
            var baseUrl = targetUrl.Split("&page=").FirstOrDefault()
                ?? throw new ArgumentException("URL 格式不正確");

            return Enumerable.Range(1, totalPages).Select(page => $"{baseUrl}&page={page}");
        }

        private async Task<(int OriginCount, IList<string> FilteredJobs)> CrawlWithFilterAsync(IEnumerable<string> urls)
        {
            int count = 1;
            int totalPages = urls.Count();
            int totalOriginCount = 0;
            var filteredJobList = new List<string>();

            foreach (var url in urls)
            {
                var (row, col) = Console.GetCursorPosition();
                await Console.Out.WriteLineAsync($"每 {_settings.IntervalInSeconds} 秒擷取第 {count++} / {totalPages} 頁職缺資訊中...");
                Console.SetCursorPosition(row, col);

                var jobNodes = await FetchJobNodesAsync(url);
                totalOriginCount += jobNodes.Count;

                AddJobWithFilter(filteredJobList, jobNodes);

                await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalInSeconds));
            }

            return (totalOriginCount, filteredJobList);
        }

        private async Task<HtmlNodeCollection> FetchJobNodesAsync(string url)
        {
            var pageContent = await _httpClient.GetStringAsync(url);
            _htmlDocument.LoadHtml(pageContent);

            var jobNodes = _htmlDocument.DocumentNode.SelectNodes("//*[@id=\"js-job-content\"]/article");
            if (jobNodes == null)
            {
                throw new NullReferenceException("無法在頁面中找到職缺資料的節點");
            }

            return jobNodes;
        }

        private void AddJobWithFilter(ICollection<string> jobList, HtmlNodeCollection jobNodes)
        {
            foreach (var jobNode in jobNodes)
            {
                if (ShouldExcludeJob(jobNode))
                {
                    continue;
                }

                if (!IsDateInRange(jobNode))
                {
                    continue;
                }

                jobList.Add(jobNode.OuterHtml);
            }
        }

        private bool ShouldExcludeJob(HtmlNode jobNode)
        {
            var jobTitle = jobNode.GetAttributeValue("data-job-name", string.Empty).ToUpperInvariant();
            var companyName = jobNode.GetAttributeValue("data-cust-name", string.Empty);
            var industryName = jobNode.GetAttributeValue("data-indcat-desc", string.Empty);

            if (string.IsNullOrWhiteSpace(jobTitle) ||
                _exclusionList.Industries.Contains(industryName) ||
                _exclusionList.CompanyNamesForCSharp.Contains(companyName) ||
                _exclusionList.DeferredCompaniesForCSharp.Contains(companyName) ||
                _exclusionList.SubmittedCompaniesForCSharp.Contains(companyName) ||
                _exclusionList.CompanyKeywords.Any(key => companyName.Contains(key)) ||
                _exclusionList.JobTitleKeywordsForCSharp.Any(key => jobTitle.Contains(key)))
            {
                return true;
            }

            return false;
        }


        private bool IsDateInRange(HtmlNode jobNode)
        {
            var dateText = jobNode.SelectSingleNode(".//span[@class='b-tit__date']")?.InnerText.Trim();
            if (DateOnly.TryParseExact(dateText, "M/dd", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var jobDate))
            {
                return jobDate >= _settings.StartDate && jobDate <= _settings.EndDate;
            }

            return false;
        }

        private string GenerateOutputHtml(IEnumerable<string> filteredJobNodes)
        {
            var sb = new StringBuilder();
            foreach (var jobNodeHtml in filteredJobNodes)
            {
                sb.Append(jobNodeHtml);
            }

            return sb.ToString();
        }

        private async Task OpenBrowserSimulation(string outputHtml, string targetUrl, string browserPreference)
        {
            var isFirefox = DetermineBrowserPreference(browserPreference);
            var xPath = "//*[@id=\"js-job-content\"]";

            await Console.Out.WriteLineAsync($"開啟{(isFirefox ? "火狐" : "Chrome")}瀏覽器模擬中...\n");
            await _seleniumTool.Process(targetUrl, xPath, outputHtml, isFirefox);
        }

        private bool DetermineBrowserPreference(string browserPreference)
        {
            return browserPreference.Trim() switch
            {
                "1" => true, // Firefox
                "2" => false, // Chrome
                _ => true
            };
        }
    }
}
