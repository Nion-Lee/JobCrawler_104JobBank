using HtmlAgilityPack;
using System.Globalization;
using System.Text;

namespace JobCrwaler_104
{
    public class CrawlingManager
    {
        private readonly BlackList _blackList;
        private readonly HtmlDocument _document;
        private const string _tempPath = "C:\\Users\\User\\Desktop\\my repo\\JobCrwaler_104\\JobCrwaler_104\\temp";
        private const string _sourcePath = "C:\\Users\\User\\Desktop\\my repo\\JobCrwaler_104\\JobCrwaler_104\\SourceHtml";
        private const string _outputPath = "C:\\Users\\User\\Desktop\\my repo\\JobCrwaler_104\\JobCrwaler_104\\Output.html";

        public CrawlingManager()
        {
            _blackList = new BlackList();
            _document = new HtmlDocument();
        }

        public async Task<(int, int)> ProcessAsync()
        {
            await PreTrimSourceHtml();

            var sourceHtml = await File.ReadAllTextAsync(_sourcePath);
            _document.LoadHtml(sourceHtml);


            var xPathPages = "/html/body/main/div[3]/div/div[2]/div[1]/label[1]/select/option[1]";
            var pageText = _document.DocumentNode.SelectSingleNode(xPathPages).InnerText;
            var totalPage = ExtractTotalPage(pageText);






            var xPathArtical = "//*[@id=\"js-job-content\"]/article";
            var articles = _document.DocumentNode.SelectNodes(xPathArtical) ??
                           throw new NullReferenceException("來源資料找不到artical節點");

            int originCount = articles.Count;
            var filtered = articles.Where(article => !IsBlacklist(article))
                                   //.Where(article => IsSpecifiedDate(article, "3/31"))
                                   .Where(article => IsDateInRange(article, "0403", "0405"))
                                   .ToList();

            var outputHtml = GetHtmlString(filtered);
            await File.WriteAllTextAsync(_outputPath, outputHtml, Encoding.UTF8);

            return (originCount, filtered.Count);
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

        private bool IsSpecifiedDate(HtmlNode node, string date)
        {
            var xPathDateNode = ".//span[@class='b-tit__date']";
            var text = node.SelectSingleNode(xPathDateNode).InnerText.Trim();

            if (string.IsNullOrEmpty(text))
                return true;

            return text.Equals(date);
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

        private string GetHtmlString(IList<HtmlNode> nodes)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nodes.Count; i++)
            {
                sb.Append(nodes[i].OuterHtml);
            }
            return sb.ToString();
        }

        private async Task PreTrimSourceHtml()
        {
            var oldStr = "data-job-name=\"\"";
            var newStr = "data-job-name=\"";

            using (var fsSource = new FileStream(_sourcePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fsSource))
            using (var FsTemp = new FileStream(_tempPath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(FsTemp))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var replacedLine = line.Replace(oldStr, newStr);
                    await writer.WriteLineAsync(replacedLine);
                }
            }

            File.Delete(_sourcePath);
            File.Move(_tempPath, _sourcePath);
        }

        public int ExtractTotalPage(string text)
        {
            int targetIndex = 0;
            while (targetIndex < text.Length)
            {
                if (text[targetIndex++] == '/')
                    break;
            }

            targetIndex += 1;
            
            int total = 0;
            while (text[targetIndex] != ' ')
            {
                total = total * 10 + text[targetIndex] - '0';
                targetIndex++;
            }

            return total;
        }
    }
}



/*
 
 https://www.104.com.tw/jobs/search/    ?ro=0&jobcat=2007001000&isnew=7&       keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=14&asc=0&excludeIndustryCat=1001001001&page=1&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1
 https://www.104.com.tw/jobs/search/    ?ro=1&jobcat=2007001000&isnew=7&       keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=14&asc=0&excludeIndustryCat=1001001001&page=1&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1
 首頁
 https://www.104.com.tw/jobs/search/list?ro=1&jobcat=2007001000&isnew=7&kwop=7&keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=14&asc=0&excludeIndustryCat=1001001001&page=2&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1
 滑1
 https://www.104.com.tw/jobs/search/list?ro=1&jobcat=2007001000&isnew=7&kwop=7&keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=14&asc=0&excludeIndustryCat=1001001001&page=3&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1
 滑2
 https://www.104.com.tw/jobs/search/list?ro=1&jobcat=2007001000&isnew=7&kwop=7&keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=14&asc=0&excludeIndustryCat=1001001001&page=4&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1
 滑3


 https://www.104.com.tw/jobs/search/list?ro=1&jobcat=2007001000&isnew=7&kwop=7&keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=14&asc=0&excludeIndustryCat=1001001001&page=16&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1
 手動載入第16頁
 */