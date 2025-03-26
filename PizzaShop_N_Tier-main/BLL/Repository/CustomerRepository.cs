using BLL.Interfaces;
using DAL.Models;
using DAL.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly PizzaShopContext _context;

    public CustomerRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public List<Customer> GetAllCustomers()
    {
        return _context.Customers.ToList();
    }

    public List<Order> GetOrderByCustomerId(int customerId)
    {
        return _context.Orders.Where(r => r.CustomerId == customerId).ToList();
    }

    public int GetOrderIdByCustomerId(int customerId)
    {
        var orderId = (from order in _context.Orders
                       where order.CustomerId == customerId
                       select order.OrderId).FirstOrDefault();
        return orderId;
    }

    public decimal GetMaxOrderValue(int customerId)
    {
        return _context.Orders
                       .Where(o => o.CustomerId == customerId)
                       .Max(o => o.TotalAmount);
    }

    public decimal GetAverageOrderValue(int customerId)
    {
        return _context.Orders
                        .Where(o => o.CustomerId == customerId)
                        .Average(o => o.TotalAmount);
    }

    public DateTime GetLastVisit(int customerId)
    {
        return _context.Orders  
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => o.CreatedAt)
                .FirstOrDefault();
    }

     public string GetPaymentStatusNameByOrderId(int orderId)
    {
        var paymentstatus = (from order in _context.Payments
                           where order.OrderId == orderId
                           select order.PaymentStatus.PaymentStatusName).FirstOrDefault();
        return paymentstatus;
    }

    public Customer GetCustomerById(int customerId)
    {
        return _context.Customers.FirstOrDefault(r => r.CustomerId == customerId);
    }
}