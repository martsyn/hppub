using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using GoalUploader.Properties;
using Hp.Merlin.HedgeSense;
using Hp.Merlin.Services;

namespace GoalUploader
{
    static class GoalUploaderProgram
    {
        private static readonly TextWriter Log = Console.Error;

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
            catch (FaultException x)
            {
                Log.WriteLine("Remote operation failed: {0}", x.Reason);
                return 2;
            }
            catch (Exception x)
            {
                Log.WriteLine("{0}: {1}", x.GetType().Name, x.Message);
                return 1;
            }
            return 0;
        }

        private static void SendGoals(string stratName, string path)
        {
            Log.WriteLine("Parsing {0}...", path);
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
                Log.WriteLine(
                    "Warning: strategy name from file ('{0}') mismatches command line argument ('{1}'): using '{2}'.",
                    fileStratName, stratName, stratName);
            }

            DumpGoals(stratName, goals);

            Log.WriteLine("Sending goals...");
            RemoteCall(
                stratName, p =>
                    {
                        p.SetGoals(goals);
                        return true;
                    });
            Log.WriteLine("Done.");
        }

        private static void RequestGoals(string stratName, string path)
        {
            Log.WriteLine("Requesting goals...");
            var goals = RemoteCall(stratName, p => p.GetCurrentGoals());
            Log.WriteLine("Done.");
            DumpGoals(stratName, goals);

            Log.WriteLine("Saving list into {0}...", path);
            using (var output = path != null ? new StreamWriter(path) : Console.Out)
                StrategyGoal.SaveGoals(stratName, true, goals, output);
        }

        private static void DumpGoals(string strategyName, List<StrategyGoal> goals)
        {
            Log.WriteLine("Got {0} goals for strategy {1}:", goals.Count, strategyName);
            foreach (var goal in (IEnumerable<StrategyGoal>) goals)
                Log.WriteLine("\t{0}", goal);
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
