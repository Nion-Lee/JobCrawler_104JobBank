using JobCrwaler_104;

var manager = new CrawlingManager();
var quantity = await manager.ProcessAsync();

Console.WriteLine($"共{quantity}筆篩選後資料");


//using System.Text;

//var pathSource = @"c:\tests\source.txt";
//var pathNew = @"c:\tests\newfile.txt";


//using (var source = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite))
//{
//    var bytes = new byte[source.Length];
//    int numBytesToRead = (int)source.Length;
//    int numBytesRead = 0;

//    while (numBytesToRead > 0)
//    {
//        // Read may return anything from 0 to numBytesToRead.
//        int n = await source.ReadAsync(bytes, numBytesRead, numBytesToRead);

//        var str = Encoding.UTF8.GetString(bytes);

//        await Console.Out.WriteAsync(str);

//        var replacement = "999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999";
//        int n1 = await source.ReadAsync(bytes, numBytesRead, numBytesToRead);

//        for (int i = 0; i < bytes.Length; i++)
//        {
//            await Console.Out.WriteAsync("");
//        }

//        // Break when the end of the file is reached.
//        if (n == 0)
//            break;

//        numBytesRead += n;
//        numBytesToRead -= n;
//    }

//    numBytesToRead = bytes.Length;

//    // Write the byte array to the other FileStream.
//    using (var fsNew = new FileStream(pathNew, FileMode.Create, FileAccess.Write))
//    {
//        fsNew.Write(bytes, 0, numBytesToRead);
//    }
//}
