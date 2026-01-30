using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.IdenitiyModels;
using OnlineTheatre.Repository;
using OnlineTheatre.Repository.Implementation;
using OnlineTheatre.Repository.Interface;
using OnlineTheatre.Service.Implementation;
using OnlineTheatre.Service.Integration;
using OnlineTheatre.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()          
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// Repository (Generic - register once for ALL entities)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IUserRepository), typeof(UserRepository));

// Your Services (Transient, per slides)
builder.Services.AddTransient<IShowService, ShowService>();
builder.Services.AddTransient<ITicketService, TicketService>();
builder.Services.AddTransient<IShoppingCartService, ShoppingCartService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddHttpClient<ITicketmasterService, TicketmasterService>();
builder.Services.AddHttpClient<ISeatGeekService, SeatGeekService>();




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
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
//app.UseAuthentication();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();


async Task SeedAdminAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1) Ensure role exists
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // 2) Ensure admin user exists
    var adminEmail = "admin@theatre.com";
    var adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(adminUser, adminPassword);

        if (!created.Succeeded)
            throw new Exception("Admin user create failed: " +
                string.Join(", ", created.Errors.Select(e => e.Description)));
    }

    // 3) Put user in Admin role
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        await userManager.AddToRoleAsync(adminUser, "Admin");
}

// ✅ Call it before app.Run()
await SeedAdminAsync(app.Services);


app.Run();

// FIXED Seed (uses Titile correctly)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Shows.Any())
    {
        var shows = new List<Show>
        {
            new Show { Titile = "Hamlet", StartTime = DateTime.Now.AddDays(10), BasePrice = 50m },
            new Show { Titile = "Romeo & Juliet", StartTime = DateTime.Now.AddDays(15), BasePrice = 45m },
            new Show { Titile = "Macbeth", StartTime = DateTime.Now.AddDays(20), BasePrice = 60m }
        };

        context.Shows.AddRange(shows);
        context.SaveChanges();

        // Generate tickets for seeded shows!
        var showService = scope.ServiceProvider.GetRequiredService<IShowService>();
        foreach (var show in shows)
        {
            showService.Insert(show);  // Triggers GenerateTicketsForShow
        }

        Console.WriteLine("✅ Shows + 150 tickets seeded!");
    }
}