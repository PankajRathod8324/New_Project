using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;

namespace BLL.Interfaces;
public interface ITaxRepository
{
    List<Taxis> GetAllTaxes();
    List<TaxType> GetAllTaxTypes();

    string GetTaxNameById(int taxId);
    decimal GetTaxRateById(int taxId);

    int GetTexTypeIdByTaxId(int taxId);

    string GetTaxTypeNameById(int statusId);

    Taxis GetTaxById(int taxId);
    void AddTax(Taxis tax);
    bool UpdateTax(Taxis tax);
    void DeleteTax(Taxis tax);

}