using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

namespace Hp.Merlin.HedgeSense
{
    public enum StrategyGoalVersion
    {
        Original,
        DelayedBracket,
    }

    [Serializable]
    public class StrategyGoal
    {
        private static readonly string[] CsvHeader = new[]
            {
                "Strategy", "Instrument", "Direction", "AvgPrice", "Target", "TakeProfitPrice", "StopLossPrice",
                "OpenTimestamp", "CurrentPosition", "RealizedPnl",
                "NextBracketEffectiveDate", "CurrentTakeProfitPrice", "CurrentStopLossPrice",
            };

        private static readonly CultureInfo StorageCulture = CultureInfo.InvariantCulture;

        private readonly string _instrument;
        private int _target;
        private double _takeProfitPrice;
        private double _stopLossPrice;
        private DateTime _openTimestamp;
        private int _currentPosition;
        private double _avgPrice;
        private double _realizedPnl;
        
        private DateTime _newBracketEffectiveDate;
        private double _currentTakeProfitPrice;
        private double _currentStopLossPrice;

        public string Instrument
        {
            get { return _instrument; }
        }

        public int Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public double TakeProfitPrice
        {
            get { return _takeProfitPrice; }
            set { _takeProfitPrice = value; }
        }

        public double StopLossPrice
        {
            get { return _stopLossPrice; }
            set { _stopLossPrice = value; }
        }

        public DateTime OpenTimestamp
        {
            get { return _openTimestamp; }
            set { _openTimestamp = value; }
        }

        public int CurrentPosition
        {
            get { return _currentPosition; }
            set { _currentPosition = value; }
        }

        public double AvgPrice
        {
            get { return _avgPrice; }
            set { _avgPrice = value; }
        }

        public double RealizedPnl
        {
            get { return _realizedPnl; }
            set { _realizedPnl = value; }
        }

        public DateTime NewBracketEffectiveDate
        {
            get { return _newBracketEffectiveDate; }
            set { _newBracketEffectiveDate = value; }
        }

        public double CurrentTakeProfitPrice
        {
            get { return _currentTakeProfitPrice; }
            set { _currentTakeProfitPrice = value; }
        }

        public double CurrentStopLossPrice
        {
            get { return _currentStopLossPrice; }
            set { _currentStopLossPrice = value; }
        }

        public StrategyGoal(
            string instrument, int target = 0, double takeProfitPrice = 0, double stopLossPrice = 0,
            DateTime openTimestamp = default(DateTime), int currentPosition = 0,
            double avgPrice = 0, double realizedPnl = 0,
            DateTime newBracketEffectiveDate = default(DateTime), double currentTakeProfitPrice = 0, double currentStopLossPrice = 0)
        {
            if (string.IsNullOrEmpty(instrument)) throw new ArgumentNullException("instrument");

            _instrument = instrument;
            _target = target;
            _takeProfitPrice = takeProfitPrice;
            _stopLossPrice = stopLossPrice;
            _openTimestamp = openTimestamp;
            _currentPosition = currentPosition;
            _avgPrice = avgPrice;
            _realizedPnl = realizedPnl;
            _newBracketEffectiveDate = newBracketEffectiveDate;
            _currentTakeProfitPrice = currentTakeProfitPrice;
            _currentStopLossPrice = currentStopLossPrice;
        }

        private StrategyGoal(StrategyGoal s)
            : this(
                s._instrument, s._target, s._takeProfitPrice, s._stopLossPrice,
                s._openTimestamp, s._currentPosition,
                s._avgPrice, s._realizedPnl,
                s._newBracketEffectiveDate, s._currentTakeProfitPrice, s._currentStopLossPrice)
        {
        }

        public StrategyGoal Copy()
        {
            return new StrategyGoal(this);
        }

