using ContosoUniversityRBAC;
//using ContosoUniversityRBAC.Areas.Identity.Data;
using ContosoUniversityRBAC.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sang.AspNetCore.RoleBasedAuthorization;
var builder = WebApplication.CreateBuilder(args);

/*
 // 添加配置验证
builder.Services.AddOptions<MyOptions>()
    .Bind(builder.Configuration.GetSection("MyOptions"))
    .ValidateDataAnnotations();
*/
// 注册过滤器和日志
builder.Services.AddScoped<LogModelStateFilter>();

builder.Logging.ClearProviders();  
builder.Logging.AddLog4Net();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogModelStateFilter>();
});
/*
builder.Services.AddRazorPages(options => {
    // 文件夹级别授权
   // options.Conventions.AuthorizeFolder("/AdminPages", "RequireRoleAdmin");

    // 单页面授权
    options.Conventions.AuthorizePage("/Identity/Account/Register", "RequireRoleAdmin");

    // 允许匿名访问特定页面
    //options.Conventions.AllowAnonymousToPage("/PublicPage");
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireRoleAdmin",
        policyBuilder => policyBuilder.RequireRole("RoleAdmin"));
});
*/
/*
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Razor Pages 授权配置
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Identity/Account/Register", "require_role_admin");
});

// 授权策略配置
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("require_role_admin",
        policyBuilder => policyBuilder.RequireRole("RoleAdmin"));
});
*/
builder.Services.AddControllersWithViews();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ContosoUniversityRBAC.Data.MyDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddDefaultIdentity<MyUser>(opt =>
    {
        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequiredLength = 6;

        opt.SignIn.RequireConfirmedAccount = false;


        opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    }
    )
    .AddRoles<MyRole>()
    .AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders()
    .AddUserManager<UserManager<MyUser>>()
    .AddRoleManager<RoleManager<MyRole>>();
/*
IdentityBuilder iBuilder = new IdentityBuilder(typeof(MyUser), typeof(MyRole), builder.Services);

iBuilder.AddEntityFrameworkStores<MyDbContext>()
        .AddDefaultTokenProviders()
        .AddUserManager<UserManager<MyUser>>()
         .AddRoleManager<RoleManager<MyRole>>();
*/
builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformer>();
builder.Services.AddSangRoleBasedAuthorization();

builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();
 



 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MyDbContext>();
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
       

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        var logger1 = services.GetRequiredService<ILogger<Program>>();

        logger1.LogError(ex, "An error occurred while seeding the database.");
    }
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization( );


// 单独配置注册路由的权限

app.MapGet("/Identity/Account/Register", [Authorize(Roles = "RoleAdmin")] () => Results.Redirect("/users/create"));
 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
