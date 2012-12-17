using System;
using System.Collections.Generic;
using System.IO;
using GoalUploader.Properties;
using Hp.Merlin.HedgeSense;
using Hp.Merlin.Services;

[assembly:log4net.Config.XmlConfigurator]

namespace GoalUploader
{
    static class GoalUploaderProgram
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static int Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                    ShowUsage();

                switch (args[0])
                {
                    case "-s":
                        {
                            if (args.Length < 2 || args.Length > 3)
                                ShowUsage();

                            string strategy = args.Length > 2 ? args[1] : null;
                            string path = args.Length > 2 ? args[2] : args[1];
                            SendGoals(strategy, path);
                        }
                        break;
                    case "-r":
                        {
                            if (args.Length < 2 || args.Length > 3)
                                ShowUsage();

                            string strategy = args[1];
                            string path = args.Length > 2 ? args[2] : null;
                            RequestGoals(strategy, path);
                        }
                        break;
                    case "-o":
                    case "-t":
                    case "-p":
                        {
                            if (args.Length < 2 || args.Length > 3)
                                ShowUsage();

                            string strategy = args[1];
                            DateTime start = args.Length == 3 ? DateTime.Parse(args[2]) : default(DateTime);

                            switch (args[0][1])
                            {
                                case 'o':
                                    RequestOrderHistory(strategy, start);
                                    break;
                                case 't':
                                    RequestTransactionHistory(strategy, start);
                                    break;
                                case 'p':
                                    RequestPnl(strategy, start);
                                    break;
                            }
                        }
                        break;
                    default:
                        ShowUsage();
                        break;
                }
            }
            catch (Exception x)
            {
                Log.Error("Operation failed", x);
                return 1;
            }
            return 0;
        }

        private static void SendGoals(string strategy, string path)
        {
            Log.DebugFormat("Parsing {0}...", path);
            string strategyFromFile;
            var goals = StrategyGoal.LoadGoals(path, true, out strategyFromFile);

            if (string.IsNullOrEmpty(strategy))
            {
                if (string.IsNullOrEmpty(strategyFromFile))
                    throw new Exception("Specify strategy name either in first column or via command line argument");
                strategy = strategyFromFile;
            }
            else if (!string.IsNullOrEmpty(strategyFromFile) && strategy != strategyFromFile)
            {
                Log.DebugFormat(
                    "Warning: strategy name from file ('{0}') mismatches command line argument ('{1}'): using '{2}'.",
                    strategyFromFile, strategy, strategy);
            }

            DumpGoals(strategy, goals);

            Log.Debug("Sending goals...");
            RemoteCall(
                strategy, p =>
                    {
                        p.SetGoals(goals);
                        return true;
                    });
            Log.Debug("Done.");
        }

        private static void RequestGoals(string strategy, string path)
        {
            Log.DebugFormat("Requesting goals...");
            var goals = RemoteCall(strategy, p => p.GetCurrentGoals());
            Log.DebugFormat("Done.");
            DumpGoals(strategy, goals);

            Log.DebugFormat("Saving list into {0}...", path);
            using (var output = path != null ? new StreamWriter(path) : Console.Out)
                StrategyGoal.SaveGoals(strategy, true, goals, output);
        }

        private static void DumpGoals(string strategyName, List<StrategyGoal> goals)
        {
            Log.DebugFormat("Got {0} goals for strategy {1}:", goals.Count, strategyName);
            foreach (var goal in (IEnumerable<StrategyGoal>) goals)
                Log.DebugFormat("\t{0}", goal);
        }

        private static void RequestOrderHistory(string strategy, DateTime start)
        {
            var list = RemoteCall(strategy, p => p.GetOrderHistory(start));
            foreach (var i in list)
                Console.WriteLine(i);
        }

        private static void RequestTransactionHistory(string strategy, DateTime start)
        {
            var list = RemoteCall(strategy, p => p.GetTransactionHistory(start));
            foreach (var i in list)
                Console.WriteLine(i);
        }

        private static void RequestPnl(string strategy, DateTime start)
        {
            var list = RemoteCall(strategy, p => p.GetPnl(start));
            Console.WriteLine(
                string.Join(
                    ",", new object[]
                        {
                            "Strategy",
                            "Symbol",
                            "LastClosePosition",
                            "LastClosePrice",
                            "LastCloseMarketValue",
                            "TransactionCount",
                            "BoughtAmount",
                            "BoughtAvgPrice",
                            "SoldAmount",
                            "SoldAvgPrice",
                            "ManualAdjustments",
                            "OtherTransactions",
                            "AvgPriceSinceOpen",
                            "CurrentPosition",
                            "CurrentPrice",
                            "CurrentMarketValue",
                            "RealizedPnl",
                            "UnrealizedPnl",
                            "TotalPnl",
                        }));
            foreach (var i in list)
                Console.WriteLine(
                    string.Join(
                        ",", new object[]
                            {
                                i.Strategy,
                                i.Symbol,
                                i.LastClosePosition,
                                i.LastClosePrice,
                                i.LastCloseMarketValue,
                                i.TransactionCount,
                                i.BoughtAmount,
                                i.BoughtAvgPrice,
                                i.SoldAmount,
                                i.SoldAvgPrice,
                                i.ManualAdjustments,
                                i.OtherTransactions,
                                i.AvgPriceSinceOpen,
                                i.CurrentPosition,
                                i.CurrentPrice,
                                i.CurrentMarketValue,
                                i.RealizedPnl,
                                i.UnrealizedPnl,
                                i.TotalPnl,
                            }));
        }

        private static void ShowUsage()
        {
            throw new Exception(@"
Usage:

  GoalUploader -s [strategy_name] goals.tsv
    -- Sends goals contained in goals.tsv for strategy_name. If name is omitted, 
       file's first column is used.

  GoalUploader -r strategy_name [file]
    -- Displays or saves current goals for strategy_name.

  GoalUploader -o strategy_name [start-date]
    -- Displays order history.

  GoalUploader -t strategy_name [start-date]
    -- Displays transaction history.

  GoalUploader -p strategy_name [start-date]
    -- Displays current PNL.
");
        }

        private static T RemoteCall<T>(string strategyName, Func<IGoalsStrategy, T> operation)
        {
            var proxy = WebConstants.GetServiceBusProxy<IGoalsStrategy>(
                Settings.Default.MerlinBusDomain, ProcessType.Strategy, strategyName,
                Settings.Default.MerlinBusUserName, Settings.Default.MerlinBusSharedSecret);

// ReSharper disable SuspiciousTypeConversion.Global
            using ((IDisposable) proxy)
// ReSharper restore SuspiciousTypeConversion.Global
            {
                return operation(proxy);
            }
        }
    }
}
