using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;

namespace BLL.Interfaces;
public interface IOrderRepository
{
    Order GetOrderById(int orderId);
    List<Order> GetAllOrders();
    string GetCustomerById(int customerId);

    string GetCustomerEmailById(int customerId);

    string GetCustomerPhoneById(int customerId);

    int GetTableIdByCustomerId(int customerId);

    int GetReviewById(int reviewId);
    string GetPaymentModeById(int paymentId);
    string GetOrderStatusById(int statusId);

    Payment GetPaymentByOrderId(int orderId);

    List<OrderItem> GetOrderItemsByOrderId(int orderId);

    List<OrderTable> GetOrderTablesByOrderId(int orderId);

    List<OrderTax> GetOrderTaxesByOrderId(int orderId);

    List<OrderModifier> GetOrderModifiersByOrderItemIdAndItemId(int orderItemId, int itemId);
    List<OrderStatus> GetAllOrderStatuses();
    int GetOrderStatusIdByStatusName(string statusName);

}