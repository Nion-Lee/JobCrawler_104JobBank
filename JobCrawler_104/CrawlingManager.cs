using HtmlAgilityPack;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace JobCrwaler_104
{
    public class CrawlingManager
    {
        private readonly BlackList _blackList;
        private readonly HttpClient _httpClient;
        private readonly HtmlDocument _document;
        private readonly SeleniumTool _seleniumTool;

        private readonly double _crawlingInterval = 0.35;
        private readonly string _startDate = "0406";
        private readonly string _endDate = "0408";

        private readonly string _targetUrl;
        private readonly string _preferenceStr;

        public CrawlingManager(string url, string browserPreference)
        {
            _targetUrl = url;
            _preferenceStr = browserPreference;
            _blackList = new BlackList();
            _httpClient = new HttpClient();
            _document = new HtmlDocument();
            _seleniumTool = new SeleniumTool();
        }

        public async Task<(int, int)> ProcessAsync()
        {
            await Console.Out.WriteLineAsync("蒐集資料中...\n");

            int totalPage = await GetPageCount();

            if (totalPage < 0)
                return default;

            var urls = GetPageUrls(totalPage, _targetUrl);

            await Console.Out.WriteLineAsync("爬蟲中...\n");

            var (originCount, jobList) = await GetFilteredNodes(urls);
            var outputHtml = GetHtmlString(jobList);

            await Console.Out.WriteLineAsync($"\n原始資料：{originCount}筆，篩選後：{jobList.Count}筆\n");

            var IsFirefox = IsFirefoxOverChrome();
            var outputMsg = IsFirefox ? "火狐" : "Chrome";
            await Console.Out.WriteLineAsync($"開啟{outputMsg}瀏覽器模擬中...\n");

            var xPath = "//*[@id=\"js-job-content\"]";
            _seleniumTool.Process(_targetUrl, xPath, outputHtml, IsFirefox);

            return (originCount, jobList.Count);
        }

        private async Task<int> GetPageCount()
        {
            try
            {
                var firstPage = await _httpClient.GetStringAsync(_targetUrl);
                var regex = new Regex("\"totalPage\":(\\d+)");
                var match = regex.Match(firstPage);

                return int.Parse(match.Groups[1].Value);
            }
            catch (Exception ex)
            {
                if (ex is not (InvalidOperationException or FormatException))
                    throw;

                await Console.Out.WriteLineAsync("URL錯誤！請輸入正確104職缺搜尋連結\n");
                await Console.Out.WriteLineAsync("強制終止程序...\n");
                return -1;
            }
        }

        private async Task<(int, IList<string>)> GetFilteredNodes(string[] urls)
        {
            int originCount = 0;
            var estimatedCount = urls.Length * 15;
            var jobListList = new List<string>(estimatedCount);

            for (int i = 0; i < urls.Length; i++)
            {
                var (row, col) = Console.GetCursorPosition();
                await Console.Out.WriteLineAsync($"每{_crawlingInterval}秒擷取第 {i + 1} / {urls.Length} 頁職缺資訊中");
                Console.SetCursorPosition(row, col);

                var articles = await GetArticles(urls[i]);

                originCount += articles.Count;
                AppendWhileMeetsRequirements(jobListList, articles);

                await Task.Delay(TimeSpan.FromSeconds(_crawlingInterval));
            }

            await Console.Out.WriteLineAsync();
            return (originCount, jobListList);
        }

        private async Task<HtmlNodeCollection> GetArticles(string url)
        {
            var xPath = "//*[@id=\"js-job-content\"]/article";

            var html = await _httpClient.GetStringAsync(url);
            _document.LoadHtml(html);

            var articles = _document.DocumentNode.SelectNodes(xPath)
                ?? throw new NullReferenceException("來源資料找不到artical節點");

            return articles;
        }

        private void AppendWhileMeetsRequirements(List<string> jobList, HtmlNodeCollection articles)
        {
            for (int i = 0; i < articles.Count; i++)
            {
                if (IsBlacklist(articles[i]))
                    continue;

                if (!IsDateInRange(articles[i], _startDate, _endDate))
                    continue;

                jobList.Add(articles[i].OuterHtml);
            }
        }

        private bool IsBlacklist(HtmlNode node)
        {
            var jobTitle = node.GetAttributeValue("data-job-name", "").ToUpper();
            var company = node.GetAttributeValue("data-cust-name", "");
            var industry = node.GetAttributeValue("data-indcat-desc", "");

            if (string.IsNullOrEmpty(jobTitle))
                return true;

            if (_blackList.IndustryName.Contains(industry))
                return true;

            if (_blackList.CompanyNames.Contains(company))
                return true;

            if (_blackList.CompanyNames_SeemsGoodButCurrentlyNo.Contains(company))
                return true;

            if (_blackList.CompanyNames_HadSubmitted.Contains(company))
                return true;

            foreach (var keyword in _blackList.CompanyKeywords)
            {
                if (company.Contains(keyword))
                    return true;
            }

            foreach (var keyword in _blackList.JobTitleKeywords)
            {
                if (jobTitle.Contains(keyword))
                    return true;
            }

            return false;
        }

        private bool IsDateInRange(HtmlNode node, string startDateText, string endDateText)
        {
            var dateNode = ".//span[@class='b-tit__date']";
            var dateText = node.SelectSingleNode(dateNode).InnerText.Trim();

            if (string.IsNullOrEmpty(dateText))
                return false;

            var date = DateOnly.ParseExact(dateText, "M/dd", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var startDate = DateOnly.ParseExact(startDateText, "MMdd", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var endDate = DateOnly.ParseExact(endDateText, "MMdd", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

            if (startDate > endDate)
                return false;

            return date >= startDate && date <= endDate;
        }

        private string GetHtmlString(IList<string> nodes)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nodes.Count; i++)
            {
                sb.Append(nodes[i]);
            }

            return sb.ToString();
        }

        private string[] GetPageUrls(int totalPages, string firstPageUrl)
        {
            var regex = new Regex("&page=(\\d+)");
            var match = regex.Match(firstPageUrl);

            if (!match.Success)
                throw new ArgumentException($"{firstPageUrl}為錯誤格式");

            int pageNumIndex = match.Index;
            int pageNumLength = match.Length;

            var sbFront = new StringBuilder(pageNumIndex);
            var sbEnd = new StringBuilder(firstPageUrl.Length - pageNumIndex);

            for (int i = 0; i < pageNumIndex; i++)
            {
                sbFront.Append(firstPageUrl[i]);
            }

            for (int i = pageNumIndex + pageNumLength; i < firstPageUrl.Length; i++)
            {
                sbEnd.Append(firstPageUrl[i]);
            }

            var urls = new string[totalPages];
            var sbBuffer = new StringBuilder(firstPageUrl.Length + 2);
            for (int i = 1; i <= urls.Length; i++)
            {
                sbBuffer.Append(sbFront);
                sbBuffer.Append("&page=");
                sbBuffer.Append(i);
                sbBuffer.Append(sbEnd);
                urls[i - 1] = sbBuffer.ToString();
                sbBuffer.Clear();
            }

            return urls;
        }

        private bool IsFirefoxOverChrome()
        {
            _ = int.TryParse(_preferenceStr, out int num);

            if (num == 2)
                return false;

            return true;
        }
    }
}