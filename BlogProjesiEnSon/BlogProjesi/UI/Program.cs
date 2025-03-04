using Infrastructure.Extensions;
using Business.Extentions;
using Infrastructure.AppContext;
using Infrastructure.Seeds;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Domain.Entites;
using Microsoft.AspNetCore.Identity;
using UI.Areas.Admin;
using Mapster;
using AspNetCoreHero.ToastNotification;
using Infrastructure.DataAccess.EntityFramework;
using Infrastructure.DataAccess.Interface;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddBusinessServices();
MappingProfile.ConfigureMappings();


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "tr-TR", "en-US" }
                            .Select(culture => new System.Globalization.CultureInfo(culture))
                            .ToArray(); // CultureInfo dizisine dönüştürülüyor

    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await FakeDataGenerator.SeedAsync(context);
}

//builder.Services.AddAuthentication("CookieAuth")
//        .AddCookie("CookieAuth", options =>
//        {
//            options.LoginPath = "/Account/Login";
//        });

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Role", "Admin"));
//});
// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = new { Message = "Bir hata oluştu, lütfen daha sonra tekrar deneyin." };
        await context.Response.WriteAsJsonAsync(error);
    });
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseStaticFiles();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//        name: "areas",
//        pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
//    );

//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}"
//    );
//});
//app.UseEndpoints(endpoints =>
//{
//    // Admin area'ya varsayılan yönlendirme
//    endpoints.MapControllerRoute(
//        name: "admin",
//        pattern: "/",
//        defaults: new { area = "Admin", controller = "Admin", action = "Index" }
//    );

//    // Area-based routing
//    endpoints.MapControllerRoute(
//        name: "areas",
//        pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
//    );

//    // Default routing
//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}"
//    );
//});
app.UseEndpoints(endpoints =>
{
    // User alanı için varsayılan yönlendirme: /User/Home/Index
    endpoints.MapControllerRoute(
        name: "userHome",
        pattern: "User/{controller=Home}/{action=Index}/{id?}",
        defaults: new { area = "User" }
    );

    // Details sayfası: /User/Home/Details/{slug}
    endpoints.MapControllerRoute(
        name: "postDetails",
        pattern: "User/{controller=Home}/{action=Details}/{slug}",
        defaults: new { area = "User" }
    );

    // Diğer alan yönlendirmeleri...
    endpoints.MapControllerRoute(
        name: "areaRoute",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    // Varsayılan yönlendirme (diğer istekler için)
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});






app.Run();
