using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json.Nodes;
using BLL.Interfaces;
using DAL.Models;
using DAL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

// using PizzaShop.Models;
using X.PagedList.Extensions;

namespace PizzaShop.Controllers;


public class MenuController : Controller
{
    private readonly IMenuService _menuService;

    private readonly String _imageFolderPath = "wwwroot/images/";

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult MenuItem(int categoryId, UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        ViewBag.PageSize = filterOptions.PageSize;

        var categories = _menuService.GetAllCategories();
        ViewBag.Categories = categories.Select(r => new SelectListItem
        {
            Value = r.CategoryId.ToString(),
            Text = r.CategoryName
        }).ToList();

        var itemtypes = _menuService.GetAllItemTypes();
        ViewBag.Itemtypes = itemtypes.Select(r => new SelectListItem
        {
            Value = r.ItemtypeId.ToString(),
            Text = r.ItemType1
        }).ToList();

        var units = _menuService.GetAllUnits();
        ViewBag.Units = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();

        var modifiergroups = _menuService.GetAllModifierGroups();
        ViewBag.ModifierGroups = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();

        var menuitemvm = _menuService.getFilteredMenuItems(categoryId, filterOptions);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_MenuItemPV", menuitemvm);
        }
        return View(menuitemvm);
    }


    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult EditMenuItem(int itemId, int pagenumber)
    {
        // Fetch Categories, Item Types, Units, and Modifier Groups
        ViewBag.Categories = _menuService.GetAllCategories()
            .Select(r => new SelectListItem { Value = r.CategoryId.ToString(), Text = r.CategoryName })
            .ToList();

        ViewBag.Itemtypes = _menuService.GetAllItemTypes()
            .Select(r => new SelectListItem { Value = r.ItemtypeId.ToString(), Text = r.ItemType1 })
            .ToList();

        ViewBag.Units = _menuService.GetAllUnits()
            .Select(r => new SelectListItem { Value = r.UnitId.ToString(), Text = r.UnitName })
            .ToList();

        ViewBag.ModifierGroups = _menuService.GetAllModifierGroups()
            .Select(r => new SelectListItem { Value = r.ModifierGroupId.ToString(), Text = r.ModifierGroupName })
            .ToList();

        // Fetch Item Data
        var item = _menuService.GetItemById(itemId);
        if (item == null)
        {
            return NotFound(); // Return a 404 if the item doesn't exist
        }
        ViewBag.Page = pagenumber;
        Console.WriteLine("In Edit PageNumber: " + pagenumber);

        // Fetch associated modifier groups
        var itemModifiers = _menuService.GetItemModifier(item.ItemId);

        Console.WriteLine("I am IN Water");

        // var modifiers = _menuService.GetModifiersByModifierGroupId((int)item.ModifierGroupId);

        var itemvm = new MenuCategoryVM
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
            ItemPhoto = item.CategoryPhoto,
            ModifierGroupIds = itemModifiers.Select(m => new ItemModifierVM
            {
                ItemId = m.ItemId,
                ModifierGroupId = m.ModifierGroupId,
                ModifierGroupName = m.ModifierGroupId != 0 ? _menuService.GetModifierGroupNameById(m.ModifierGroupId) : "No GroupName",
                MinSelection = m.MinSelection,
                MaxSelection = m.MaxSelection,
                MenuModifiers = _menuService.GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new ModifierVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList(),
                MenuModifierGroupItem = _menuService.GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new MenuModifierGroupVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList()
            }).ToList()
        };



        return PartialView("_EditItemPV", itemvm);
    }



    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult EditMenuItem([FromBody] JsonObject menuItemData)
    {
        if (menuItemData == null)
        {
            return BadRequest("Invalid JSON format. Could not parse data.");
        }

        Console.WriteLine("Raw JSON received: " + menuItemData.ToString());

        // Extract Item ID
        int itemId = TryParseInt(menuItemData["ItemId"]);
        if (itemId <= 0)
        {
            return BadRequest("Invalid Item ID.");
        }

        // Extract individual fields
        string itemName = menuItemData["ItemName"]?.ToString();
        int categoryId = TryParseInt(menuItemData["CategoryId"]);
        int itemTypeId = TryParseInt(menuItemData["ItemtypeId"]);
        decimal rate = TryParseDecimal(menuItemData["Rate"]);
        int quantity = TryParseInt(menuItemData["Quantity"]);
        int unitId = TryParseInt(menuItemData["UnitId"]);
        bool isAvailable = TryParseBool(menuItemData["IsAvailable"]);
        decimal taxPercentage = TryParseDecimal(menuItemData["TaxPercentage"]);
        string shortCode = menuItemData["ShortCode"]?.ToString();
        string description = menuItemData["Description"]?.ToString();
        bool taxDefault = TryParseBool(menuItemData["TaxDefault"]);
        string ItemPhoto = menuItemData["ItemPhoto"]?.ToString();


        var base64String = ItemPhoto.Split(',')[1];

        // Convert base64 string to byte array
        byte[] imageBytes = Convert.FromBase64String(base64String);

        // Generate a unique file name for the image
        var uniqueFileName = Guid.NewGuid().ToString() + ".jpg"; // Or another image extension
        var filePath = Path.Combine(_imageFolderPath, uniqueFileName);

        // Ensure the directory exists
        Directory.CreateDirectory(_imageFolderPath);

        // Save the image to the server using FileStream
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            stream.Write(imageBytes, 0, imageBytes.Length);
        }

        // Store the relative image path (to store in database)
        ItemPhoto = "/images/" + uniqueFileName;
        // Parse Modifier Groups JSON array safely
        List<ItemModifierGroup> modifierGroups = new List<ItemModifierGroup>();
        if (menuItemData.ContainsKey("ModifierGroupIds") && menuItemData["ModifierGroupIds"] != null)
        {
            modifierGroups = JsonConvert.DeserializeObject<List<ItemModifierGroup>>(menuItemData["ModifierGroupIds"].ToString());
        }

        Console.WriteLine($"Updating Item ID: {itemId}, Name: {itemName}");
        Console.WriteLine($"Modifier Groups: {string.Join(",", modifierGroups.Select(m => m.ModifierGroupId))}");

        // Step 1: Update Menu Item
        var menuitem = new MenuItem
        {
            ItemId = itemId,
            CategoryId = categoryId,
            ItemName = itemName,
            ItemtypeId = itemTypeId,
            Rate = rate,
            Quantity = quantity,
            UnitId = unitId,
            IsAvailable = isAvailable,
            TaxPercentage = taxPercentage,
            ShortCode = shortCode,
            Description = description,
            TaxDefault = taxDefault,
            CategoryPhoto = ItemPhoto
            
        };

        bool updateSuccess = _menuService.UpdateMenuItem(menuitem);

        if (!updateSuccess)
        {
            return BadRequest("Failed to update Menu Item.");
        }

        // Step 2: Update Modifier Groups
        _menuService.UpdateMenuItemModifierGroups(itemId, modifierGroups);
        TempData["Message"] = "Item updated successfully!";
        TempData["MessageType"] = "success";

        return Json(new { success = true, message = "Menu Item Updated Successfully!" });
    }


    [Authorize(Policy = "MenuViewPolicy")]

    public IActionResult MenuCategory()
    {
        // var categories = _menuService.GetAllCategories();
        var categories = _menuService.GetAllCategories(); // Fetch Categories from the database
        var categorySelectList = categories.Select(r => new SelectListItem
        {
            Value = r.CategoryId.ToString(),
            Text = r.CategoryName
        }).ToList();
        ViewBag.Categories = categorySelectList;

        var itemtypes = _menuService.GetAllItemTypes(); // Fetch ItemTypes from the database
        var itemtypeSelectList = itemtypes.Select(r => new SelectListItem
        {
            Value = r.ItemtypeId.ToString(),
            Text = r.ItemType1
        }).ToList();
        ViewBag.Itemtypes = itemtypeSelectList;

        var units = _menuService.GetAllUnits(); // Fetch Units from the database
        var unitSelectList = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();
        ViewBag.Units = unitSelectList;

        var modifiergroups = _menuService.GetAllModifierGroups(); // Fetch Units from the database
        var modifiergroupSelectList = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();
        ViewBag.ModifierGroups = modifiergroupSelectList;


        // var categories = _menuService.GetAllCategories();

        var categoryvm = new MenuCategoryVM
        {
            menuCategories = categories
        };


        // foreach (var category in categoryvm.menuCategories)
        // {
        //     Console.WriteLine(category.CategoryName);
        // }
        // return View(categories);

        return PartialView("_MenuCategoryPV", categoryvm);
    }


    // [HttpPost]
    // public IActionResult AddMenuCategory(MenuCategory model)
    // {
    //     Console.WriteLine(ModelState.IsValid);
    //     Console.WriteLine("--------------Add MenuCategory");
    //     Console.WriteLine(model.CategoryName);
    //     if (!ModelState.IsValid)
    //     {
    //         // Log the model state errors
    //         foreach (var state in ModelState)
    //         {
    //             foreach (var error in state.Value.Errors)
    //             {
    //                 Console.WriteLine($"Property: {state.Key}, Error: {error.ErrorMessage}");
    //             }
    //         }
    //         return RedirectToAction("Menu", "Home");
    //     }

    //     var category = new MenuCategory
    //     {
    //         CategoryName = model.CategoryName,
    //         CategoryDescription = model.CategoryDescription
    //     };



    //     _menuService.AddCategory(category);
    //     Console.WriteLine("--------------Add User END");
    //     TempData["Message"] = "Category added successfully!";
    //     TempData["MessageType"] = "success";

    //     return RedirectToAction("Menu", "Home");
    // }


    [Authorize(Policy = "MenuViewPolicy")]
    public IActionResult MenuModifier(int groupId, UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        var units = _menuService.GetAllUnits(); // Fetch Units from the database
        var unitSelectList = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();
        ViewBag.Units = unitSelectList;

        var modifiergroups = _menuService.GetAllModifierGroups(); // Fetch Units from the database
        var modifiergroupSelectList = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();
        ViewBag.ModifierGroups = modifiergroupSelectList;


        Console.WriteLine("Click this : " + groupId);
        filterOptions.Page ??= 1;

        X.PagedList.IPagedList<MenuModifierGroupVM> menumodifiervm = _menuService.getFilteredMenuModifiers(groupId, filterOptions);


        // if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        // {
        //     return PartialView("_MenuModifierPV", menumodifiervm);
        // }


        // MenuModifierGroupVM modifiervm = new MenuModifierGroupVM
        // {
        //     menuModifiers = menumodifiervm
        //     // UnitName = modifiers.UnitId.HasValue ? _menuService.GetUnitById(modifiers.UnitId.Value) : "No Units"
        // };
        return PartialView("_MenuModifierPV", menumodifiervm);
    }

    [Authorize]
    public IActionResult GetAllModifier(UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        var modifiers = _menuService.GetModifiers(filterOptions);

        return PartialView("_MenuModifierByModifierGroup", modifiers);
    }



    [Authorize]
    public IActionResult GetEditAllModifier(int modifierGroupId, UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        var pagedModifiers = _menuService.GetModifiers(filterOptions);
        int totalItems = _menuService.GetTotalModifierCount(); // Implement this in your service

        var associatedModifierIds = _menuService.GetModifiersByModifierGroupId(modifierGroupId)
            .Select(m => m.ModifierId)
            .ToList();

        var menuModifierGroup = new MenuModifierGroupVM
        {
            ModifierGroupId = modifierGroupId,
            Modifier = pagedModifiers,
            ModifierIds = associatedModifierIds
        };

        var viewModel = new PaginatedViewModel<MenuModifierGroupVM>
        {
            Data = menuModifierGroup,
            PageSize = filterOptions.PageSize,
            PageNumber = filterOptions.Page.Value,
            TotalItemCount = totalItems
        };

        return PartialView("_EditModifierGroupTable", viewModel);
    }






    [Authorize]
    public IActionResult GetModifiersByGroup(int groupId, UserFilterOptions filterOptions)
    {
        var modifiers = _menuService.GetModifiersByModifierGroupId(groupId);
        var groupName = _menuService.GetModifierGroupById(groupId);
        // var combinemodifier = _menuService.GetItemModifier()

        var modifieritems = modifiers
           .Select(item => new MenuModifierGroupVM
           {
               ModifierGroupId = (int)item.ModifierGroupId,
               ModifierName = item.ModifierName,
               ModifierRate = item.ModifierRate,
               UnitId = item.UnitId,
               Quantity = item.Quantity,
               ModifierDecription = item.ModifierDecription,
               UnitName = item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"

           }).ToList();
        MenuModifierGroupVM modifiervm = new MenuModifierGroupVM
        {
            menuModifiers = modifieritems,
            ModifierGroupId = groupId,
            ModifierGroupName = groupName.ModifierGroupName


            // UnitName = modifiers.UnitId.HasValue ? _menuService.GetUnitById(modifiers.UnitId.Value) : "No Units"
        };


        // var groupName = _menuService.GetModifierNameById(groupId, modifiervm);
        return PartialView("_ModifierList", modifiervm);
    }

    [Authorize]
    public IActionResult GetModifiersByGroupEdit(int groupId, UserFilterOptions filterOptions)
    {
        var modifiers = _menuService.GetModifiersByModifierGroupId(groupId);
        var groupName = _menuService.GetModifierGroupById(groupId);

        var modifieritems = modifiers.Select(item => new MenuModifierGroupVM
        {
            ModifierGroupId = (int)item.ModifierGroupId,
            ModifierName = item.ModifierName,
            ModifierRate = item.ModifierRate,
            UnitId = item.UnitId,
            Quantity = item.Quantity,
            ModifierDecription = item.ModifierDecription,
            UnitName = item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"
        }).ToList();

        // Creating the ItemModifierVM instance
        ItemModifierVM modifieritemvm = new ItemModifierVM
        {
            ModifierGroupId = groupId,
            ModifierGroupName = groupName.ModifierGroupName,
            MenuModifierGroupItem = modifieritems
        };

        return PartialView("_EditModifierList", modifieritemvm);
    }


    public IActionResult GetModifiersGroupByItem(int groupId, UserFilterOptions filterOptions, int itemId)
    {
        var modifiers = _menuService.GetModifiersByModifierGroupId(groupId);
        var groupName = _menuService.GetModifierGroupById(groupId);
        var combinemodifier = _menuService.GetItemModifier(itemId);

        var modifieritems = modifiers
           .Select(item => new MenuModifierGroupVM
           {
               ModifierName = item.ModifierName,
               ModifierRate = item.ModifierRate,
           }).ToList();
        MenuModifierGroupVM modifiervm = new MenuModifierGroupVM
        {
            menuModifiers = modifieritems,
            ModifierGroupId = groupId,
            ModifierGroupName = groupName.ModifierGroupName,
            itemModifierGroups = combinemodifier.Select(modifier => new ItemModifierVM
            {
                ItemId = modifier.ItemId,
                ModifierGroupId = modifier.ModifierGroupId,
                MaxSelection = modifier.MaxSelection,
                MinSelection = modifier.MinSelection
            }).ToList(),
            // UnitName = modifiers.UnitId.HasValue ? _menuService.GetUnitById(modifiers.UnitId.Value) : "No Units"
        };


        // var groupName = _menuService.GetModifierNameById(groupId, modifiervm);
        return Json(modifiervm);
    }
    // public IActionResult GetModifiers(int page = 1, string search = "")
    // {
    //     int pageSize = 5;
    //     int totalPages;

    //     var modifiers = _menuService.GetModifiers(page, search, pageSize, out totalPages);

    //     return Json(new { data = modifiers, totalPages });
    // }


    [Authorize(Policy = "MenuViewPolicy")]
    public IActionResult MenuModifierGroup()
    {
        var units = _menuService.GetAllUnits(); // Fetch Units from the database
        var unitSelectList = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();
        ViewBag.Units = unitSelectList;

        var modifiergroups = _menuService.GetAllModifierGroups(); // Fetch Units from the database
        var modifiergroupSelectList = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();
        ViewBag.ModifierGroups = modifiergroupSelectList;


        var modifierGroups = _menuService.GetAllModifierGroups();

        var modifierGroupvm = new MenuModifierGroupVM
        {
            menuModifierGroups = modifierGroups
        };

        return PartialView("_MenuModifierGroupPV", modifierGroupvm);
    }


    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult AddMenuCategory(MenuCategory category)
    {


        // Console.WriteLine("Hekrkjnhfrkjnfcekjnb" + categoryvm.editCategories.CategoryName);
        if (ModelState.IsValid)
        {
            _menuService.AddCategory(category);
            TempData["Message"] = "Category added successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Menu", "Home");
        }
        return RedirectToAction("Menu", "Home");
    }

    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult EditCategory(MenuCategoryVM model)
    {
        Console.WriteLine("Edit name:" + model.CategoryName);
        Console.WriteLine("Edit name:" + model.CategoryId);
        Console.WriteLine("Edit name:" + model.CategoryDescription);
        // if (ModelState.IsValid)
        // {
        //     _menuService.UpdateCategory(model);
        //     return RedirectToAction("Menu", "Home");
        // }

        // return Json(new { success = false, message = "Invalid data" });
        TempData["Message"] = "Category updated successfully!";
        TempData["MessageType"] = "info";
        _menuService.UpdateCategory(model);
        return RedirectToAction("Menu", "Home");
    }


    [Authorize(Policy = "MenuDeletePolicy")]
    [HttpPost]
    public IActionResult DeleteCategory(int id)
    {
        Console.WriteLine("tHIS IS iD: " + id);
        var category = _menuService.GetCategoryById(id);
        _menuService.DeleteCategory(category);
        TempData["Message"] = "Category deleted successfully!";
        TempData["MessageType"] = "success";
        return RedirectToAction("Menu", "Home");
    }


    [Authorize(Policy = "MenuDeletePolicy")]
    public IActionResult DeleteItem([FromBody] List<MenuItem> items)
    {

        Console.WriteLine("HEEJNJKFNJN");
        if (items == null || items.Count == 0)
        {
            return BadRequest("No items received");
        }

        Console.WriteLine("Updatedjfnldnfledf");
        Console.WriteLine(items.Count);

        _menuService.DeleteItem(items);

        TempData["Message"] = "Successfully Delete Item.";
        TempData["MessageType"] = "success"; // Types: success, error, warning, info



        return RedirectToAction("Menu", "Home");

    }


    [HttpPost]
    public IActionResult AddMenuItem([FromBody] JsonObject menuItemData)
    {
        if (menuItemData == null)
        {
            return BadRequest("Invalid JSON format. Could not parse data.");
        }

        Console.WriteLine("Raw JSON received: " + menuItemData.ToString());

        // Extract individual values safely
        string itemName = menuItemData["ItemName"]?.ToString();
        int categoryId = TryParseInt(menuItemData["CategoryId"]);
        int itemTypeId = TryParseInt(menuItemData["ItemtypeId"]);
        decimal rate = TryParseDecimal(menuItemData["Rate"]);
        int quantity = TryParseInt(menuItemData["Quantity"]);
        int unitId = TryParseInt(menuItemData["UnitId"]);
        bool isAvailable = TryParseBool(menuItemData["IsAvailable"]);
        decimal taxPercentage = TryParseDecimal(menuItemData["TaxPercentage"]);
        string shortCode = menuItemData["ShortCode"]?.ToString();
        string description = menuItemData["Description"]?.ToString();
        bool taxDefault = TryParseBool(menuItemData["TaxDefault"]);
        string ItemPhoto = menuItemData["ItemPhoto"]?.ToString();


        var base64String = ItemPhoto.Split(',')[1];

        // Convert base64 string to byte array
        byte[] imageBytes = Convert.FromBase64String(base64String);

        // Generate a unique file name for the image
        var uniqueFileName = Guid.NewGuid().ToString() + ".jpg"; // Or another image extension
        var filePath = Path.Combine(_imageFolderPath, uniqueFileName);

        // Ensure the directory exists
        Directory.CreateDirectory(_imageFolderPath);

        // Save the image to the server using FileStream
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            stream.Write(imageBytes, 0, imageBytes.Length);
        }

        // Store the relative image path (to store in database)
        ItemPhoto = "/images/" + uniqueFileName;
        // Parse Modifier Groups JSON array safely
        List<ItemModifierGroup> modifierGroups = new List<ItemModifierGroup>();
        if (menuItemData.ContainsKey("ModifierGroupIds") && menuItemData["ModifierGroupIds"] != null)
        {
            modifierGroups = JsonConvert.DeserializeObject<List<ItemModifierGroup>>(menuItemData["ModifierGroupIds"].ToString());
        }

        Console.WriteLine($"Received Item Name: {itemName}");
        Console.WriteLine($"Modifier Groups: {string.Join(",", modifierGroups.Select(m => m.ModifierGroupId))}");

        // Step 1: Save Menu Item
        var menuitem = new MenuItem
        {
            CategoryId = categoryId,
            ItemName = itemName,
            ItemtypeId = itemTypeId,
            Rate = rate,
            Quantity = quantity,
            UnitId = unitId,
            IsAvailable = isAvailable,
            TaxPercentage = taxPercentage,
            ShortCode = shortCode,
            Description = description,
            TaxDefault = taxDefault,
            CategoryPhoto = ItemPhoto
        };

        _menuService.AddMenuItem(menuitem);

        // Step 2: Save Modifier Groups (if present)
        if (modifierGroups.Any())
        {
            foreach (var modifierGroup in modifierGroups)
            {
                var menuitemmodifier = new ItemModifierGroup
                {
                    ItemId = menuitem.ItemId,
                    ModifierGroupId = modifierGroup.ModifierGroupId,
                    MinSelection = modifierGroup.MinSelection,
                    MaxSelection = modifierGroup.MaxSelection
                };

                _menuService.AddMenuItemModifierGroup(menuitemmodifier);
            }
        }

        return Json(new { success = true, message = "Menu Item Added Successfully!" });


    }

    private int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }

    private decimal TryParseDecimal(object value)
    {
        return decimal.TryParse(value?.ToString(), out decimal result) ? result : 0;
    }

    private bool TryParseBool(object value)
    {
        return bool.TryParse(value?.ToString(), out bool result) ? result : false;
    }

    [HttpGet]
    public IActionResult GetModifierGroup(int id)
    {
        Console.WriteLine($"Fetching Modifier Group for ID: {id}");

        var group = _menuService.GetModifierGroupById(id);
        if (group == null)
        {
            Console.WriteLine("❌ Modifier Group not found!");
            return NotFound("Modifier Group not found.");
        }

        var modifiers = _menuService.GetModifiersByModifierGroupId(id);

        Console.WriteLine($"✅ Found {modifiers.Count()} Modifiers in Group {id}");

        var viewModel = new MenuModifierGroupVM
        {
            ModifierGroupId = group.ModifierGroupId,
            ModifierGroupName = group.ModifierGroupName,
            ModifierGroupDecription = group.ModifierGroupDecription,
            Modifiers = modifiers.Select(m => new ModifierVM
            {
                ModifierId = m.ModifierId,
                ModifierName = m.ModifierName
            }).ToList()
        };

        return PartialView("_EditModifierGroupPV", viewModel); // Return Partial View

    }

    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult AddMenuModifierGroup([FromBody] MenuModifierGroupVM model)
    {
        Console.WriteLine("Received Data:");
        Console.WriteLine("Modifier Group Name: " + model.ModifierGroupName);
        Console.WriteLine("Modifier Group Description: " + model.ModifierGroupDecription);
        Console.WriteLine("Selected Modifier IDs: " + (model.ModifierIds != null ? string.Join(", ", model.ModifierIds) : "None"));

        // 1️⃣ Save Modifier Group
        var modifierGroup = new MenuModifierGroup
        {
            ModifierGroupName = model.ModifierGroupName,
            ModifierGroupDecription = model.ModifierGroupDecription
        };

        _menuService.AddModifierGroup(modifierGroup);  // Saving to database
        Console.WriteLine("Saved Modifier Group ID: " + modifierGroup.ModifierGroupId);

        // 2️⃣ Save Selected Modifiers into the Group

        // 2️⃣ Validate and Save Selected Modifiers


        // 2️⃣ Associate Combined Modifiers (Only this step is needed)
        if (model.ModifierIds != null && model.ModifierIds.Any())
        {
            foreach (var combinedModifierId in model.ModifierIds)
            {
                var existingModifier = _menuService.GetModifierById(combinedModifierId);
                if (existingModifier != null)
                {
                    var combinedModifier = new CombineModifier
                    {
                        ModifierGroupId = modifierGroup.ModifierGroupId, // Associate with group
                        ModifierId = combinedModifierId
                    };
                    _menuService.AddCombinedModifierGroup(combinedModifier);
                    Console.WriteLine($"Linked Combined Modifier ID {combinedModifierId} to Modifier Group ID {modifierGroup.ModifierGroupId}");
                }
                else
                {
                    Console.WriteLine($"Combined Modifier with ID {combinedModifierId} not found, skipping.");
                }
            }
        }
        // 3️⃣ Return Success Response
        var filterOptions = new UserFilterOptions
        {
            Page = 1,
            PageSize = 10 // Default values, adjust as needed
        };
        return MenuModifier(modifierGroup.ModifierGroupId, filterOptions);
    }

    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult EditModifierGroup([FromBody] MenuModifierGroupVM model)
    {
        if (model == null)
        {
            return Json(new { success = false, message = "Invalid data received." });
        }

        Console.WriteLine("Editing Modifier Group ID:" + model.ModifierGroupId);
        Console.WriteLine("New Name:" + model.ModifierGroupName);
        Console.WriteLine("New Description:" + model.ModifierGroupDecription);

        // if (!ModelState.IsValid)
        // {
        //     return Json(new { success = false, message = "Invalid data." });
        // }

        // Fetch existing Modifier Group from DB
        var existingGroup = _menuService.GetModifierGroupById(model.ModifierGroupId);
        if (existingGroup == null)
        {
            return Json(new { success = false, message = "Modifier Group not found." });
        }

        // Update basic details
        existingGroup.ModifierGroupName = model.ModifierGroupName;
        existingGroup.ModifierGroupDecription = model.ModifierGroupDecription;

        // Update Modifier Group associations
        var existingModifierIds = _menuService.GetModifiersByModifierGroupId(model.ModifierGroupId)
                                                .Select(m => m.ModifierId)
                                                .ToList();

        // Find modifiers to remove (Old ones that are not in the new list)
        var modifiersToRemove = existingModifierIds.Except(model.ModifierIds).ToList();
        foreach (var modifierId in modifiersToRemove)
        {
            _menuService.RemoveCombinedModifierGroup(modifierId, existingGroup.ModifierGroupId);
        }

        // Find modifiers to add (New ones that were not in the old list)
        var modifiersToAdd = model.ModifierIds.Except(existingModifierIds).ToList();
        foreach (var modifierId in modifiersToAdd)
        {
            var combinedModifier = new CombineModifier
            {
                ModifierId = modifierId,
                ModifierGroupId = model.ModifierGroupId
            };
            _menuService.AddCombinedModifierGroup(combinedModifier);
        }

        var modifierGroupVM = new MenuModifierGroupVM
        {
            ModifierGroupId = existingGroup.ModifierGroupId,
            ModifierGroupName = existingGroup.ModifierGroupName,
            ModifierGroupDecription = existingGroup.ModifierGroupDecription
        };
        _menuService.UpdateModifierGroup(modifierGroupVM);

        var filterOptions = new UserFilterOptions
        {
            Page = 1,
            PageSize = 10 // Default values, adjust as needed
        };
        return MenuModifier(modifierGroupVM.ModifierGroupId, filterOptions);

    }

    [Authorize(Policy = "MenuDeletePolicy")]
    [HttpPost]
    public IActionResult DeleteModifierGroup(int groupId)
    {
        Console.WriteLine("This is ID to delete: " + groupId);

        // Get all modifier groups sorted by ID
        var allGroups = _menuService.GetAllModifierGroups()
                                    .OrderBy(mg => mg.ModifierGroupId)
                                    .ToList();

        // Find index of the current group
        var currentIndex = allGroups.FindIndex(mg => mg.ModifierGroupId == groupId);

        // Find the previous group's ID (if available)
        int? previousGroupId = null;
        if (currentIndex > 0) // Ensure there's a previous group
        {
            previousGroupId = allGroups[currentIndex - 1].ModifierGroupId;
        }

        // Delete the current modifier group
        var modifierGroup = _menuService.GetModifierGroupById(groupId);
        if (modifierGroup != null)
        {
            _menuService.DeleteModifierGroup(modifierGroup);
        }

        // Set success message
        TempData["Message"] = "Modifier Group deleted successfully!";
        TempData["MessageType"] = "error";

        Console.WriteLine("Previous Group ID: " + (previousGroupId.HasValue ? previousGroupId.ToString() : "None"));

        // Redirect or return data
        var filterOptions = new UserFilterOptions
        {
            Page = 1,
            PageSize = 10 // Default values, adjust as needed
        };
        return MenuModifier((int)previousGroupId, filterOptions);// Pass previousGroupId to reload view with it
    }

    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult AddMenuModifier(MenuModifierGroupVM menuModifier)
    {
        Console.WriteLine(ModelState.IsValid);
        // Console.WriteLine("--------------Add Modifier" + menuModifier.ModifierGroupId);


        var units = _menuService.GetAllUnits(); // Fetch Units from the database
        var unitSelectList = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();
        ViewBag.Units = unitSelectList;

        var modifiergroups = _menuService.GetAllModifierGroups(); // Fetch Units from the database
        var modifiergroupSelectList = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();
        ViewBag.ModifierGroups = modifiergroupSelectList;


        var newmenumodifier = new MenuModifier
        {
            ModifierName = menuModifier.ModifierName,
            ModifierRate = menuModifier.ModifierRate,
            Quantity = menuModifier.Quantity,
            UnitId = menuModifier.UnitId,
            ModifierDecription = menuModifier.ModifierDecription

        };

        _menuService.AddModifier(newmenumodifier);

        // Add multiple Modifier Groups
        if (menuModifier.ModifierGroupIds != null && menuModifier.ModifierGroupIds.Any())
        {
            foreach (var groupId in menuModifier.ModifierGroupIds)
            {
                var combinedModifier = new CombineModifier
                {
                    ModifierId = newmenumodifier.ModifierId,
                    ModifierGroupId = groupId
                };
                _menuService.AddCombinedModifierGroup(combinedModifier);
            }
        }

        Console.WriteLine("--------------Add Modifier END");
        TempData["Message"] = "Modifier added successfully!";
        TempData["MessageType"] = "success";



        return Json(new { success = true, message = "Modifier Added Successfully!" });
    }

    public IActionResult EditMenuModifier(int modifierid)
    {
        Console.WriteLine(modifierid);

        // Fetch Units from the database
        var units = _menuService.GetAllUnits();
        ViewBag.Units = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();

        // Fetch Modifier Groups from the database
        var modifierGroups = _menuService.GetAllModifierGroups();
        ViewBag.ModifierGroups = modifierGroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();

        // Fetch the modifier item
        var modifier = _menuService.GetModifierById(modifierid);
        if (modifier == null)
        {
            return NotFound();
        }

        // Fetch associated modifier group IDs (multiple)
        var assignedModifierGroups = _menuService.GetModifierGroupsByModifierId(modifierid);

        var itemvm = new MenuModifierGroupVM
        {
            ModifierId = modifier.ModifierId,
            ModifierGroupIds = assignedModifierGroups.Select(mg => mg.ModifierGroupId).ToList(), // Multiple selection
            ModifierName = modifier.ModifierName,
            ModifierRate = modifier.ModifierRate,
            UnitId = modifier.UnitId,
            Quantity = modifier.Quantity,
            ModifierDecription = modifier.ModifierDecription,
            UnitName = modifier.UnitId.HasValue ? _menuService.GetUnitById(modifier.UnitId.Value) : "No Unit"
        };

        return PartialView("_EditModifierPV", itemvm);
    }

    [HttpPost]
    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult EditMenuModifier([FromBody] MenuModifierGroupVM menuModifier)
    {
        if (menuModifier == null)
        {
            return Json(new { success = false, message = "Invalid request data." });
        }

        Console.WriteLine(ModelState.IsValid);

        // Fetch the existing modifier
        var existingModifier = _menuService.GetModifierById(menuModifier.ModifierId);
        if (existingModifier == null)
        {
            return Json(new { success = false, message = "Modifier not found." });
        }

        // Update the modifier properties
        existingModifier.ModifierName = menuModifier.ModifierName;
        existingModifier.ModifierRate = menuModifier.ModifierRate;
        existingModifier.Quantity = menuModifier.Quantity;
        existingModifier.UnitId = menuModifier.UnitId;
        existingModifier.ModifierDecription = menuModifier.ModifierDecription;

        _menuService.UpdateModifier(existingModifier);

        // Fetch existing assigned modifier groups
        var existingModifierGroups = _menuService.GetModifierGroupsByModifierId(existingModifier.ModifierId)
                                                  .Select(mg => mg.ModifierGroupId)
                                                  .ToList();

        var newModifierGroups = menuModifier.ModifierGroupIds ?? new List<int>();

        // Find groups to remove (exist in old but not in new)
        var groupsToRemove = existingModifierGroups.Except(newModifierGroups).ToList();
        // Find groups to add (exist in new but not in old)
        var groupsToAdd = newModifierGroups.Except(existingModifierGroups).ToList();

        // Remove modifier groups that are no longer selected
        foreach (var groupId in groupsToRemove)
        {
            _menuService.RemoveCombinedModifierGroup(existingModifier.ModifierId, groupId);
        }

        // Add new modifier groups that were selected
        foreach (var groupId in groupsToAdd)
        {
            var combinedModifier = new CombineModifier
            {
                ModifierId = existingModifier.ModifierId,
                ModifierGroupId = groupId
            };
            _menuService.AddCombinedModifierGroup(combinedModifier);
        }

        Console.WriteLine("--------------Edit Modifier END");
        TempData["Message"] = "Modifier updated successfully!";
        TempData["MessageType"] = "success";

        return Json(new { success = true, message = "Modifier updated successfully!" });
    }

    public IActionResult DeleteModifier([FromBody] List<MenuModifier> modifiers)
    {
        Console.WriteLine("HEEJNJKFNJN");
        if (modifiers == null || modifiers.Count == 0)
        {
            return BadRequest("No modifiers received");
        }

        Console.WriteLine("Updatedjfnldnfledf");
        Console.WriteLine(modifiers.Count);

        _menuService.DeleteModifiers(modifiers);

        TempData["Message"] = "Successfully Delete Item.";
        TempData["MessageType"] = "success"; // Types: success, error, warning, info



        return RedirectToAction("Menu", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}