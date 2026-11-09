using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
[SessionAuthorize("Participant")]
public class ParticipantController(RaceDayApiClient api) : Controller
{
    private string Token => HttpContext.Session.GetString("JwtToken")!;
    public async Task<IActionResult> Index(string? search,string? type)
    {
        try { var all=await api.GetAsync<List<EventVm>>("api/events") ?? new(); if(!string.IsNullOrWhiteSpace(search)) all=all.Where(e=>e.EventName.Contains(search,StringComparison.OrdinalIgnoreCase)||e.Location.Contains(search,StringComparison.OrdinalIgnoreCase)).ToList(); if(!string.IsNullOrWhiteSpace(type)) all=all.Where(e=>e.EventType.Equals(type,StringComparison.OrdinalIgnoreCase)).ToList(); ViewBag.Search=search; ViewBag.Type=type; return View(all); }
        catch(ApiException ex) { TempData["Error"]=ex.Message; return View(new List<EventVm>()); }
    }
    public async Task<IActionResult> Details(int id)
    {
        try { var e=await api.GetAsync<EventVm>($"api/events/{id}",Token); return e is null ? NotFound() : View(e); }
        catch(ApiException ex) { TempData["Error"]=ex.Message; return RedirectToAction(nameof(Index)); }
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Enrol(int categoryId)
    {
        try { await api.PostAsync<ApiEnrolmentRequest,EnrolmentVm>("api/enrolments",new ApiEnrolmentRequest { CategoryId=categoryId },Token); TempData["Success"]="Your enrolment has been recorded."; }
        catch(ApiException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(MyEnrolments));
    }
    public async Task<IActionResult> MyEnrolments()
    { try { return View(await api.GetAsync<List<EnrolmentVm>>("api/users/me/enrolments",Token) ?? new()); } catch(ApiException ex) { TempData["Error"]=ex.Message; return View(new List<EnrolmentVm>()); } }
    public async Task<IActionResult> MyResults()
    { try { return View(await api.GetAsync<List<ResultVm>>("api/users/me/results",Token) ?? new()); } catch(ApiException ex) { TempData["Error"]=ex.Message; return View(new List<ResultVm>()); } }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> CancelEnrolment(int id)
    { try { await api.DeleteAsync($"api/enrolments/{id}",Token); TempData["Success"]="Enrolment cancelled."; } catch(ApiException ex) { TempData["Error"]=ex.Message; } return RedirectToAction(nameof(MyEnrolments)); }
    [HttpGet] public async Task<IActionResult> Profile()
    { try { return View(await api.GetAsync<UserVm>("api/users/me",Token) is { } u ? new ProfileVm { FirstName=u.FirstName,LastName=u.LastName,Email=u.Email,ProfilePictureName=u.ProfilePictureName } : new ProfileVm()); } catch(ApiException ex) { TempData["Error"]=ex.Message; return View(new ProfileVm()); } }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Profile(ProfileVm model)
    {
        ModelState.Remove(nameof(model.Email)); ModelState.Remove(nameof(model.ProfilePictureName));
        if(!ModelState.IsValid) return View(model);
        try {
            if(model.ProfilePicture is not null) { var upload=await api.UploadAsync<StorageUploadVm>("api/storage/profile-picture",model.ProfilePicture,Token); model.ProfilePictureName=upload?.BlobName; }
            await api.PutAsync<ApiProfileRequest,UserVm>("api/users/me",new ApiProfileRequest { FirstName=model.FirstName,LastName=model.LastName,Password=string.IsNullOrWhiteSpace(model.NewPassword)?null:model.NewPassword,ProfilePictureName=model.ProfilePictureName },Token);
            TempData["Success"]="Profile updated."; return RedirectToAction(nameof(Profile));
        } catch(ApiException ex) { ModelState.AddModelError("",ex.Message); return View(model); }
    }
}
