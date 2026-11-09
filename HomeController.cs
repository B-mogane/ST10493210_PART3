using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class HomeController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        try { return View(await api.GetAsync<List<EventVm>>("api/events") ?? new()); }
        catch (Exception ex) { ViewBag.ApiError = "Events are temporarily unavailable. " + ex.Message; return View(new List<EventVm>()); }
    }
    public IActionResult Error() => View();
}
