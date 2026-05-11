using System.Threading.Tasks;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiniChain.Web.Pages.Account;

public class Login : PageModel
{
    public async Task OnGetAsync(string returnUrl = "/")
    {
        var props = new AuthenticationProperties { RedirectUri = returnUrl };
        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, props);
    }
}