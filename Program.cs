using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Connection String
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add EmailService to dependency injection
builder.Services.AddTransient<EmailService>();

// CancelExpiredReservations service
builder.Services.AddScoped<CancelExpiredReservations>();

// background service to cancel expired reservations
builder.Services.AddHostedService<ReservationExpiryService>();

builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make the session cookie essential
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

//app.MapControllerRoute(
//    name: "staff",
//    pattern: "{controller=StaffReservation}/{action=Dashboard}/{id?}");

//app.MapControllerRoute(
//    name: "admin",
//    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "login",
//    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
