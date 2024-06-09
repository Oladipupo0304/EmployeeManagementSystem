using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.Context;
using EmployeeManagementSystem.Models.Auth;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers;

     public class AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            INotyfService notyf,
            EMSContext emsContext,
            IHttpContextAccessor httpContextAccessor) : Controller
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;
    private readonly INotyfService _notyfService = notyf;
    private readonly EMSContext _emsContext = emsContext;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        
        [RedirectAuthenticatedUsers]

    //adding the register action to y  controller
    public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == model.Email || u.UserName == model.Username);

                if (existingUser != null)
                {
                    _notyfService.Warning("Oops, User already exist!");
                    return View();
                }

                var user = new IdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    _notyfService.Error("An error occured while registering user! \n Wrong parameter!!!");
                    return View();
                }

                _notyfService.Success("Registration was successful"  + model.Username );
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Login", "Auth");
            }

            return View(model);
        }
    //adding the login action to y  controller
    [RedirectAuthenticatedUsers]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Check if the user exists
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                // Check the password
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Login successful, redirect to the appropriate page
                    _notyfService.Success("Login was successful");
                    return RedirectToAction("RegisterCourse"); //new { id = student.StudentID });

                }
            }
            ModelState.AddModelError("", "Invalid login attempt");
        }
        return View(model);
    }

    public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Auth");
        }
}

     
