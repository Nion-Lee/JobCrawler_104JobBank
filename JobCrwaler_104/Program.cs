using JobCrwaler_104;

var manager = new CrawlingManager();
var quantity = await manager.ProcessAsync();

Console.WriteLine($"共{quantity}筆篩選後資料");