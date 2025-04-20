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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Business.Mapping;
using Infrastructure.Repositories.MailRepository;

var builder = WebApplication.CreateBuilder(args);
MappingConfig.Configure();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddOptions();

// 🔔 Toast Mesajları (Notyf)
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});

// 🌐 Kültür Ayarları
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "tr-TR", "en-US" }
        .Select(culture => new CultureInfo(culture)).ToArray();

    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// 🌐 Google Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = "1032155090023-gtort1shrfp7fkrfl6n0j83lvqu2kf1c.apps.googleusercontent.com"; // <-- Buraya kendi ID'ni yaz
        options.ClientSecret = "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"; // <-- Buraya kendi Secret'ını yaz
        options.CallbackPath = "/signin-google"; // Google Console’da bu path tanımlı olmalı

    });

// 💉 Bağımlılıkları yükle
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddBusinessServices();
MappingProfile.ConfigureMappings();

var app = builder.Build();

// 💾 Fake Data (Opsiyonel)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await FakeDataGenerator.SeedAsync(context);
}

// 🔒 Hata yönetimi ve güvenlik
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

// 🔐 Kimlik Doğrulama
app.UseAuthentication(); // Google Login için şart!
app.UseAuthorization();
app.UseSession(); // burada
// 🌍 Localization
var locOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

// 📍 Route Ayarları
app.UseEndpoints(endpoints =>
{
    // ✅ Admin Area
    endpoints.MapAreaControllerRoute(
        name: "admin",
        areaName: "Admin",
        pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}"
    );

    // ✅ User Area
    endpoints.MapAreaControllerRoute(
        name: "user",
        areaName: "User",
        pattern: "User/{controller=Home}/{action=Index}/{id?}"
    );

    // ✅ Post Detay
    endpoints.MapAreaControllerRoute(
        name: "userPostDetails",
        areaName: "User",
        pattern: "User/Home/Details/{slug}",
        defaults: new { controller = "Home", action = "Details" }
    );

    // ✅ Default Route → User alanı
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}",
        defaults: new { area = "User" }
    );
    endpoints.MapControllerRoute(
          name: "login",
          pattern: "giris-yap", // URL'deki "Giris"
          defaults: new { controller = "Account", action = "Login" });

    endpoints.MapControllerRoute(
        name: "register",
        pattern: "kayıt-ol", // URL'deki "Giris"
        defaults: new { controller = "Account", action = "Login" });
});



app.Run();
