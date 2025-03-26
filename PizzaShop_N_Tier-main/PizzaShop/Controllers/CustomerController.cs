using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using BLL.Interfaces;
using BLL.Repository;
using ClosedXML.Excel;
using DAL.Models;
using DAL.ViewModel;
using DinkToPdf;
using DinkToPdf.Contracts;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Rotativa.AspNetCore;
using SelectPdf;

namespace PizzaShop.Controllers;


public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    public CustomerController(ICustomerService customerService)
    {
        _customerService= customerService;
    }


    // [Authorize(Policy = "CustomersViewPolicy")]
    public IActionResult Customer(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        ViewBag.PageSize = filterOptions.PageSize;

     

        var ordervm = _customerService.GetFilteredCustomers(filterOptions, orderStatus, filterdate, startDate, endDate);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_CustomerPV", ordervm);
        }
        return View(ordervm);
    }

   

    public IActionResult CustomerDetails(int customerId)
    {
        var customer = _customerService.GetCustomerByCustomerId(customerId);
        if (customer == null)
        {
            return NotFound("Customer not found");
        }

        return PartialView("_CustomerDetailsPV", customer);
    }


  


    private int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}