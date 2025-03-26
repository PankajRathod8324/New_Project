using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace BLL.Interfaces;
public interface ICustomerService
{
    CustomerVM GetCustomerByCustomerId(int customerId);

    IPagedList<CustomerVM> GetFilteredCustomers(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate);

}