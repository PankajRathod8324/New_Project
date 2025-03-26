using System.ComponentModel.DataAnnotations;
using DAL.Models;
using X.PagedList;

namespace DAL.ViewModel
{
    public class OrderVM
    {
        public List<Order> AllOrders { get; set; }

        public IPagedList<Order>? Orders { get; set; }

        public List<OrderItemVM> OrderItems { get; set; }

        public List<OrderTableVM> OrderTables { get; set; }

        public List<OrderTaxVM> OrderTax {get; set;}

        public List<OrderPaymentVM> OrderPayment {get; set;}

        public string InvoiceNo {get; set;}

        public DateTime PaidOn {get; set;}

        public DateTime PlacedOn {get; set;}

        public DateTime ModifiedOn {get; set;}

        public TimeSpan Duration {get; set;}

        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerPhone { get; set; }

        public int NoOfPerson { get; set; }

        public DateOnly Date { get; set; }

        public int? ReviewId { get; set; }

        public int ReviewRating { get; set; }

        public string? Comment { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TotalAmount { get; set; }

        public int? OrderStatusId { get; set; }

        public string OrderStatus { get; set; }

        public int? PaymentModeId { get; set; }
        
        public string PaymentMode { get; set; }

        public DateOnly OrderDate {get; set;}

        public string OrderType {get; set;}

        public string PaymentStatus {get; set;}

        public int ItemCount {get; set;}

        public decimal OrderAmount {get; set;}
    }
}