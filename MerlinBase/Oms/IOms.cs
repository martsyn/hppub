using System.Collections.Generic;
using System.ServiceModel;
using Hp.Merlin.Services;

namespace Hp.Merlin.Oms
{
    [ServiceContract(Namespace = WebConstants.SchemaNamespace)]
    public interface IOms
    {
        /// <summary>
        /// Places new order.
        /// </summary>
        /// <param name="order">Order, containing unique request ID.</param>
        [OperationContract]
        void PlaceOrder(Order order);

        /// <summary>
        /// Cancel/Replace request to adjust previously placed order.
        /// </summary>
        /// <param name="origRequestId">Original request ID.</param>
        /// <param name="order">Adjusted order with new request ID.</param>
        [OperationContract]
        void ModifyOrder(string origRequestId, Order order);

        /// <summary>
        /// Cancel request to cancel remaining quantity of previously placed order.
        /// </summary>
        /// <param name="origRequestId">Original request ID supplied within <see cref="Order"/> in </param>
        /// <param name="requestId">ID for this cancellation request.</param>
        [OperationContract]
        void CancelOrder(string origRequestId, string requestId);

        /// <summary>
        /// Retrieve a list of pending orders placed from specified strategies.
        /// </summary>
        /// <param name="strategies">A list of strategies. If null supplied, orders for all strategies are returned.</param>
        /// <returns></returns>
        [OperationContract]
        List<Order> GetPendingOrders(IList<string> strategies);

        /// <summary>
        /// Retrieve a list of current positions for specified strategies.
        /// </summary>
        /// <param name="strategies">A list of strategies. If null supplied, positions for all strategies are returned.</param>
        /// <returns></returns>
        List<Position> GetPositions(IList<string> strategies);

        /// <summary>
        /// Retrieve a history of orders.
        /// </summary>
        /// <param name="strategies">A list of strategies. If null supplied, positions for all strategies are returned.</param>
        /// <returns></returns>
        List<Position> GetOrderHistory(IList<string> strategies);
    }
}
