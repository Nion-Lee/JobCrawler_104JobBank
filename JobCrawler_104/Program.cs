using HtmlAgilityPack;
using JobCrawler_104;
using JobCrwaler_104;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(context.Configuration.GetRequiredSection("ExclusionList").Get<ExclusionList>()!);
        services.AddSingleton(context.Configuration.GetRequiredSection("CrawlingSettings").Get<CrawlingSettings>()!);
        services.AddHttpClient();
        services.AddScoped<HtmlDocument>();
        services.AddSingleton<SeleniumTool>();
        services.AddSingleton<CrawlingManager>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .UseConsoleLifetime();

var host = builder.Build();

using var serviceScope = host.Services.CreateScope();
var services = serviceScope.ServiceProvider;

var crawlingManager = services.GetRequiredService<CrawlingManager>();

await Console.Out.WriteAsync("請輸入目標104網址：");
var url = await Console.In.ReadLineAsync();

await Console.Out.WriteLineAsync();
await Console.Out.WriteAsync("請選擇使用火狐或Chrome，輸入數字1或2：");
var browserPreference = await Console.In.ReadLineAsync();
await Console.Out.WriteLineAsync();

var quantity = await crawlingManager.ProcessAsync(url, browserPreference);

await Console.Out.WriteLineAsync($"\n程序結束");
