using System;
using System.Text;

namespace Hp.Merlin.Oms
{
    public enum TransactionAction
    {
        Unknown,
        ManualAdjustment,
        Bought,
        Sold,
        BoughtCover,
        SoldShort,
        Dividend,
        Split,
        Expiration,
        Exercise,
    }

    [Serializable]
    public class Transaction
    {
        private DateTime _timestamp;
        private string _strategyId;
        private string _orderRequestId;
        private string _symbol;
        private int _quantity;
        private double _price;
        private TransactionAction _action;
        private double _commission;
        private string _description;

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public string StrategyId
        {
            get { return _strategyId; }
            set { _strategyId = value; }
        }

        public string OrderRequestId
        {
            get { return _orderRequestId; }
            set { _orderRequestId = value; }
        }

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public TransactionAction Action
        {
            get { return _action; }
            set { _action = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public double Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public double Commission
        {
            get { return _commission; }
            set { _commission = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public override string ToString()
        {
            var res = new StringBuilder();
            res.Append(OrderRequestId ?? "<no-id>");
            res.Append('(').Append(StrategyId ?? "<no-strat>").Append("): ");
            res.Append(Action).Append(' ');
            res.Append(Quantity);
            res.Append('x');
            res.Append(Symbol ?? "<no-symbol>");
            res.Append(" @").Append(Price);
            if (Commission != 0)
                res.Append(" comm=$").Append(Commission);
            if (Timestamp != default(DateTime))
                res.Append(' ').Append(Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            if (Description != null)
                res.Append(' ').Append(Description);

            return res.ToString();
        }
    }
}
