using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;

namespace BLL.Interfaces;
public interface ICustomerRepository
{
   List<Customer> GetAllCustomers();

   List<Order> GetOrderByCustomerId(int customerId);

   int GetOrderIdByCustomerId(int customerId);
   Customer GetCustomerById(int customerId);

   decimal GetMaxOrderValue(int customerId);

   decimal GetAverageOrderValue(int customerId);

   DateTime GetLastVisit(int customerId);

   string GetPaymentStatusNameByOrderId(int orderId);

}