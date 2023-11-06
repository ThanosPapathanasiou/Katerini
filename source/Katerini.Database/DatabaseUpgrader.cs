using System;
using DbUp;
using System.Reflection;

namespace Katerini.Database;

public class DatabaseUpgrader
{
    private readonly string _connectionString;

    public DatabaseUpgrader(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool UpgradeSchema()
    {
        EnsureDatabase.For.SqlDatabase(_connectionString);

        var upgrader =
            DeployChanges.To
                .SqlDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
#if DEBUG
            Console.ReadLine();
#endif 
            return false;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();

        return true;
    }
}