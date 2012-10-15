using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Hp.Merlin.HedgeSense
{
    [Serializable]
    public class StrategyGoal
    {
        private const string CsvHeader =
            "Strategy,Instrument,Direction,AvgPrice,Target,TakeProfitPrice,StopLossPrice,OpenTimestamp,CurrentPosition,RealizedPnl";

        private static readonly CultureInfo StorageCulture = CultureInfo.InvariantCulture;

        private readonly string _instrument;
        private int _target;
        private double _takeProfitPrice;
        private double _stopLossPrice;
        private DateTime _openTimestamp;
        private int _currentPosition;
        private double _avgPrice;
        private double _realizedPnl;

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

        public StrategyGoal(
            string instrument, int target = 0, double takeProfitPrice = 0, double stopLossPrice = 0,
            DateTime openTimestamp = default(DateTime), int currentPosition = 0,
            double avgPrice = 0, double realizedPnl = 0)
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
        }

        private StrategyGoal(StrategyGoal s)
            : this(
                s._instrument, s._target, s._takeProfitPrice, s._stopLossPrice,
                s._openTimestamp, s._currentPosition,
                s._avgPrice, s._realizedPnl)
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
                if (line.Length == 0)
                    continue;
                try
                {
                    var parts = line.Split('\t', ',');
                    if (parts.Length < 8)
                        throw new Exception("Not enough columns");
                    for (int i = 0; i < parts.Length; i++)
                        parts[i] = parts[i].Trim('"', '\'', ' ');

                    var strategy = parts[0];
                    var symbol = parts[1];
                    var directionStr = parts[2];
                    var avgPriceStr = parts[3];
                    var targetStr = parts[4];
                    var limitStr = parts[5];
                    var stopStr = parts[6];
                    var openDateStr = parts[7];
                    var currentPositionStr = parts.Length > 8 ? parts[8] : null;
                    var realizedPnlStr = parts.Length > 9 ? parts[9] : null;

                    // check header presence: not header if "Target" is empty or a number
                    double number;
                    if (lineIdx == 1 && (targetStr.Length != 0 && !double.TryParse(targetStr, out number)))
                    {
                        hasHeader = true;
                        continue;
                    }

                    if (strategyName == null)
                        strategyName = strategy;
                    else if (strategy != strategyName)
                        throw new Exception("Mismatch between strategy names");

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

                    var limit = limitStr.Length == 0 ? 0 : double.Parse(limitStr);
                    var stop = stopStr.Length == 0 ? 0 : double.Parse(stopStr);

                    DateTime openDate;
                    DateTime.TryParseExact(
                        openDateStr, new[] {"M/d/yyyy", "M-d-yyyy", "yyyy/d/M", "yyyy-d-M"},
                        StorageCulture, DateTimeStyles.None, out openDate);

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

                    var goal = new StrategyGoal(
                        symbol, target, limit, stop, 
                        openDate, currentPosition, avgPrice, realizedPnl);
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

        public static void SaveGoals(string strategyName, bool includeHeader, IEnumerable<StrategyGoal> goals, string filePath)
        {
            if (goals == null) throw new ArgumentNullException("goals");
            if (filePath == null) throw new ArgumentNullException("filePath");

            using (var file = new StreamWriter(filePath))
                SaveGoals(strategyName, includeHeader, goals, file);
        }

        public static void SaveGoals(string strategyName, bool includeHeader, IEnumerable<StrategyGoal> goals, TextWriter output)
        {
            if (goals == null) throw new ArgumentNullException("goals");
            if (output == null) throw new ArgumentNullException("output");

            if (includeHeader)
                output.WriteLine(CsvHeader);
            foreach (var g in goals)
                output.WriteLine(g.ToCsvString(strategyName));
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
            return res.ToString();
        }

        public string ToCsvString(string strategyName)
        {
            return string.Join(
                ",",
                strategyName,
                _instrument,
                _target == 0 ? "" : _target > 0 ? "Long" : "Short",
                _avgPrice == 0 ? "" : _avgPrice.ToString(StorageCulture),
                _target == 0 ? "" : Math.Abs(_target).ToString(StorageCulture),
                _takeProfitPrice == 0 ? "" : _takeProfitPrice.ToString(StorageCulture),
                _stopLossPrice == 0 ? "" : _stopLossPrice.ToString(StorageCulture),
                _openTimestamp == default(DateTime) ? "" : _openTimestamp.ToString("MM/dd/yyyy", StorageCulture),
                _currentPosition == 0 ? "" : _currentPosition.ToString(StorageCulture),
                _realizedPnl == 0 ? "" : _realizedPnl.ToString(StorageCulture));
        }
    }
}