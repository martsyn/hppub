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

                            string stratName = args.Length > 2 ? args[1] : null;
                            string path = args.Length > 2 ? args[2] : args[1];
                            SendGoals(stratName, path);
                        }
                        break;
                    case "-r":
                        {
                            if (args.Length < 2 || args.Length > 3)
                                ShowUsage();

                            string stratName = args[1];
                            string path = args.Length > 2 ? args[2] : null;
                            RequestGoals(stratName, path);
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

        private static void SendGoals(string stratName, string path)
        {
            Log.DebugFormat("Parsing {0}...", path);
            string fileStratName;
            var goals = StrategyGoal.LoadGoals(path, true, out fileStratName);

            if (string.IsNullOrEmpty(stratName))
            {
                if (string.IsNullOrEmpty(fileStratName))
                    throw new Exception("Specify strategy name either in first column or via command line argument");
                stratName = fileStratName;
            }
            else if (!string.IsNullOrEmpty(fileStratName) && stratName != fileStratName)
            {
                Log.DebugFormat(
                    "Warning: strategy name from file ('{0}') mismatches command line argument ('{1}'): using '{2}'.",
                    fileStratName, stratName, stratName);
            }

            DumpGoals(stratName, goals);

            Log.Debug("Sending goals...");
            RemoteCall(
                stratName, p =>
                    {
                        p.SetGoals(goals);
                        return true;
                    });
            Log.Debug("Done.");
        }

        private static void RequestGoals(string stratName, string path)
        {
            Log.DebugFormat("Requesting goals...");
            var goals = RemoteCall(stratName, p => p.GetCurrentGoals());
            Log.DebugFormat("Done.");
            DumpGoals(stratName, goals);

            Log.DebugFormat("Saving list into {0}...", path);
            using (var output = path != null ? new StreamWriter(path) : Console.Out)
                StrategyGoal.SaveGoals(stratName, true, goals, output);
        }

        private static void DumpGoals(string strategyName, List<StrategyGoal> goals)
        {
            Log.DebugFormat("Got {0} goals for strategy {1}:", goals.Count, strategyName);
            foreach (var goal in (IEnumerable<StrategyGoal>) goals)
                Log.DebugFormat("\t{0}", goal);
        }

        private static void ShowUsage()
        {
            throw new Exception(@"
Usage examples:

  GoalUploader -s strategy_name goals.tsv
    -- sends goals contained in goals.tsv for strategy_name

  GoalUploader -s goals.tsv
    -- sends goals contained in goals.tsv for strategy_name read from file's first column

  GoalUploader -r strategy_name goals.tsv
    -- requests and saves current goals for strategy_name in goals.tsv:

  GoalUploader -r strategy_name
    -- requests and displays current goals for strategy_name
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
