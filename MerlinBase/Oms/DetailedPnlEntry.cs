﻿using System;

namespace Hp.Merlin.Oms
{
    [Serializable]
    public class DetailedPnlEntry
    {
        private string _strategy;
        private string _symbol;
        private double _lastClosePosition;
        private double _lastClosePrice;
        private double _lastCloseMarketValue;
        private int    _transactionCount;    
        private double _boughtAmount;
        private double _boughtAvgPrice;
        private double _soldAmount;
        private double _soldAvgPrice;
        private double _manualAdjustments;
        private int    _otherTransactions;
        private double _avgPriceSinceOpen;
        private double _currentPosition;
        private double _currentPrice;
        private double _currentMarketValue;
        private double _realizedPnl;
        private double _unrealizedPnl;
        private double _totalPnl;

        public string Strategy
        {
            get { return _strategy; }
            set { _strategy = value; }
        }

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public double LastClosePosition
        {
            get { return _lastClosePosition; }
            set { _lastClosePosition = value; }
        }

        public double LastClosePrice
        {
            get { return _lastClosePrice; }
            set { _lastClosePrice = value; }
        }

        public double LastCloseMarketValue
        {
            get { return _lastCloseMarketValue; }
            set { _lastCloseMarketValue = value; }
        }

        public int TransactionCount
        {
            get { return _transactionCount; }
            set { _transactionCount = value; }
        }

        public double BoughtAmount
        {
            get { return _boughtAmount; }
            set { _boughtAmount = value; }
        }

        public double BoughtAvgPrice
        {
            get { return _boughtAvgPrice; }
            set { _boughtAvgPrice = value; }
        }

        public double SoldAmount
        {
            get { return _soldAmount; }
            set { _soldAmount = value; }
        }

        public double SoldAvgPrice
        {
            get { return _soldAvgPrice; }
            set { _soldAvgPrice = value; }
        }

        public double ManualAdjustments
        {
            get { return _manualAdjustments; }
            set { _manualAdjustments = value; }
        }

        public int OtherTransactions
        {
            get { return _otherTransactions; }
            set { _otherTransactions = value; }
        }

        public double AvgPriceSinceOpen
        {
            get { return _avgPriceSinceOpen; }
            set { _avgPriceSinceOpen = value; }
        }

        public double CurrentPosition
        {
            get { return _currentPosition; }
            set { _currentPosition = value; }
        }

        public double CurrentPrice
        {
            get { return _currentPrice; }
            set { _currentPrice = value; }
        }

        public double CurrentMarketValue
        {
            get { return _currentMarketValue; }
            set { _currentMarketValue = value; }
        }

        public double RealizedPnl
        {
            get { return _realizedPnl; }
            set { _realizedPnl = value; }
        }

        public double UnrealizedPnl
        {
            get { return _unrealizedPnl; }
            set { _unrealizedPnl = value; }
        }

        public double TotalPnl
        {
            get { return _totalPnl; }
            set { _totalPnl = value; }
        }
    }
}
