using System;

namespace Katerini.Database;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlDatabase")!;
            var dbUpgrader = new DatabaseUpgrader(connectionString);
            var success = dbUpgrader.UpgradeSchema();

            return success switch
            {
                true => 0,
                false => -1
            };
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.ResetColor();
            return -2;
        }
    }
}
