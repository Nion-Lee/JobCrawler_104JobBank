
await Console.Out.WriteAsync("請輸入八皇后問題之個數：");

var input = string.Empty;
input = await Console.In.ReadLineAsync();

await Console.Out.WriteLineAsync();
var isValid = byte.TryParse(input, out var num);

if (!isValid)
{
    throw new ArgumentException("輸入數值錯誤！");
}

var solutions = new NQueensProblem().SolveNQueens(num);
int count = 1;
foreach (var item in solutions)
{
    await Console.Out.WriteLineAsync($"Solution {count}:");
    foreach (string row in item)
    {
        await Console.Out.WriteLineAsync(row);
    }
    await Console.Out.WriteLineAsync();
    count++;
}

await Console.Out.WriteLineAsync("\n已完成所有組合之呈現。");


public class NQueensProblem
{
    public List<List<string>> SolveNQueens(int n)
    {
        var results = new List<List<string>>();
        Solve(0, new int[n], new bool[n], new bool[2 * n], new bool[2 * n], new List<string>(), results, n);

        return results;
    }

    private void Solve(int row, int[] queens, bool[] cols, bool[] diag1, bool[] diag2, List<string> board, List<List<string>> results, int n)
    {
        if (row == n)
        {
            results.Add(new List<string>(board));
            return;
        }

        for (int col = 0; col < n; col++)
        {
            int d1 = col - row + n;
            int d2 = col + row;
            if (!cols[col] && !diag1[d1] && !diag2[d2])
            {
                queens[row] = col;
                cols[col] = diag1[d1] = diag2[d2] = true;
                var rowArray = new string('.', n).ToCharArray();
                rowArray[col] = 'Q';
                board.Add(new string(rowArray));

                Solve(row + 1, queens, cols, diag1, diag2, board, results, n);

                board.RemoveAt(board.Count - 1);
                cols[col] = diag1[d1] = diag2[d2] = false;
            }
        }
    }
}

