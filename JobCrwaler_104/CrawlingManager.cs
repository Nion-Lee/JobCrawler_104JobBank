using HtmlAgilityPack;
using System.Text;

namespace JobCrwaler_104
{
    public class CrawlingManager
    {
        private readonly BlackList _blackList;
        private const string _sourcePath = "C:\\Users\\User\\Desktop\\my repo\\JobCrwaler_104\\JobCrwaler_104\\SourceHtml";
        private const string _outputPath = "C:\\Users\\User\\Desktop\\my repo\\JobCrwaler_104\\JobCrwaler_104\\Output.html";

        public CrawlingManager()
        {
            _blackList = new BlackList();
        }

        public async Task<int> ProcessAsync()
        {
            var document = new HtmlDocument();
            var filtered = new List<HtmlNode>();
            var sourceHtml = await File.ReadAllTextAsync(_sourcePath);

            document.LoadHtml(sourceHtml);

            var xPath = "//*[@id=\"js-job-content\"]/article";
            var articles = document.DocumentNode.SelectNodes(xPath);

            foreach (var article in articles)
            {
                if (!IsBlacklist(article))
                {
                    filtered.Add(article);
                }
            }

            var outputHtml = GetHtmlString(filtered);
            await File.WriteAllTextAsync(_outputPath, outputHtml, Encoding.UTF8);

            return filtered.Count;
        }

        private JobInfo DesrializeHtmlNodes(HtmlNode node)
        {
            return new JobInfo
            {
                JobTitle = node.GetAttributeValue("data-job-name", null),
                CompanyName = node.GetAttributeValue("data-cust-name", null),
                //Description = node.GetAttributeValue("data-cust-name", null),
                //WebsiteLink = node.GetAttributeValue("data-cust-name", null)
            };
        }

        private bool IsBlacklist(HtmlNode node)
        {
            var jobTitle = node.GetAttributeValue("data-job-name", null).ToUpper();
            var companyName = node.GetAttributeValue("data-cust-name", null);

            if (_blackList.CompanyNames.Contains(companyName))
                return true;

            if (_blackList.CompanyNames_SeemsGoodButCurrentlyNo.Contains(companyName))
                return true; 
            
            if (_blackList.CompanyNames_HadSubmitted.Contains(companyName))
                return true;

            foreach (var keyword in _blackList.CompanyKeywords)
            {
                if (companyName.Contains(keyword))
                    return true;
            }

            foreach (var keyword in _blackList.JobTitleKeywords)
            {
                if (jobTitle.Contains(keyword))
                    return true;
            }

            return false;
        }

        private string GetHtmlString(List<HtmlNode> nodes)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nodes.Count; i++)
            {
                sb.Append(nodes[i].OuterHtml);
            }

            return sb.ToString();
        }
    }
}
