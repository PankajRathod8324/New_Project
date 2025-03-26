using System.ComponentModel.DataAnnotations;
using DAL.Models;
using X.PagedList;

namespace DAL.ViewModel
{
    public class OrderItemVM
    {
        public int ItemId { get; set; }

        public int ModifierId { get; set; }

        public string ItemName { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public int Quantity { get; set; }


        public int OrderId { get; set; }
        public List<OrderModifierVM> Modifiers { get; set; }


    }
}