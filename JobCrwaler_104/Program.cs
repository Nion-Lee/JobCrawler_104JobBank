using JobCrwaler_104;

await Console.Out.WriteLineAsync("請輸入正確目標104網址：");

var url = Console.ReadLine();


url = "https://www.104.com.tw/jobs/search/?ro=1&jobcat=2007001000&isnew=3&kwop=7&keyword=C%23%20.NET&expansionType=area%2Cspec%2Ccom%2Cjob%2Cwf%2Cwktm&area=6001002003%2C6001001001%2C6001001002%2C6001001003%2C6001001004%2C6001001005%2C6001001006%2C6001001007%2C6001001011%2C6001001008&order=15&asc=0&excludeIndustryCat=1001001001&page=3&mode=s&jobsource=2018indexpoc&langFlag=0&langStatus=0&recommendJob=1&hotJob=1";

await Console.Out.WriteLineAsync();

var manager = new CrawlingManager(url);
var quantity = await manager.ProcessAsync();

await Console.Out.WriteLineAsync($"程序結束");