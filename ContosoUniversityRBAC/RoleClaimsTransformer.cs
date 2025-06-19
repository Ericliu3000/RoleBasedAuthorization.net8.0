//using ContosoUniversityRBAC.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ContosoUniversityRBAC
{
    public class RoleClaimsTransformer : IClaimsTransformation
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly RoleManager<MyRole> _roleManager;
        private readonly ILogger<RoleClaimsTransformer> _logger;

        public RoleClaimsTransformer(UserManager<MyUser> userManager, RoleManager<MyRole> roleManager, ILogger<RoleClaimsTransformer> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        { 
            // 仅处理已认证的用户
            if (principal.Identity?.IsAuthenticated != true)
                return principal;

            // 获取用户ID
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return principal;

            // 获取用户
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return principal;

            // 创建新的身份
            var identity = new ClaimsIdentity();

            // 添加用户角色作为Claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                //将角色也添加到Claims 
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));

                // 如果需要角色的Claims（例如角色权限）
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                   
                 
                    identity.AddClaims(roleClaims);
                }
            }

  
         
            // 添加用户的自定义Claims（仅添加不存在的声明）
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in userClaims)
            {
                // 检查是否已存在相同类型和值的声明
                if (!principal.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    identity.AddClaim(claim);
                }
            }

            // 将新身份添加到主体
            principal.AddIdentity(identity);
            return principal;
        }
    }
}
