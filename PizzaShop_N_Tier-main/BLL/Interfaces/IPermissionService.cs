using System.Threading.Tasks;
using DAL.Models;
using DAL.ViewModel;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;
public interface IPermissionService
{
    Permission GetPermissionByName(string name);
    List<PermissionVM> GetPermissionsForRole(int roleId);
    bool UpdatePermissions(List<PermissionVM> permissions);
    public int GetRoleIdByRoleName(string rolename);
}