using System;

namespace Katerini.Database;

public static class Program
{
    const int SUCCESS_CODE = 0;
    const int FAILURE_TO_UPDATE_CODE = 1;
    const int ERROR_CODE = 2;
    
    // TODO: connect with serilog / seq and output the logs there.
    
    public static int Main(string[] args)
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlDatabase")!;
            var dbUpgrader = new DatabaseUpgrader(connectionString);
            var success = dbUpgrader.UpgradeSchema();

            return success switch
            {
                true => SUCCESS_CODE,
                false => FAILURE_TO_UPDATE_CODE
            };
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.ResetColor();
            return ERROR_CODE;
        }
    }
}
