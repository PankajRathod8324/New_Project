using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;

namespace BLL.Interfaces;
public interface IPermissionRepository
{
    Permission GetPermissionByName(string name);
    List<PermissionVM> GetAllPermissionsByroleId(int roleId);
    public int GetRoleIdByRoleName(string rolename);
    bool UpdateRolePermissions(List<PermissionVM> permissions);

}