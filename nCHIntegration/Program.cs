using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nCHIntegration.Data;
using nCHIntegration.Models;
using nCHIntegration.Services;
using System;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppHODBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HOConnectionStringLocal")));

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 🛑 FIX: กำหนดให้ Serializer ใช้ชื่อคุณสมบัติตามที่ประกาศใน C# (PascalCase) 
        //         แทนที่จะใช้ camelCase ตามมาตรฐาน JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

//builder.Services.AddDbContext<AppCRADbContext>(options =>
//    options.UseMySql(builder.Configuration.GetConnectionString("CRAConnectionString"),
//        new MySqlServerVersion(new Version(8, 0, 21)))); // Adjust version as per your MySQL server.

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

await SeedService.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // This MUST be the first thing in the pipeline for Development to catch all errors
    app.UseDeveloperExceptionPage();
}
else // This is the PRODUCTION/Staging/etc. block (when IsDevelopment() is false)
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
