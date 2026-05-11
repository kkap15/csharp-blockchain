using System.Threading.Tasks;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiniChain.Web.Pages.Account;

public class Logout : PageModel
{
    public async Task OnGetAsync()
    {
        var props = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();
        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme,  props);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}