using ContosoUniversityRBAC.Data;
using ContosoUniversityRBAC.Models;
using ContosoUniversityRBAC.Models.UserViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Sang.AspNetCore.RoleBasedAuthorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContosoUniversityRBAC.Controllers
{
    public class UsersController : Controller
    {
        private readonly MyDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly RoleManager<MyRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(MyDbContext context, UserManager<MyUser> userManager, RoleManager<MyRole> roleManager, ILogger<UsersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;

        }

        // GET: Users
        [Resource("UsersRead")]
        public async Task<IActionResult> Index(int? pageNumber,string? SelectedRoleId,string? Search,string? sortOrder)
        {

            IdentityInitializer a = new IdentityInitializer(_roleManager, _userManager);

            await a.Initialize();
            ViewData["CurrentSort"] = sortOrder;
            ViewData["SortOrder"] = String.IsNullOrEmpty(sortOrder) ? "username_desc" : "";
            ViewData["Search"] = Search;
            ViewData["SelectedRoleId"] = SelectedRoleId;
            var rolesQuery = from d in _roleManager.Roles
                             orderby d.Name, d.Id
                             select d;

          
            ViewBag.Roles = new SelectList(rolesQuery.AsNoTracking(), "Id", "Name", SelectedRoleId);
            IQueryable<MyUser> userQuery;

            if (SelectedRoleId != null)
            {
                // 通过角色ID获取角色名称
                var role = await _roleManager.FindByIdAsync(SelectedRoleId);
                if (role != null)
                {
                    // 注意：这里不能直接将IList转为IQueryable，需要从数据库重新查询
                    //userQuery = _userManager.Users
                    //           .Where(u => u.Roles.Any(r => r.RoleId == SelectedRoleId));
                    var userIdsInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                    var userIds = userIdsInRole.Select(u => u.Id).ToList();

                    // 从数据库查询这些用户
                    userQuery = _userManager.Users
                        .Where(u => userIds.Contains(u.Id));
                    
                    
                                                  
                }
                else
                {
                    // 角色不存在，返回空查询
                    userQuery = Enumerable.Empty<MyUser>().AsQueryable();
                }
            }
            else
            {
                // 如果没有选择角色，返回所有用户
                userQuery = _userManager.Users;
                 
            }

 
            // return View(await courses.ToListAsync());
            if (!string.IsNullOrEmpty(Search))
            {

                userQuery = userQuery.Where(user => EF.Functions.Like(user.UserName, $"%{Search}%") || EF.Functions.Like(user.Email, $"%{Search}%"));
                                         
            }
            switch (sortOrder)
            {
                case "username_desc":
                    userQuery = userQuery.OrderByDescending(s => s.UserName);
                    break;

                default:
                    userQuery = userQuery.OrderBy(s => s.UserName);
                    break;
            }


            int pageSize = 5;
            return View(await PaginatedList<IdentityUser>.CreateAsync(
                userQuery.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Users/Details/5
        [Resource("UsersRead")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

          
            MyUser myuser = await _userManager.FindByIdAsync(id);

            if (myuser == null)
            {
                return NotFound();
            }
            else
            {
                UserEditInputModel model = new UserEditInputModel(myuser);
                return View(model);
            }
                
        }

        // GET: Users/Create
        [Resource("UsersDelete")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Resource("UsersDelete")]
        public async Task<IActionResult> Create( UserCreateInputModel userCreateInputModel)
        {
            
            if (ModelState.IsValid)
            {
                 
                try
                {
                    MyUser user = userCreateInputModel.ToMyUser(_userManager);
                    IdentityResult result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", $"Can not create the user. Reason: {result.Errors.FirstOrDefault().Description}");
                       //ViewData["ConfirmationMessage"] = $"Can not create the user. Reason: {result.Errors.FirstOrDefault().Description}";
                     
                        return View(userCreateInputModel);
                    }


                    ViewData["ConfirmationMessage"] = "User was Created Successfully!";
               
                    return RedirectToAction(nameof(Index));
                }
                 catch (DbUpdateException ex)
                {
                    //Log the error (uncomment ex variable name and write a log.
            
                    ModelState.AddModelError("",  ex.Message);
                }
           
            }
            return View(userCreateInputModel);
        }

        // GET: Users/Edit/5
        [Resource("UsersWrite")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MyUser myuser = await _userManager.FindByIdAsync(id.ToString());
            if (myuser == null)
            {
                return NotFound();
            }
            UserEditInputModel input = new UserEditInputModel(myuser);
            ViewData["Title"] = myuser.UserName;
            await PopulateAssignedRoleDataAsync(myuser);
            await PopulateAssignedClaimDataAsync(myuser);
            return View(input);
        }
        [Resource("UsersWrite")]
        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(   UserEditInputModel Input , String[] selectedRoles, string[] selectedClaims)
        {
            
            if (ModelState.IsValid)
            {
                MyUser myuser = await _userManager.FindByIdAsync(Input.Id);
                if (myuser == null)
                {
                    ModelState.AddModelError("Error", $"Can not find the user. UserName: {Input.UserName}");
        
                }
                else
                {
                    //Input.CopyToMyUser(_userManager, myuser);
                    myuser.UserName = Input.UserName;
                    myuser.Email = Input.Email;
                    myuser.LockoutEnd = Input.LockoutEnd;
                }
                try
                {

                    IdentityResult result = await _userManager.UpdateAsync(myuser);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", $"Can not create the user. Reason: {result.Errors.FirstOrDefault().Description}");
                       

                        return View(Input);
                    }
                    else
                    {
                        await UpdateUserRolesAsync(selectedRoles, myuser);
                        await UpdateUserClaimsAsync(selectedClaims, myuser);

                        ViewData["ConfirmationMessage"] = "User Edit Successfully";

                        return RedirectToAction(nameof(Index));
                    }

                }

                catch (DbUpdateException ex)
                {

                     
                    //Log the error (uncomment ex variable name and write a log.
                    ModelState.AddModelError("",  ex.Message);
                }

                return View(Input);
            }
            ViewData["ConfirmationMessage"] = "Validate Error!!!";
            return View(Input);
        }

        // GET: Users/Delete/5
        [Resource("UsersDelete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }


            MyUser myuser = await _userManager.FindByIdAsync(id);
            if (myuser == null)
            {
                return NotFound();
            }
            else
            {
                UserEditInputModel model = new UserEditInputModel(myuser);
                return View(model);
            }
              
        }
        [Resource("UsersDelete")]

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            
            MyUser myuser = await _userManager.FindByIdAsync(id);
            if (myuser != null)
            {
                // _context.UserCreateInputModel.Remove(userCreateInputModel);
                try
                {
                    IdentityResult result = await _userManager.DeleteAsync(myuser);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", $"Can not delete the user. Reason: {result.Errors.FirstOrDefault().Description}");
                        // ViewData["ConfirmationMessage"] = $"Can not create the user. Reason: {result.Errors.FirstOrDefault().Description}";
                       

                        return View();
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Can not delete the user. Reason:{ex.Message}");
                    // ViewData["ConfirmationMessage"] = $"Can not create the user. Reason: {result.Errors.FirstOrDefault().Description}";
                   
                }

            }

            
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ResetPassword(String? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MyUser myuser = await _userManager.FindByIdAsync(id.ToString());
            if (myuser == null)
            {
                return NotFound();
            }
            UserResetPasswordInputModel input = UserResetPasswordInputModel.FromMyUser(myuser);

            ViewData["Title"] = myuser.UserName;

            return View(input);
        }
        [HttpPost, ActionName("ResetPassword")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ResetPasswordPost(UserResetPasswordInputModel Input)
        {
            if (ModelState.IsValid)
            {
                MyUser myuser = await _userManager.FindByIdAsync(Input.Id);
                if (myuser == null)
                {
                    ModelState.AddModelError("Error", $"Can not find the user. UserName: {Input.UserName}");
                     
                }
                else
                {
                    //Input.CopyToMyUser(_userManager, myuser);
                    myuser.UserName = Input.UserName;
                     
                    if (Input.Password is not null and not "")
                    {
                        myuser.PasswordHash = _userManager.PasswordHasher.HashPassword(myuser, Input.Password);
                    }
                }
                try
                {

                    IdentityResult result = await _userManager.UpdateAsync(myuser);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", $"Can not update the user. Reason: {result.Errors.FirstOrDefault().Description}");
                        

                        return View(Input);
                    }
                    else
                    {


                        ViewData["ConfirmationMessage"] = "User Edit Successfully";

                        return RedirectToAction(nameof(Index));
                    }

                }

                catch (DbUpdateException ex)
                {
                       //Log the error (uncomment ex variable name and write a log.
                    ModelState.AddModelError("", ex.Message);
                }

                return View(Input);
            }
            ViewData["ConfirmationMessage"] = "Validate Error!!!";
            return View(Input);
        }
        private bool UserCreateInputModelExists(string id)
        {
            return _context.UserCreateInputModel.Any(e => e.Id == id);
        }
        private async Task PopulateAssignedRoleDataAsync(MyUser myuser)
        {
            var allRoles = await _roleManager.Roles.OrderBy(s => s.Name).ToArrayAsync();
            var userRoles = await _userManager.GetRolesAsync(myuser);
            var viewModel = new List<AssignRoleData>();
            foreach (var role in allRoles)
            {
                viewModel.Add(new AssignRoleData
                {
                    RoleId = role.Id.ToString(),
                    RoleName = role.Name,
                    Assigned = userRoles.Contains(role.Name)
                });
            }



            ViewData["Roles"] = viewModel;
        }
        private async Task UpdateUserRolesAsync(string[] selectedRoles, MyUser userToUpdate)
        {
            if (selectedRoles == null)
            {
                // userToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            var selectedRolesHS = new HashSet<string>(selectedRoles);
            var userRoles = new HashSet<string>
                (await _userManager.GetRolesAsync(userToUpdate));
            var allRoles = await _roleManager.Roles.ToArrayAsync();
 
            foreach (var role in allRoles)
            {
                if (selectedRolesHS.Contains(role.Id.ToString()))
                {
                    if (!userRoles.Contains(role.Name))
                    {
                        await _userManager.AddToRoleAsync(userToUpdate, role.Name);

                    }
                }
                else
                {

                    if (userRoles.Contains(role.Name))
                    {
                        
                        await _userManager.RemoveFromRoleAsync(userToUpdate, role.Name);
                    }
 
                }
            }
        }
        private async Task PopulateAssignedClaimDataAsync(MyUser myuser)
        {
            var allResources = ResourceData.Resources.OrderBy(s=>s.Key);
            var userClaims = await _userManager.GetClaimsAsync(myuser);
            var allClaims = new List<Claim>();
            var viewModel = new List<AssignClaimData>();
            foreach (var resource in allResources)
            {
                allClaims.Add(new Claim(ResourceClaimTypes.Permission, resource.Key));

            }



            foreach (var claim in allClaims)
            {
                viewModel.Add(new AssignClaimData
                {
                    ClaimType = Sang.AspNetCore.RoleBasedAuthorization.ResourceClaimTypes.Permission,
                    ClaimValue = claim.Value,
                    Assigned = (userClaims.Any(userClaim => userClaim.Type == claim.Type && userClaim.Value == claim.Value))

                });
                

            }

 


            ViewData["Claims"] = viewModel;
        }

        private async Task UpdateUserClaimsAsync(string[] selectedClaims, MyUser userToUpdate)
        {
            if (selectedClaims == null)
            {
                // userToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }
            var allResources = ResourceData.Resources;
            var selectedClaimHS = new HashSet<string>(selectedClaims);
            var userClaims = await _userManager.GetClaimsAsync(userToUpdate);
            var userClaimsList = userClaims
                    .Where(claim => claim.Type.Equals(ResourceClaimTypes.Permission, StringComparison.Ordinal))
                     .Select(claim => claim.Value)
                    .ToList();

            foreach (var resource in allResources)
            {

                if (selectedClaimHS.Contains(resource.Key.ToString()))
                {
                    if (!userClaimsList.Contains(resource.Key.ToString()))
                    {
                        await _userManager.AddClaimAsync(userToUpdate, new Claim(ResourceClaimTypes.Permission, resource.Key));

                    }
                }
                else
                {

                    if (userClaimsList.Contains(resource.Key.ToString()))
                    {

                        await _userManager.RemoveClaimAsync(userToUpdate, new Claim(ResourceClaimTypes.Permission, resource.Key));
                    }
 
                }
            }
        }
    }
}
