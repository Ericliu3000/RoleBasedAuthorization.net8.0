using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sang.AspNetCore.RoleBasedAuthorization;
using System.Security.Claims;
using static System.Formats.Asn1.AsnWriter;

namespace ContosoUniversityRBAC.Data
{
    public class IdentityInitializer
    {
        private readonly RoleManager<MyRole> _roleManager;
        private readonly UserManager<MyUser> _userManager;
        
        public IdentityInitializer(RoleManager<MyRole> roleManager, UserManager<MyUser> userManager )
        { 
            _roleManager = roleManager;
            _userManager = userManager;
           
            //private Logger logger = LogManager.GetLogger(typeof(IdentityInitializer));
            

        }

   

        public async Task<bool> Initialize()
        {
            
            bool ret = true;
            if (!_userManager.Users.Any())
            {
                MyUser myuser;
                myuser = new MyUser { UserName = $"admin", Email = $"admin@contoso.com" };
                var result = await _userManager.CreateAsync(myuser, "123456");
             
                for (int i = 0; i < 10; i++) {
                    myuser = new MyUser { UserName = $"Test{i}", Email = $"Test{i}@contoso.com" };
                     result = await _userManager.CreateAsync(myuser, "123456");
                    if (!result.Succeeded) 
                    {
                        ret = false;
                        break;
                         
                    }


                }
         


            }
            if (!_roleManager.Roles.Any(r => r.Name == ResourceRole.Administrator))
            {
                MyRole role = new MyRole { Name = ResourceRole.Administrator };
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    ret = false;
                }
                else
                {
                    MyUser adminuser=await _userManager.FindByNameAsync("admin");
                    result = await _userManager.AddToRoleAsync(adminuser, ResourceRole.Administrator);
                }
            }
            

            foreach( var resource in  ResourceData.Resources)
            {

            MyRole role = new MyRole { Name = $"Role{resource.Key}" };
                
                
                if (!_roleManager.Roles.Any(r => r.Name == role.Name))
                {
                    var res = await _roleManager.CreateAsync(role);
                    if (!res.Succeeded)
                    {  ret = false;
            
                        break; 
                    }
                    else
                    {
                        await _roleManager.AddClaimAsync(role, new Claim(ResourceClaimTypes.Permission, resource.Key));
                    }

                }
            }

            
            return ret;
        }

    }
}
