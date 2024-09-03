using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace JobCrwaler_104
{
    public class SeleniumTool
    {
        public async Task Process(string url, string xPath, string outputHtml, bool isFirefoxOverChrome)
        {
            try
            {
                using WebDriver driver = isFirefoxOverChrome ? new FirefoxDriver()
                                                             : new ChromeDriver();
                driver.Navigate().GoToUrl(url);
                var node = driver.FindElement(By.XPath(xPath));

                var script = "const parentNode = arguments[0].parentNode;" +
                    "arguments[0].remove();" +
                    "const newDiv = document.createElement('div');" +
                    "parentNode.appendChild(newDiv);" +
                    "newDiv.setAttribute('id', 'modified-js-job-content');" +
                    "newDiv.innerHTML = arguments[1];";

                driver.ExecuteScript(script, node, outputHtml);

                await Console.Out.WriteLineAsync("\n請按任一鍵結束瀏覽器模擬\n");
                await Console.In.ReadLineAsync();

                driver.Quit();
            }
            catch (WebDriverException ex)
            {
                await Console.Out.WriteLineAsync($"\n程式已被強制終止");
                await Console.Out.WriteLineAsync($"{ex.Message}");
            }
        }
    }
}
