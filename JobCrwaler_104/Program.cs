using JobCrwaler_104;

var manager = new CrawlingManager();
var quantity = await manager.ProcessAsync();

Console.WriteLine($"共{quantity}筆篩選後資料");



//using OpenQA.Selenium;
//using OpenQA.Selenium.Firefox;

//var driverService = FirefoxDriverService.CreateDefaultService();
//using var driver = new FirefoxDriver(driverService);


//// 訪問指定網頁
//var url = "https://www.104.com.tw/jobs/search/?ro=1&jobcat=2007001000&isnew=3&kwop=7&keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=14&asc=0&excludeIndustryCat=1001001001&page=10&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1"; // 更換為您想要操作的網頁URL
//driver.Navigate().GoToUrl(url);

////// 刪除指定節點的所有子節點
//var parentNode = driver.FindElement(By.XPath("//*[@id=\"js-job-content\"]"));
//driver.ExecuteScript("while (arguments[0].firstChild) { arguments[0].removeChild(arguments[0].firstChild); }", parentNode);

////// 添加新內容到指定節點
//var newContent = "<p>New content added!</p>";
//driver.ExecuteScript("arguments[0].innerHTML = arguments[1];", parentNode, newContent);

//// 注意：根據需要，請在這裡添加其他操作，例如等待用戶輸入、網頁截圖等。

//Console.ReadLine();
//// 關閉瀏覽器
//driver.Quit();