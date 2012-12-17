using System;
using System.Runtime.Serialization;
using System.Text;

namespace Hp.Merlin.Oms
{
    public enum OrderSide
    {
        Invalid,
        Buy,
        Sell,
        SellShort,
    }

    public enum OrderStatus
    {
        None,
        Submitted,
        Executed,
        Cancelled,
        Pending,
        Partial,
        Replaced,
        Rejected,
        ReplaceRejected,
        CancelRejected,
        Expired,
        CancelPending,
        ReplacePending,
        Suspended,
    }

    public enum OrderTimeInForce
    {
        Day = 0,
        GoodTillCancel = 1,
        AtTheOpening = 2,
        ImmediateOrCancel = 3,
        FillOrKill = 4,
        GoodTillCrossing = 5,
        GoodTillDate = 6,
        AtTheClose = 7,
        GoodThroughCrossing = 8,
        AtCrossing = 9,
    }

    [Serializable]
    [KnownType(typeof(MarketOrder))]
    [KnownType(typeof(LimitOrder))]
    [KnownType(typeof(StopOrder))]
    [KnownType(typeof(StopLimitOrder))]
    public abstract class Order
    {
        private string _requestId;
        private string _strategyId;
        private string _symbol;
        private OrderSide _side;
        private int _quantity;
        private OrderTimeInForce _timeInForce;

        private OrderStatus _status;
        private DateTime _timestamp;
        
        private int _lastFilledQuantity;
        private double _lastFilledPrice;
        private int _totalFilledQuantity;
        private double _totalFilledAvgPrice;

        private string _description;

        public string RequestId
        {
            get { return _requestId; }
            set { _requestId = value; }
        }

        public string StrategyId
        {
            get { return _strategyId; }
            set { _strategyId = value; }
        }

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public OrderSide Side
        {
            get { return _side; }
            set { _side = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public OrderTimeInForce TimeInForce
        {
            get { return _timeInForce; }
            set { _timeInForce = value; }
        }

        public OrderStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public int LastFilledQuantity
        {
            get { return _lastFilledQuantity; }
            set { _lastFilledQuantity = value; }
        }

        public double LastFilledPrice
        {
            get { return _lastFilledPrice; }
            set { _lastFilledPrice = value; }
        }

        public int TotalFilledQuantity
        {
            get { return _totalFilledQuantity; }
            set { _totalFilledQuantity = value; }
        }

        public double TotalFilledAvgPrice
        {
            get { return _totalFilledAvgPrice; }
            set { _totalFilledAvgPrice = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public override string ToString()
        {
            var res = new StringBuilder();
            res.Append(RequestId ?? "<no-id>");
            res.Append('(');
            res.Append(StrategyId ?? "<no-strat>");
            res.Append("): ");
            res.Append(Side).Append(' ');
            res.Append(Quantity);
            res.Append('x');
            res.Append(Symbol ?? "<no-symbol>");
            res.Append(' ');
            res.Append(ParametersToString());
            if (TimeInForce != default(OrderTimeInForce))
                res.Append(" - ").Append(TimeInForce);
            res.Append(": ");
            res.Append(Status);
            if (Timestamp != default(DateTime))
                res.Append(' ').Append(Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            if (_description != null)
                res.Append(' ').Append(_description);

            return res.ToString();
        }

        protected abstract string ParametersToString();
    }

    [Serializable]
    public class MarketOrder : Order
    {
        protected override string ParametersToString()
        {
            return "MKT";
        }
    }

    [Serializable]
    public class LimitOrder : Order
    {
        public double Price { get; set; }
        protected override string ParametersToString()
        {
            return "LMT @" + Price;
        }
    }

    [Serializable]
    public class StopOrder : Order
    {
        public double StopPrice { get; set; }
        protected override string ParametersToString()
        {
            return "STP @" + StopPrice;
        }
    }

    [Serializable]
    public class StopLimitOrder : Order
    {
        public double StopPrice { get; set; }
        public double Price { get; set; }
        protected override string ParametersToString()
        {
            return string.Format("STPLMT @{0}::{1}", StopPrice, Price);
        }
    }
}
