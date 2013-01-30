using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using GoalUploader.Properties;
using Hp.Merlin.HedgeSense;
using Hp.Merlin.Services;
using System.Linq;

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
                    ShowUsage("Command expected");

                string command = args[0];
                string strategy = null;
                string outFile = null;
                string inFile = null;
                DateTime start = default(DateTime);
                string[] symbols = null;
                StrategyGoalVersion version = StrategyGoalVersion.Original;

                for (int i = 1; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-s":
                            if (++i < args.Length)
                                strategy = args[i];
                            else
                                ShowUsage("Strategy expected");
                            break;
                        case "-i":
                            if (++i < args.Length)
                                inFile = args[i];
                            else
                                ShowUsage("Input filename expected");
                            break;
                        case "-o":
                            if (++i < args.Length)
                                outFile = args[i];
                            else
                                ShowUsage("Output filename expected");
                            break;
                        case "-t":
                            if (++i < args.Length)
                                start = DateTime.ParseExact(args[i], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                            else
                                ShowUsage("Start time expected");
                            break;
                        case "-n":
                            if (++i < args.Length)
                                symbols = args[i].Split(',', '|', ';');
                            else
                                ShowUsage("Start time expected");
                            break;
                        case "-v":
                            if (++i < args.Length)
                                version = (StrategyGoalVersion) Enum.Parse(typeof (StrategyGoalVersion), args[i]);
                            else
                                ShowUsage("Version expected");
                            break;
                        default:
                            ShowUsage("Unknown option " + args[i]);
                            break;
                    }
                }

                if (strategy == null)
                    ShowUsage("Strategy not specified");

                TextWriter output = outFile != null ? new StreamWriter(outFile) : Console.Out;

                try
                {
                    switch (command)
                    {
                        case "set":
                            if (inFile == null)
                                ShowUsage("Input file not specified");
                            SendGoals(strategy, inFile);
                            break;
                        case "close":
                            Close(strategy, symbols);
                            break;
                        case "get":
                            RequestGoals(strategy, output, version);
                            break;
                        case "orders":
                            RequestOrderHistory(strategy, start, output);
                            break;
                        case "transactions":
                            RequestTransactionHistory(strategy, start, output);
                            break;
                        case "pnl":
                            RequestPnl(strategy, start, output);
                            break;
                        case "eodpnl":
                            RequestEodPnl(strategy, start, output);
                            break;
                        default:
                            ShowUsage("Unknown command: " + command);
                            break;
                    }
                }
                finally
                {
                    if (outFile != null)
                        output.Close();
                }
            }
            catch (Exception x)
            {
                Log.Error(x.Message);
                if (x is CommunicationObjectFaultedException)
                    return 2;
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

        private static void Close(string strategy, string[] instruments)
        {
            Log.DebugFormat(
                "Closing {0}: {1}", strategy,
                instruments != null
                    ? string.Join(", ", instruments)
                    : "all targets");
            RemoteCall(
                strategy, p =>
                    {
                        p.Close(instruments != null ? instruments.Select(SecurityMaster.FromHSSymbol).ToList() : null);
                        return true;
                    });
            Log.Debug("Done.");
        }

        private static void RequestGoals(string strategy, TextWriter output, StrategyGoalVersion version)
        {
            Log.DebugFormat("Requesting goals...");
            var goals = RemoteCall(strategy, p => p.GetCurrentGoals());
            Log.DebugFormat("Done.");
            DumpGoals(strategy, goals);
            StrategyGoal.SaveGoals(strategy, true, goals, output, version);
        }

        private static void DumpGoals(string strategyName, List<StrategyGoal> goals)
        {
            Log.DebugFormat("Got {0} goals for strategy {1}:", goals.Count, strategyName);
            foreach (var goal in (IEnumerable<StrategyGoal>) goals)
                Log.DebugFormat("\t{0}", goal);
        }

        private static void RequestOrderHistory(string strategy, DateTime start, TextWriter output)
        {
            var list = RemoteCall(strategy, p => p.GetOrderHistory(start));

            Log.DebugFormat("Got {0} entries:", list.Count);
            foreach (var i in list)
                Log.DebugFormat("\t{0}", i.ToHSString());

            output.WriteLine(SecurityMaster.OrderCsvHeader);
            foreach (var i in list)
                output.WriteLine(i.ToCsvString());
        }

        private static void RequestTransactionHistory(string strategy, DateTime start, TextWriter output)
        {
            var list = RemoteCall(strategy, p => p.GetTransactionHistory(start));

            Log.DebugFormat("Got {0} entries:", list.Count);
            foreach (var i in list)
                Log.DebugFormat("\t{0}", i.ToHSString());

            output.WriteLine(SecurityMaster.TransactionCsvHeader);
            foreach (var i in list)
                output.WriteLine(i.ToCsvString());
        }

        private static void RequestPnl(string strategy, DateTime start, TextWriter output)
        {
            var list = RemoteCall(strategy, p => p.GetPnl(start));

            Log.DebugFormat("Got {0} entries:", list.Count);
            foreach (var i in list)
                Log.DebugFormat("\t{0}", i.ToHSString());
            
            output.WriteLine(SecurityMaster.DetailedPnlEntryCsvHeader);
            foreach (var i in list)
                output.WriteLine(i.ToCsvString());
        }

        private static void RequestEodPnl(string strategy, DateTime start, TextWriter output)
        {
            var list = RemoteCall(strategy, p => p.GetEodPnl(start));

            Log.DebugFormat("Got {0} entries:", list.Count);
            foreach (var i in list)
                Log.DebugFormat("\t{0}", i.ToHSString());
            
            output.WriteLine(SecurityMaster.DetailedPnlEntryCsvHeader);
            foreach (var i in list)
                output.WriteLine(i.ToCsvString());
        }

        private static void ShowUsage(string error)
        {
            const string usage = @"
Usage:

    GoalUploader.exe command [options]

Commands:

    set           - Sets goals from input file.
    close         - Resets the target of specified instrument (or all targets,
                    if instrument is omitted) to 0 and closes the position
                    immediately.
    get           - Retrieves current goals.
    orders        - Retrieves order history.
    transactions  - Retrieves transaction history.
    pnl           - Retrieves PNL report.
    eodpnl        - Retrieves end-of-day PNL history.

Options:

    -s strategy   - Strategy name.
    -i filename   - Input filename.
    -o filename   - Output filename (stdout, if unspecified).
    -t YYYY/MM/DD - Start date for historical and PNL commands.
    -n symbol     - Instrument ticker.

Examples:

    GoalUploader.exe set -s STRATEGY -i goals.csv
    GoalUploader.exe get -s STRATEGY
    GoalUploader.exe pnl -s STRATEGY -t 2013/01/01 -o STRATEGY.pnl.csv";

            throw new Exception(error != null ? error + usage : usage);
        }

        private static T RemoteCall<T>(string strategy, Func<IGoalsStrategy, T> operation)
        {
            for (int attempt = 1;; ++attempt)
            {
                try
                {
                    var proxy = WebConstants.GetServiceBusProxy<IGoalsStrategy>(
                        Settings.Default.MerlinBusDomain, ProcessType.Strategy, strategy,
                        Settings.Default.MerlinBusUserName, Settings.Default.MerlinBusSharedSecret);
// ReSharper disable SuspiciousTypeConversion.Global
                    using ((IDisposable) proxy)
// ReSharper restore SuspiciousTypeConversion.Global
                    {
                        return operation(proxy);
                    }
                }
                catch (CommunicationObjectFaultedException)
                {
                    if (attempt >= 3)
                        throw;
                    Log.ErrorFormat("Communication attempt #{0} with {1} failed, retrying...", attempt, strategy);
                }
            }
        }
    }
}