        public static List<StrategyGoal> LoadGoals(string filePath, bool requireHeader, out string strategyName)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            strategyName = null;
            var goals = new List<StrategyGoal>();
            int lineIdx = 0;
            bool hasHeader = false;
            foreach (string line in File.ReadLines(filePath))
            {
                ++lineIdx;

                // test for header by testing 4th column (Target) not being empty or a number
                if (lineIdx == 1)
                {
                    var parts = line.Split(new[] {'\t', ','}, 6);
                    double number;
                    if (parts.Length == 6 && parts[4] != null && !double.TryParse(parts[4], out number))
                        hasHeader = true;
                    continue;
                }

                if (line.Length == 0)
                    continue;
                try
                {
                    string strategy;
                    var goal = FromCsvString(line, out strategy);

                    if (strategyName == null)
                        strategyName = strategy;
                    else if (strategy != strategyName)
                        throw new Exception("Mismatch between strategy names");

                    goals.Add(goal);
                }
                catch (Exception x)
                {
                    throw new Exception(
                        string.Format(
                            "Error processing line {0}: {1}\nFormat expected: {2}",
                            lineIdx, x.Message, CsvHeader));
                }
            }

            if (requireHeader && !hasHeader)
                throw new Exception("File requires header");

            return goals;
        }

        private static StrategyGoal FromCsvString(string line, out string strategy)
        {
            var parts = line.Split(new[] {'\t', ','});
            if (parts.Length < 8)
                throw new Exception("Not enough columns");
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim('"', '\'', ' ');

            strategy = parts[0];
            var symbol = parts[1];
            var directionStr = parts[2];
            var avgPriceStr = parts[3];
            var targetStr = parts[4];
            var limitStr = parts[5];
            var stopStr = parts[6];
            var openDateStr = parts[7];
            var currentPositionStr = parts.Length > 8 ? parts[8] : null;
            var realizedPnlStr = parts.Length > 9 ? parts[9] : null;
            var newBracketEffectiveDateStr = parts.Length > 10 ? parts[10] : null;
            var currentTakeProfitPriceStr = parts.Length > 11 ? parts[11] : null;
            var currentStopLossPriceStr = parts.Length > 12 ? parts[12] : null;

            if (symbol.Length == 0)
                throw new Exception("Instrument is missing");

            double avgPrice;
            double.TryParse(avgPriceStr, out avgPrice);

            int amount = targetStr.Length == 0 ? 0 : int.Parse(targetStr);
            if (amount < 0) throw new Exception("Target cannot be negative");
            int target;
            if (amount != 0)
            {
                int direction;
                switch (directionStr.ToLowerInvariant())
                {
                    case "short":
                        direction = -1;
                        break;
                    case "long":
                        direction = 1;
                        break;
                    default:
                        throw new Exception("Direction short or long direction is expected");
                }
                target = amount*direction;
            }
            else
                target = 0;

            var limit = ParseDouble(limitStr);
            var stop = ParseDouble(stopStr);

            var openDate = ParseDate(openDateStr);

            int currentPosition;
            if (currentPositionStr != null)
                int.TryParse(currentPositionStr, out currentPosition);
            else
                currentPosition = 0;

            double realizedPnl;
            if (realizedPnlStr != null)
                double.TryParse(realizedPnlStr, out realizedPnl);
            else
                realizedPnl = 0;

            var newBracketEffectiveDate = ParseDate(newBracketEffectiveDateStr);
            var currentTakeProfitPrice = ParseDouble(currentTakeProfitPriceStr, limit);
            var currentStopLossPrice = ParseDouble(currentStopLossPriceStr, stop);

            var goal = new StrategyGoal(
                symbol, target, limit, stop,
                openDate, currentPosition, avgPrice, realizedPnl,
                newBracketEffectiveDate, currentTakeProfitPrice, currentStopLossPrice);
            return goal;
        }

        private static double ParseDouble(string str, double defaultVal = 0)
        {
            if (string.IsNullOrEmpty(str))
                return defaultVal;
            return double.Parse(str);
        }

