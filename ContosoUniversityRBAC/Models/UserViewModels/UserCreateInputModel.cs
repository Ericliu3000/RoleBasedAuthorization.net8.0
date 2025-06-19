using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversityRBAC.Models.UserViewModels;
public class UserCreateInputModel
{
    public string Id { get; set; }

    [Required(ErrorMessage = "Name is required"),
     MaxLength(255, ErrorMessage = "Name is too long"),
     Display(Name = "Name")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email is required"),
     MaxLength(255, ErrorMessage = "Email is too long"),
     EmailAddress(ErrorMessage = "Invalid email address"),
     Display(Name = "Email")]
    public string Email { get; set; }

    
    [Required(ErrorMessage = "Password is required"),
      //RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{6,15}$",
      //                 ErrorMessage = "Password must be 6-15 characters, containing at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 symbol"),
      RegularExpression(".{6,15}$", ErrorMessage = "Password must be 6-15 characters"),
      Display(Name = "Password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required"),
     Compare(nameof(Password), ErrorMessage = "The two passwords do not match"),
     Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }

    
    [Display(Name = "LockoutEnd")]
    public DateTimeOffset? LockoutEnd { get; set; }

    public MyUser ToMyUser(UserManager<MyUser> userManager)
    {
        MyUser user = new()
        {
            UserName = UserName,
            Email = Email,
            NormalizedUserName = UserName.ToUpperInvariant(), // 修正：使用UserName规范化
            EmailConfirmed = true,
            LockoutEnabled = true
            //CreatedAt = DateTime.UtcNow // 添加创建时间
        };

        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, Password);



        return user;
    }
}