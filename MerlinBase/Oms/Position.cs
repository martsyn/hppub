using System;

namespace Hp.Merlin.Oms
{
    [Serializable]
    public class Position
    {
        private string _strategyId;
        private string _symbol;
        private double _amount;
        private DateTime _timestamp;
        private double _avgPrice;
        private double _realizedPnl;

        public Position()
        {
        }

        public Position(Position source)
        {
            _strategyId = source._strategyId;
            _symbol = source._symbol;
            _amount = source._amount;
            _timestamp = source._timestamp;
            _avgPrice = source._avgPrice;
            _realizedPnl = source._realizedPnl;
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

        public double Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
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

        public virtual Position Copy()
        {
            return new Position(this);
        }
    }
}