        private static DateTime ParseDate(string dateStr)
        {
            if (dateStr == null)
                return default(DateTime);
            DateTime date;
            DateTime.TryParseExact(
                dateStr, new[] {"M/d/yyyy", "M-d-yyyy", "yyyy/d/M", "yyyy-d-M"},
                StorageCulture, DateTimeStyles.None, out date);
            return date;
        }

        public static void SaveGoals(string strategyName, bool includeHeader, IEnumerable<StrategyGoal> goals, string filePath, StrategyGoalVersion version)
        {
            if (goals == null) throw new ArgumentNullException("goals");
            if (filePath == null) throw new ArgumentNullException("filePath");

            using (var file = new StreamWriter(filePath))
                SaveGoals(strategyName, includeHeader, goals, file, version);
        }

        public static void SaveGoals(string strategyName, bool includeHeader, IEnumerable<StrategyGoal> goals, TextWriter output, StrategyGoalVersion version)
        {
            if (goals == null) throw new ArgumentNullException("goals");
            if (output == null) throw new ArgumentNullException("output");

            int columnNumber =
                version >= StrategyGoalVersion.DelayedBracket
                    ? CsvHeader.Length
                    : Array.IndexOf(CsvHeader, "NextBracketEffectiveDate");

            if (includeHeader)
                output.WriteLine(string.Join(",", CsvHeader.Take(columnNumber)));
            foreach (var g in goals)
                output.WriteLine(g.ToCsvString(strategyName, version));
        }

        public override string ToString()
        {
            var res = new StringBuilder();
            res.AppendFormat(
                "{0,-5}x{1,6} limit={2,8} stop={3,8}",
                _instrument, _target,
                _takeProfitPrice != 0 ? _takeProfitPrice.ToString("$0.00####") : "n/a",
                _stopLossPrice != 0 ? _stopLossPrice.ToString("$0.00####") : "n/a");
            if (_openTimestamp != default(DateTime))
                res.AppendFormat(" opened={0:MM/dd/yyyy}", _openTimestamp);
            if (_currentPosition != 0)
                res.AppendFormat(" curPos={0}", _currentPosition);
            if (_avgPrice != 0)
                res.AppendFormat(" avgPrice=${0}", _avgPrice);

            if (_newBracketEffectiveDate != default(DateTime))
            {
                res.AppendFormat(
                    " (current expiring {0:MM/dd/yyyy}: limit={1,8} stop={2,8})",
                    _newBracketEffectiveDate,
                    _currentTakeProfitPrice != 0 ? _currentTakeProfitPrice.ToString("$0.00####") : "n/a",
                    _currentStopLossPrice != 0 ? _currentStopLossPrice.ToString("$0.00####") : "n/a");
            }
            return res.ToString();
        }

        private string ToCsvString(string strategyName, StrategyGoalVersion version)
        {
            var columns = new List<string>
                {
                    strategyName,
                    _instrument,
                    _target == 0 ? "" : _target > 0 ? "Long" : "Short",
                    FormatDouble(_avgPrice),
                    _target == 0 ? "" : Math.Abs(_target).ToString(StorageCulture),
                    FormatDouble(_takeProfitPrice),
                    FormatDouble(_stopLossPrice),
                    FormatDate(_openTimestamp),
                    _currentPosition == 0 ? "" : _currentPosition.ToString(StorageCulture),
                    FormatDouble(_realizedPnl),
                };

            if (version >= StrategyGoalVersion.DelayedBracket)
            {
                columns.AddRange(new[]
                    {
                        FormatDate(_newBracketEffectiveDate),
                        FormatDouble(_currentTakeProfitPrice),
                        FormatDouble(_currentStopLossPrice),
                    }); 
            }
            return string.Join(",", columns);
        }

        private static string FormatDate(DateTime val)
        {
            return val == default(DateTime) ? "" : val.ToString("MM/dd/yyyy", StorageCulture);
        }

        private static string FormatDouble(double val)
        {
            return val == 0 || double.IsNaN(val) ? "" : val.ToString(StorageCulture);
        }
    }
}