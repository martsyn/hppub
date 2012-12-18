using System;
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

    public enum OrderType
    {
        Invalid,
        Market,
        Limit,
        Stop,
        StopLimit,
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
    public class Order
    {
        private string _requestId;
        private string _strategyId;
        private string _symbol;
        private OrderSide _side;
        private int _quantity;
        private OrderType _type;
        private double _price;
        private double _stopPrice;
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

        public OrderType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public double Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public double StopPrice
        {
            get { return _stopPrice; }
            set { _stopPrice = value; }
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
            res.Append(Side);
            res.Append(' ').Append(Quantity);
            res.Append('x').Append(Symbol ?? "<no-symbol>");
            res.Append(' ').Append(Type);
            res.Append(" lmt=").Append(Price);
            res.Append(" stp=").Append(StopPrice);
            res.Append(' ');
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
    }
}
