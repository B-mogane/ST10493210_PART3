using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class AccountController(RaceDayApiClient api) : Controller
{
    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(new LoginVm { ReturnUrl = returnUrl }); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model)
    {
        if (!ModelState.IsValid) return View(model);
        try {
            var auth = await api.PostAsync<object,AuthApiResponse>("api/auth/login",new { model.Email,model.Password });
            if (auth is null || string.IsNullOrWhiteSpace(auth.Token)) { ModelState.AddModelError("","Login could not be completed."); return View(model); }
            HttpContext.Session.SetString("JwtToken",auth.Token); HttpContext.Session.SetString("Role",auth.User.Role);
            HttpContext.Session.SetString("UserName",auth.User.FirstName + " " + auth.User.LastName);
            HttpContext.Session.SetInt32("UserID",auth.User.UserID);
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl);
            return auth.User.Role == "Organiser" ? RedirectToAction("Dashboard","Organiser") : RedirectToAction("Index","Participant");
        } catch (ApiException ex) { ModelState.AddModelError("",ex.Message); return View(model); }
    }
    [HttpGet] public IActionResult Register() => View(new RegisterVm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.Role is not ("Organiser" or "Participant")) { ModelState.AddModelError(nameof(model.Role),"Choose a valid role."); return View(model); }
        try {
            var auth = await api.PostAsync<object,AuthApiResponse>("api/auth/register",new { model.FirstName,model.LastName,model.Email,model.Password,model.Role });
            if (auth is null) { ModelState.AddModelError("","Registration could not be completed."); return View(model); }
            HttpContext.Session.SetString("JwtToken",auth.Token); HttpContext.Session.SetString("Role",auth.User.Role);
            HttpContext.Session.SetString("UserName",auth.User.FirstName + " " + auth.User.LastName); HttpContext.Session.SetInt32("UserID",auth.User.UserID);
            return auth.User.Role == "Organiser" ? RedirectToAction("Dashboard","Organiser") : RedirectToAction("Index","Participant");
        } catch (ApiException ex) { ModelState.AddModelError("",ex.Message); return View(model); }
    }
    [HttpPost, ValidateAntiForgeryToken] public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index","Home"); }
    public IActionResult AccessDenied() => View();
}
