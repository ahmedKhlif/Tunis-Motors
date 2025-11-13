using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TP2.Models;
using TP2.Models.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContextPool<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDBConnection"
 )));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 6;

    // Enable email confirmation
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register payment and email services
builder.Services.AddScoped<TP2.Services.IPaymentService, TP2.Services.StripePaymentService>();
builder.Services.AddScoped<TP2.Services.IEmailService, TP2.Services.EmailService>();

var app = builder.Build();

// Seed roles and admin user after app is built
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedRolesAndAdmin(services);
        await SeedCategories(services);
    }
    catch (Exception ex)
    {
        // Log the error but don't crash the app
        Console.WriteLine($"Seeding failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Create roles
    string[] roles = { "Admin", "Manager", "Seller", "Buyer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Create admin user
    var adminEmail = "admin@carecommerce.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Create manager user
    var managerEmail = "manager@carecommerce.com";
    var managerUser = await userManager.FindByEmailAsync(managerEmail);
    if (managerUser == null)
    {
        managerUser = new IdentityUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(managerUser, "Manager123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(managerUser, "Manager");
        }
    }

    // Create seller user
    var sellerEmail = "seller@carecommerce.com";
    var sellerUser = await userManager.FindByEmailAsync(sellerEmail);
    if (sellerUser == null)
    {
        sellerUser = new IdentityUser
        {
            UserName = sellerEmail,
            Email = sellerEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(sellerUser, "Seller123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(sellerUser, "Seller");
        }
    }

    // Create buyer user
    var buyerEmail = "buyer@carecommerce.com";
    var buyerUser = await userManager.FindByEmailAsync(buyerEmail);
    if (buyerUser == null)
    {
        buyerUser = new IdentityUser
        {
            UserName = buyerEmail,
            Email = buyerEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(buyerUser, "Buyer123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(buyerUser, "Buyer");
        }
    }
}

async Task SeedCategories(IServiceProvider serviceProvider)
{
    var context = serviceProvider.GetRequiredService<AppDbContext>();

    if (!context.Categories.Any())
    {
        var categories = new[]
        {
            new Category { CategoryName = "Sedans" },
            new Category { CategoryName = "SUVs" },
            new Category { CategoryName = "Trucks" },
            new Category { CategoryName = "Sports Cars" },
            new Category { CategoryName = "Luxury Cars" },
            new Category { CategoryName = "Electric Vehicles" },
            new Category { CategoryName = "Hybrid Cars" },
            new Category { CategoryName = "Motorcycles" },
            new Category { CategoryName = "Vans" },
            new Category { CategoryName = "Commercial Vehicles" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
