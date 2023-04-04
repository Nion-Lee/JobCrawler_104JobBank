using HtmlAgilityPack;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace JobCrwaler_104
{
    public class CrawlingManager
    {
        private readonly BlackList _blackList;
        private readonly HtmlDocument _document;
        private const string _sourcePath = "C:\\Users\\User\\Desktop\\my repo\\JobCrwaler_104\\JobCrwaler_104\\SourceHtml";
        private const string _outputPath = "C:\\Users\\User\\Desktop\\my repo\\JobCrwaler_104\\JobCrwaler_104\\Output.html";

        public CrawlingManager()
        {
            _blackList = new BlackList();
            _document = new HtmlDocument();
        }

        public async Task<(int, int)> ProcessAsync()
        {
            //await PreTrimSourceHtml();


            var sourceHtml = await File.ReadAllTextAsync(_sourcePath);
            _document.LoadHtml(sourceHtml);

            var xPathArtical = "//*[@id=\"js-job-content\"]/article";
            var articles = _document.DocumentNode.SelectNodes(xPathArtical) ??
                           throw new NullReferenceException("來源資料找不到artical節點");

            int originCount = articles.Count;
            var filtered = articles.Where(article => !IsBlacklist(article))
                                   .Where(article => IsSpecifiedDate(article, "3/31"))
                                   .ToList();

            var outputHtml = GetHtmlString(filtered);
            await File.WriteAllTextAsync(_outputPath, outputHtml, Encoding.UTF8);

            return (originCount, filtered.Count);
        }

        [Obsolete("尚未施工完成，暫無法使用")]
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
            var company = node.GetAttributeValue("data-cust-name", null);
            var industry = node.GetAttributeValue("data-indcat-desc", null);

            if (string.IsNullOrEmpty(jobTitle))
            {
                var wrongText = node.OuterHtml;
                var fixedText = wrongText.Replace("data-job-name=\"\"", "data-job-name=\"");

                _document.LoadHtml(fixedText);
                var newNode = _document.DocumentNode;

            }


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
            using var fs = new FileStream(_sourcePath, FileMode.Open, FileAccess.ReadWrite);
            using var reader = new StreamReader(fs);
            using var writer = new StreamWriter(fs);
            
           

            string? line;
            var position = 0L;
            var sb = new StringBuilder();
            var context = "data-job-name=\"\"";


            var AAA = fs.Position;
            var BBB = fs.Position;
            var RRRR1 = 0L;
            var RRRR2 = 0L;
            var RRRR3 = 0L;
            var RRRR4 = 0L;

            var totalLineCount1 = 0;
            var totalLineCount2 = 0;
            var totalLineCount3 = 0;
            var totalLineCount4 = 0;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                int index = line.IndexOf(context);

                RRRR1 = Encoding.UTF8.GetByteCount(line);
                RRRR2 = Encoding.ASCII.GetByteCount(line);
                RRRR3 = Encoding.UTF32.GetByteCount(line);
                RRRR4 = Encoding.Unicode.GetByteCount(line);

                totalLineCount1 += (int)RRRR1;
                totalLineCount2 += (int)RRRR2;
                totalLineCount3 += (int)RRRR3;
                totalLineCount4 += (int)RRRR4;


                BBB = fs.Position;


                // 覆蓋的行數錯誤，沒蓋在該蓋的地方
                // 還是差一點點

                // 後來我想想，好像可以捨棄掉這麼複雜的方法
                // 直接用replace的方法執行


                line.Replace(context, sb.ToString());

                if (index != -1)
                {
                    sb.Append(line);
                    sb.Remove(index + 15, 1);
                    sb.Insert(index + 15, ' ');
                    line = sb.ToString();

                    var count = Encoding.UTF8.GetByteCount(line);
                    fs.Seek(position - count, SeekOrigin.Begin);


                    await fs.WriteAsync(Encoding.UTF8.GetBytes(line), 0, count);
                    await fs.FlushAsync();
                    
                    //await writer.WriteLineAsync(line);
                    
                    //await writer.FlushAsync();

                    //fs.Seek(position + count, SeekOrigin.Begin);


                    sb.Clear();
                }

                position = fs.Position;

                //11760900
                //+15
            }
        }

        private async Task PreTrimSourceHtmlTemp()
        {

        }
    }
}
