using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace BLL.Interfaces;
public interface ITableAndSectionService
{
   List<Section> GetAllSection();

   void AddSection(SectionVM model);

   void EditSection(SectionVM model);

   void DeleteSection(int sectionId);

   IEnumerable<TableStatus> GetAllStatus();

   List<Table> GetTablesBySectionId(int sectionId);

   IPagedList<SectionVM> getFilteredMenuTables(int sectionId, UserFilterOptions filterOptions);

   string GetStatusById(int statusId);

    void AddTable(Table table);

    Table GetTableById(int tableId);

    bool UpdateTable(Table table);

     void DeleteTables(List<Table> tables);
}