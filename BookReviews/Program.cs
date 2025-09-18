using BookReviews.Data;
using BookReviews.Models.Dao;
using BookReviews.Repositories;
using BookReviews.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        
    })
    .AddRoles<IdentityRole>()                
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BookReviews API", Version = "v1" });
});

builder.Services.AddScoped<IRepository, EfRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookReviews API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    const string adminRole = "Admin";
    var adminEmail = builder.Configuration["Admin:Email"] ?? "admin@demo.local";
    var adminPassword = builder.Configuration["Admin:Password"] ?? "Admin#12345!";

    if (!await roleMgr.RoleExistsAsync(adminRole))
        await roleMgr.CreateAsync(new IdentityRole(adminRole));

    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new User
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true
        };
        var create = await userMgr.CreateAsync(admin, adminPassword);
        if (!create.Succeeded)
            throw new Exception("Failed to create seeded admin: " +
                string.Join(", ", create.Errors.Select(e => e.Description)));
    }

    if (!await userMgr.IsInRoleAsync(admin, adminRole))
        await userMgr.AddToRoleAsync(admin, adminRole);
}

app.Run();
