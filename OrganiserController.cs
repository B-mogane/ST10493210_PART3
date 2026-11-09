using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
[SessionAuthorize("Organiser")]
public class OrganiserController(RaceDayApiClient api) : Controller
{
    private string Token => HttpContext.Session.GetString("JwtToken")!;
    public async Task<IActionResult> Dashboard()
    {
        try { return View(await api.GetAsync<List<EventVm>>("api/events/mine",Token) ?? new()); }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return View(new List<EventVm>()); }
    }
    public async Task<IActionResult> Events()
    {
        try { return View(await api.GetAsync<List<EventVm>>("api/events/mine",Token) ?? new()); }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return View(new List<EventVm>()); }
    }
    [HttpGet] public IActionResult CreateEvent() => View("EventForm",new EventFormVm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormVm model)
    {
        if (!ModelState.IsValid) return View("EventForm",model);
        try {
            if (model.BannerImage is not null) { var upload = await api.UploadAsync<StorageUploadVm>("api/storage/event-banner",model.BannerImage,Token); model.BannerImageName = upload?.BlobName; }
            var req = new ApiEventRequest { EventName=model.EventName,Description=model.Description,EventDate=model.EventDate,Location=model.Location,RouteInfo=model.RouteInfo,EntryFee=model.EntryFee,Status=model.Status,DistanceKm=model.DistanceKm,EventType=model.EventType,BannerImageName=model.BannerImageName };
            await api.PostAsync<ApiEventRequest,EventVm>("api/events",req,Token); TempData["Success"]="Event created successfully."; return RedirectToAction(nameof(Events));
        } catch (ApiException ex) { ModelState.AddModelError("",ex.Message); return View("EventForm",model); }
    }
    [HttpGet] public async Task<IActionResult> EditEvent(int id)
    {
        try { var e=await api.GetAsync<EventVm>($"api/events/{id}",Token); if(e is null) return NotFound(); return View("EventForm",new EventFormVm { EventID=e.EventID,EventName=e.EventName,Description=e.Description,EventDate=e.EventDate,Location=e.Location,RouteInfo=e.RouteInfo,EntryFee=e.EntryFee,Status=e.Status,DistanceKm=e.DistanceKm,EventType=e.EventType,BannerImageName=e.BannerImageName }); }
        catch (ApiException ex) { TempData["Error"]=ex.Message; return RedirectToAction(nameof(Events)); }
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(EventFormVm model)
    {
        if (!ModelState.IsValid) return View("EventForm",model);
        try {
            if (model.BannerImage is not null) { var upload=await api.UploadAsync<StorageUploadVm>("api/storage/event-banner",model.BannerImage,Token); model.BannerImageName=upload?.BlobName; }
            var req=new ApiEventRequest { EventName=model.EventName,Description=model.Description,EventDate=model.EventDate,Location=model.Location,RouteInfo=model.RouteInfo,EntryFee=model.EntryFee,Status=model.Status,DistanceKm=model.DistanceKm,EventType=model.EventType,BannerImageName=model.BannerImageName };
            await api.PutAsync<ApiEventRequest,EventVm>($"api/events/{model.EventID}",req,Token); TempData["Success"]="Event updated."; return RedirectToAction(nameof(Events));
        } catch(ApiException ex) { ModelState.AddModelError("",ex.Message); return View("EventForm",model); }
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> DeleteEvent(int id)
    {
        try { await api.DeleteAsync($"api/events/{id}",Token); TempData["Success"]="Event deleted."; }
        catch(ApiException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Events));
    }
    public async Task<IActionResult> Categories(int id)
    {
        try { ViewBag.Event=await api.GetAsync<EventVm>($"api/events/{id}",Token); ViewBag.Categories=await api.GetAsync<List<CategoryVm>>($"api/events/{id}/categories",Token) ?? new(); return View(new CategoryFormVm { EventID=id }); }
        catch(ApiException ex) { TempData["Error"]=ex.Message; return RedirectToAction(nameof(Events)); }
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> AddCategory(CategoryFormVm model)
    {
        if (!ModelState.IsValid) return await Categories(model.EventID);
        try { await api.PostAsync<ApiCategoryRequest,CategoryVm>($"api/events/{model.EventID}/categories",new ApiCategoryRequest { CategoryName=model.CategoryName,DistanceKm=model.DistanceKm,MaxParticipants=model.MaxParticipants,Fee=model.Fee },Token); TempData["Success"]="Category added."; }
        catch(ApiException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Categories),new { id=model.EventID });
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> DeleteCategory(int id,int eventId)
    {
        try { await api.DeleteAsync($"api/categories/{id}",Token); TempData["Success"]="Category deleted."; }
        catch(ApiException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Categories),new { id=eventId });
    }
    public async Task<IActionResult> Enrolments(int id)
    {
        try { ViewBag.Event=await api.GetAsync<EventVm>($"api/events/{id}",Token); return View(await api.GetAsync<List<EnrolmentVm>>($"api/events/{id}/enrolments",Token) ?? new()); }
        catch(ApiException ex) { TempData["Error"]=ex.Message; return RedirectToAction(nameof(Events)); }
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> SaveResult(ResultFormVm model,int eventId)
    {
        if (!ModelState.IsValid) { TempData["Error"]="Enter a valid finish time and positive position."; return RedirectToAction(nameof(Enrolments),new { id=eventId }); }
        try { await api.PostAsync<ApiResultRequest,ResultVm>("api/results",new ApiResultRequest { EnrolmentId=model.EnrolmentId,FinishTime=model.FinishTime,Position=model.Position,Notes=model.Notes },Token); TempData["Success"]="Result saved."; }
        catch(ApiException ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction(nameof(Enrolments),new { id=eventId });
    }
}
