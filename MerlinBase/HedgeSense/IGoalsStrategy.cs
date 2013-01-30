using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Hp.Merlin.Oms;
using Hp.Merlin.Services;

namespace Hp.Merlin.HedgeSense
{
    [ServiceContract(Namespace = WebConstants.SchemaNamespace)]
    public interface IGoalsStrategy
    {
        [OperationContract]
        void SetGoals(IList<StrategyGoal> goals);

        [OperationContract]
        void Close(IList<string> symbols);

        [OperationContract]
        List<StrategyGoal> GetCurrentGoals();

        [OperationContract]
        List<Order> GetOrderHistory(DateTime start);

        [OperationContract]
        List<Transaction> GetTransactionHistory(DateTime start);

        [OperationContract]
        List<DetailedPnlEntry> GetPnl(DateTime start);

        [OperationContract]
        List<DetailedPnlEntry> GetEodPnl(DateTime start);
    }

    public static class SecurityMaster
    {
        public static string ToHSSymbol(string symbol)
        {
            return symbol != null ? symbol.Replace('-', '/') : null;
        }

        public static string FromHSSymbol(string symbol)
        {
            return symbol != null ? symbol.Replace('/', '-') : null;
        }

        public static string ToHSString(this Order i)
        {
            var res = new StringBuilder();
            res.Append(i.RequestId ?? "<no-id>");
            res.Append('(');
            res.Append(i.StrategyId ?? "<no-strat>");
            res.Append("): ");
            res.Append(i.Side);
            res.Append(' ').Append(i.Quantity);
            res.Append('x').Append(ToHSSymbol(i.Symbol) ?? "<no-symbol>");
            res.Append(' ').Append(i.Type);
            res.Append(" lmt=").Append(i.Price);
            res.Append(" stp=").Append(i.StopPrice);
            res.Append(' ');
            if (i.TimeInForce != default(OrderTimeInForce))
                res.Append(" - ").Append(i.TimeInForce);
            res.Append(": ");
            res.Append(i.Status);
            if (i.Timestamp != default(DateTime))
                res.Append(' ').Append(i.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            if (i.Description != null)
                res.Append(' ').Append(i.Description);

            return res.ToString();
        }

        public static string ToCsvString(this Order i)
        {
            return string.Join(
                ",", new object[]
                    {
                        i.RequestId,
                        i.StrategyId,
                        ToHSSymbol(i.Symbol),
                        i.Side,
                        i.Quantity,
                        i.Type,
                        i.Price,
                        i.StopPrice,
                        i.TimeInForce,
                        i.Status,
                        i.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        i.LastFilledQuantity,
                        i.LastFilledPrice,
                        i.TotalFilledQuantity,
                        i.TotalFilledAvgPrice,
                        i.Description,
                    });
        }

        public static string OrderCsvHeader
        {
            get
            {
                return string.Join(
                    ",", new[]
                        {
                            "RequestId",
                            "StrategyId",
                            "Symbol",
                            "Side",
                            "Quantity",
                            "Type",
                            "Price",
                            "StopPrice",
                            "TimeInForce",
                            "Status",
                            "Timestamp",
                            "LastFilledQuantity",
                            "LastFilledPrice",
                            "TotalFilledQuantity",
                            "TotalFilledAvgPrice",
                            "Description",
                        });
            }
        }

        public static string ToHSString(this Transaction i)
        {
            var res = new StringBuilder();
            res.Append(i.OrderRequestId ?? "<no-id>");
            res.Append('(').Append(i.StrategyId ?? "<no-strat>").Append("): ");
            res.Append(i.Action).Append(' ');
            res.Append(i.Quantity);
            res.Append('x');
            res.Append(ToHSSymbol(i.Symbol) ?? "<no-symbol>");
            res.Append(" @").Append(i.Price);
            if (i.Commission != 0)
                res.Append(" comm=$").Append(i.Commission);
            if (i.Timestamp != default(DateTime))
                res.Append(' ').Append(i.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            if (i.Description != null)
                res.Append(' ').Append(i.Description);

            return res.ToString();
        }

        public static string ToCsvString(this Transaction i)
        {
            return string.Join(
                ",", new object[]
                    {
                        i.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        i.StrategyId,
                        i.OrderRequestId,
                        ToHSSymbol(i.Symbol),
                        i.Quantity,
                        i.Price,
                        i.Action,
                        i.Commission,
                        i.Description,
                    });
        }

        public static string TransactionCsvHeader
        {
            get
            {
                return string.Join(
                    ",", new[]
                        {
                            "Timestamp",
                            "StrategyId",
                            "OrderRequestId",
                            "Symbol",
                            "Quantity",
                            "Price",
                            "Action",
                            "Commission",
                            "Description",
                        });
            }
        }

        public static string ToHSString(this DetailedPnlEntry i)
        {
            return
                string.Format(
                    "RecordDate: {19:yyyy/MM/dd}, Strategy: {0}, Symbol: {1}, LastClosePosition: {2}, LastClosePrice: {3}, LastCloseMarketValue: {4}, TransactionCount: {5}, BoughtAmount: {6}, BoughtAvgPrice: {7}, SoldAmount: {8}, SoldAvgPrice: {9}, ManualAdjustments: {10}, OtherTransactions: {11}, AvgPriceSinceOpen: {12}, CurrentPosition: {13}, CurrentPrice: {14}, CurrentMarketValue: {15}, RealizedPnl: {16}, UnrealizedPnl: {17}, TotalPnl: {18}",
                    i.Strategy, ToHSSymbol(i.Symbol), i.LastClosePosition, i.LastClosePrice, i.LastCloseMarketValue,
                    i.TransactionCount, i.BoughtAmount, i.BoughtAvgPrice, i.SoldAmount, i.SoldAvgPrice,
                    i.ManualAdjustments, i.OtherTransactions, i.AvgPriceSinceOpen, i.CurrentPosition, i.CurrentPrice,
                    i.CurrentMarketValue, i.RealizedPnl, i.UnrealizedPnl, i.TotalPnl, i.RecordDate);
        }

        public static string ToCsvString(this DetailedPnlEntry i)
        {
            return string.Join(
                ",", new object[]
                    {
                        i.RecordDate.ToString("yyyy/MM/dd"),
                        i.Strategy,
                        ToHSSymbol(i.Symbol),
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
                    });
        }

        public static string DetailedPnlEntryCsvHeader
        {
            get
            {
                return string.Join(
                    ",", new[]
                        {
                            "RecordDate",
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
                        });
            }
        }
    }
}
