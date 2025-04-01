using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using BLL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using DAL.ViewModel;
using X.PagedList;
using X.PagedList.Extensions;

namespace BLL.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }



    public IEnumerable<MenuCategory> GetAllCategories()
    {
        return _menuRepository.GetAllCategories();
    }

    public MenuCategory GetCategoryById(int id)
    {
        return _menuRepository.GetCategoryById(id);
    }

    public void AddCategory(MenuCategory category)
    {
        _menuRepository.AddCategory(category);
    }

    public void UpdateCategory(MenuCategoryVM category)
    {
        _menuRepository.UpdateCategory(category);
    }

    public void DeleteCategory(MenuCategory category)
    {
        _menuRepository.DeleteCategory(category);
    }

    public List<MenuItem> GetItemsByCategoryId(int categoryId)
    {
        return _menuRepository.GetItemsByCategoryId(categoryId);
    }

    public MenuCategoryVM GetEditItemViewModel(int itemId)
    {
        var item = GetItemById(itemId);
        if (item == null)
        {
            return null; // Return null if item not found
        }

        var itemModifiers = GetItemModifier(item.ItemId);

        return new MenuCategoryVM
        {
            ItemId = item.ItemId,
            CategoryId = item.CategoryId,
            ItemName = item.ItemName,
            ItemtypeId = item.ItemtypeId,
            Rate = item.Rate,
            Quantity = item.Quantity,
            UnitId = item.UnitId,
            IsAvailable = item.IsAvailable ?? false,
            TaxDefault = item.TaxDefault,
            TaxPercentage = item.TaxPercentage,
            ShortCode = item.ShortCode,
            Description = item.Description,
            ModifierGroupId = item.ModifierGroupId,
            ModifierGroupIds = itemModifiers.Select(m => new ItemModifierVM
            {
                ItemId = m.ItemId,
                ModifierGroupId = m.ModifierGroupId,
                ModifierGroupName = m.ModifierGroupId != 0 ? GetModifierGroupNameById(m.ModifierGroupId) : "No GroupName",
                MinSelection = m.MinSelection,
                MaxSelection = m.MaxSelection,
                MenuModifiers = GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new ModifierVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList(),
                MenuModifierGroupItem = GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new MenuModifierGroupVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList()
            }).ToList()
        };
    }

    


    public List<ItemModifierGroup> GetItemModifier(int itemid)
    {
        return _menuRepository.GetItemModifier(itemid);
    }

    public string GetModifierGroupNameById(int modifiergroupid)
    {
        return _menuRepository.GetModifierGroupNameById(modifiergroupid);
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        _menuRepository.AddMenuItem(menuItem);
    }

    public void AddMenuItemModifierGroup(ItemModifierGroup menuitemmodifier)
    {
        _menuRepository.AddMenuItemModifierGroup(menuitemmodifier);
    }

    public bool UpdateMenuItem(MenuItem menuItem)
    {
        return _menuRepository.UpdateMenuItem(menuItem);
    }

    public void UpdateMenuItemModifierGroups(int itemId, List<ItemModifierGroup> modifierGroups)
    {
        // Step 1: Remove existing modifier groups for the item
        _menuRepository.DeleteModifierGroupsByItemId(itemId);

        // Step 2: Add new modifier groups
        if (modifierGroups != null && modifierGroups.Any())
        {
            foreach (var modifierGroup in modifierGroups)
            {
                var newModifierGroup = new ItemModifierGroup
                {
                    ItemId = itemId,
                    ModifierGroupId = modifierGroup.ModifierGroupId,
                    MinSelection = modifierGroup.MinSelection,
                    MaxSelection = modifierGroup.MaxSelection
                };

                _menuRepository.AddMenuItemModifierGroup(newModifierGroup);
            }
        }
    }


    public void DeleteItem(List<MenuItem> items)
    {
        _menuRepository.DeleteItems(items);
    }

    public void DeleteModifiers(List<MenuModifier> modifiers)
    {
        _menuRepository.DeleteModifiers(modifiers);
    }


    public IEnumerable<Itemtype> GetAllItemTypes()
    {
        return _menuRepository.GetAllItemTypes();
    }

    public IEnumerable<Unit> GetAllUnits()
    {
        return _menuRepository.GetAllUnits();
    }

    public IEnumerable<MenuModifierGroup> GetAllModifierGroups()
    {
        return _menuRepository.GetAllModifierGroups();
    }
    public MenuModifierGroup GetModifierGroupById(int id)
    {
        return _menuRepository.GetModifierGroupById(id);
    }

    // public string GetModifierNameById(int modifierId, MenuModifierGroupVM modifierGroups)
    // {
    //     return _menuRepository.GetModifierNameById(modifierId, modifierGroups);
    // }

    public IPagedList<MenuModifierGroupVM> GetModifiers(UserFilterOptions filterOptions)
    {

        var modifier = _menuRepository.GetModifiers().AsQueryable();
        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            modifier = modifier.Where(u => u.ModifierName.ToLower().Contains(searchLower) ||
                                     u.ModifierRate.ToString().ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalTables = modifier.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalTables) : 10; // Default 10

        var modifierItems = modifier.Select(item => new MenuModifierGroupVM
        {
            ModifierGroupId = item.ModifierGroupId ?? 0, // Avoid null exception
            ModifierId = item.ModifierId,
            ModifierName = item.ModifierName,
            ModifierRate = item.ModifierRate,
            UnitId = item.UnitId,
            Quantity = item.Quantity,
            ModifierDecription = item.ModifierDecription,
            UnitName = item.UnitId.HasValue ? (_menuRepository.GetUnitById(item.UnitId.Value) ?? "No Unit") : "No Unit"
        }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return modifierItems;
    }
    public string GetUnitById(int unitId)
    {
        return _menuRepository.GetUnitById(unitId);
    }

    public MenuModifier GetModifierById(int modifierid)
    {
        return _menuRepository.GetModifierById(modifierid);
    }

    public int GetTotalModifierCount()
    {
        return _menuRepository.GetTotalModifierCount();
    }


    public void AddCombinedModifierGroup(CombineModifier modifierGroup)
    {
        _menuRepository.AddCombinedModifierGroup(modifierGroup);
    }
    public MenuItem GetItemById(int itemid)
    {
        return _menuRepository.GetItemById(itemid);
    }
    public List<MenuModifier> GetModifiersByModifierGroupId(int modifierGroupId)
    {
        return _menuRepository.GetModifiersByModifierGroupId(modifierGroupId);
    }

    public List<MenuModifierGroup> GetModifierGroupsByModifierId(int modifierId)
    {
        return _menuRepository.GetModifierGroupsByModifierId(modifierId);
    }

    public void RemoveCombinedModifierGroup(int modifierId, int groupId)
    {
        _menuRepository.RemoveCombinedModifierGroup(modifierId, groupId);
    }

    public void AddModifierGroup(MenuModifierGroup modifierGroup)
    {
        _menuRepository.AddModifierGroup(modifierGroup);
    }

    public void UpdateModifierGroup(MenuModifierGroupVM modifierGroup)
    {
        _menuRepository.UpdateModifierGroup(modifierGroup);
    }

    public void DeleteModifierGroup(MenuModifierGroup modifierGroup)
    {
        _menuRepository.DeleteModifierGroup(modifierGroup);
    }

    public void AddModifier(MenuModifier modifier)
    {
        _menuRepository.AddModifier(modifier);
    }

    public void UpdateModifier(MenuModifier modifier)
    {
        _menuRepository.UpdateModifier(modifier);
    }
    public IPagedList<MenuCategoryVM> getFilteredMenuItems(int categoryId, UserFilterOptions filterOptions)
    {
        var menuItems = _menuRepository.GetItemsByCategoryId(categoryId).AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            menuItems = menuItems.Where(u => u.ItemName.ToLower().Contains(searchLower) ||
                                     u.Itemtype.ItemType1.ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalItems = menuItems.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalItems) : 10; // Default 10

        var paginateditems = menuItems
           .Select(item => new MenuCategoryVM
           {
               ItemId = item.ItemId,
               ItemName = item.ItemName,
               UnitId = item.UnitId,
               CategoryId = item.CategoryId,
               ItemtypeId = item.ItemtypeId,
               Rate = item.Rate,
               Quantity = item.Quantity,
               IsAvailable = (bool)item.IsAvailable,
               TaxDefault = item.TaxDefault,
               TaxPercentage = item.TaxPercentage,
               ShortCode = item.ShortCode,
               Description = item.Description,
               ItemPhoto = item.CategoryPhoto
               
               // UnitName =  item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"

           }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return paginateditems;

    }
    public IPagedList<MenuModifierGroupVM> getFilteredMenuModifiers(int groupId, UserFilterOptions filterOptions)
    {
        var menuModifiers = _menuRepository.GetModifiersByModifierGroupId(groupId).AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            menuModifiers = menuModifiers.Where(u => u.ModifierName.ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalItems = menuModifiers.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalItems) : 10; // Default 10

        var paginatedmodifiers = menuModifiers
           .Select(item => new MenuModifierGroupVM
           {
               ModifierId = item.ModifierId,
               ModifierName = item.ModifierName,
               UnitId = item.UnitId,
               ModifierRate = item.ModifierRate,
               Quantity = item.Quantity,
               IsDeleted = item.IsDeleted,
               ModifierDecription = item.ModifierDecription,
               UnitName = item.UnitId.HasValue ? _menuRepository.GetUnitById(item.UnitId.Value) : "No Unit"

           }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return paginatedmodifiers;

    }
}