using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Retro.Data.Models;
using Retro.Web.Authentication;
using Retro.Web.Services;

namespace Retro.Web.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationUserService _applicationUserService;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationUserService applicationUserService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _applicationUserService = applicationUserService;
        }

        [HttpGet("Login")]
        public async Task<ActionResult<string>> Login(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                var authProperties = new AuthenticationProperties
                {
                    Items = {new KeyValuePair<string, string>("LoginProvider", "Microsoft")}
                };

                return Challenge(authProperties, MicrosoftAccountDefaults.AuthenticationScheme);
            }

            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);


            var user = await _userManager.FindByIdAsync(id) // For legacy users
                       ?? await _userManager.FindByEmailAsync(email); // Migrated users

            var isNewUser = user == null;
            
            if (isNewUser)
            {
                user = new ApplicationUser();
            }
            
            user.FullName = fullName;
            user.UserName = email;
            user.Email = email;


            var result = isNewUser
                ? await _userManager.CreateAsync(user)
                : await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Could not create/update user");
            }

            if (isNewUser)
            {
                await _userManager.AddLoginAsync(user, info);
            }
            
            await _applicationUserService.RefreshClaims(user);
            await _signInManager.SignInAsync(user, false);

            return RedirectToLocalUrl(returnUrl);
        }

        private ActionResult RedirectToLocalUrl(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var clientUrl = _configuration.GetValue<string>("ApplicationUrl");

            return Redirect(clientUrl);
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<ApplicationUser> Me()
        {
            return await _userManager.FindByIdAsync(User.GetId());
        }
        
        [HttpPost("Logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Json(new { Success = true });
        }
    }
}