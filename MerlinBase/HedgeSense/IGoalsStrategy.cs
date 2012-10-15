using System.Collections.Generic;
using System.ServiceModel;
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
    }
}
