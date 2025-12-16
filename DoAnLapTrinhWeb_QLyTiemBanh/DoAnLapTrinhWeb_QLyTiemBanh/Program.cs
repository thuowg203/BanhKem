using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Identity;
using DoAnLapTrinhWeb_QLyTiemBanh.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================== SESSION ======================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ====================== DB CONTEXT ======================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ====================== IDENTITY ======================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ====================== AUTHENTICATION ======================
// ⚡ Cookie vẫn là mặc định (để LoginPartial không bị ảnh hưởng)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    })
    // 🧩 JWT chỉ dùng cho API
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };

        // ⚙️ Ngăn redirect về /Account/Login khi thiếu token (cho Flutter)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Unauthorized or token missing\"}");
            }
        };
    });

// ====================== AUTHORIZATION ======================
builder.Services.AddAuthorization();

// ====================== SERVICES ======================
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddRazorPages();

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<CancelExpiredOrdersService>();

// ====================== APP PIPELINE ======================
var app = builder.Build();
app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // ✅ Cookie + JWT cùng hoạt động
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

// 🌐 MVC routes
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<DoAnLapTrinhWeb_QLyTiemBanh.Hubs.ChatHub>("/chathub");

// ====================== SEED DATA ======================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateGuestUserIfNotExists(services);
    await CreateAdminUserIfNotExists(services);
}

app.Run();

// ====================== SEED USERS ======================
async Task CreateGuestUserIfNotExists(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string guestRole = "Customer";
    if (!await roleManager.RoleExistsAsync(guestRole))
        await roleManager.CreateAsync(new IdentityRole(guestRole));

    var guestUser = await userManager.FindByNameAsync("guest");
    if (guestUser == null)
    {
        guestUser = new ApplicationUser
        {
            UserName = "guest",
            Email = "guest@tiembanh.local",
            FullName = "Khách lẻ",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(guestUser, "KhachLe@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(guestUser, guestRole);
    }
}

async Task CreateAdminUserIfNotExists(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@tiembanh.local",
            Email = "admin@tiembanh.local",
            FullName = "Quản trị viên",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, adminRole);
    }
}
