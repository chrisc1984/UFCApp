using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UFCApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UFCApp.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Load user secrets only in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'UFCAppDBContextConnection' not found.");

builder.Services.AddDbContext<UFCAppDBContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<UFCAppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<UFCAppDBContext>();

builder.Services.AddAuthentication()
    .AddCookie();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();  

app.Run();
