using JobCrwaler_104;

await Console.Out.WriteAsync("請輸入目標104網址：");
var url = Console.ReadLine();

await Console.Out.WriteLineAsync();
await Console.Out.WriteAsync("請選擇使用火狐或Chrome，輸入數字1或2：");
var browserPreference = Console.ReadLine();
await Console.Out.WriteLineAsync();

var manager = new CrawlingManager(url, browserPreference);
var quantity = await manager.ProcessAsync();

await Console.Out.WriteLineAsync($"程序結束");