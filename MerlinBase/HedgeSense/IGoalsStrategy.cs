using System;
using System.Collections.Generic;
using System.ServiceModel;
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
        List<StrategyGoal> GetCurrentGoals();

        [OperationContract]
        List<Order> GetOrderHistory(DateTime start);

        [OperationContract]
        List<Transaction> GetTransactionHistory(DateTime start);

        [OperationContract]
        List<DetailedPnlEntry> GetPnl(DateTime start);
    }
}
