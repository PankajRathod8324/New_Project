using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;

namespace BLL.Interfaces;
public interface ITableAndSectionRepository
{
   List<Section> GetAllSection();

   Section GetSectionById(int sectionId);

   void AddSection(Section section);

   void EditSection(Section section);

   void DeleteSection(Section section);

   IEnumerable<TableStatus> GetAllStatus();

   List<Table> GetTablesBySectionId(int sectionId);

   string GetStatusById(int statusId);

   string GetSectionNameByTableId(int tableId);

   string GetTableNameByTableId(int tableId);

   void AddTable(Table table);

   Table GetTableById(int tableId);

   bool UpdateTable(Table table);
   void DeleteTables(List<Table> tables);
}