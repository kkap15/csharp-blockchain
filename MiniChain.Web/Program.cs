using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniChain.Web.Services;
using MiniChain.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<ChainDbContext>(
    options => options.UseSqlite("Data Source=minichain.db"));
builder.Services.AddScoped<ChainRepository>();
builder.Services.AddSingleton<ChainService>();
builder.Services.AddSingleton<WalletService>();
builder.Services.AddHostedService(p => p.GetRequiredService<ChainService>());

builder.Services.AddAuth0WebAppAuthentication(o =>
{
    o.Domain = builder.Configuration["Auth0:Domain"]!;
    o.ClientId = builder.Configuration["Auth0:ClientId"]!;
    o.ClientSecret = builder.Configuration["Auth0:ClientSecret"]!;
})
.WithAccessToken(o =>
{
    o.Audience = builder.Configuration["Auth0:Audience"]!;
});

builder.Services.AddAuthentication()
    .AddJwtBearer(o =>
    {
        o.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        o.Audience = builder.Configuration["Auth0:Audience"];
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ChainDbContext>().Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();