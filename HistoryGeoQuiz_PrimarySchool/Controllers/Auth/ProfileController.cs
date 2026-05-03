using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Filters;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HistoryGeoQuiz_PrimarySchool.Controllers.Auth
{
    public class ProfileController : BaseController
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return RedirectToAction("Login", "Account");

            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null) return NotFound();

            var updateModel = new UpdateProfileViewModel
            {
                FullName = profile.FullName,
                Gender = profile.Gender
            };

            ViewBag.Profile = profile;
            return View(updateModel);
        }

        // POST: Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return RedirectToAction("Login", "Account");

            var (success, error) = await _profileService.UpdateProfileAsync(userId, model);
            
            if (success)
            {
                TempData[TempDataKeys.SuccessMessage] = "Cập nhật thông tin cá nhân thành công!";
                
                // Update session if name changed
                var profile = await _profileService.GetProfileAsync(userId);
                if (profile != null)
                {
                    HttpContext.Session.SetString(SessionKeys.UserName, profile.FullName);
                    if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
                    {
                        HttpContext.Session.SetString(SessionKeys.UserAvatarUrl, profile.AvatarUrl);
                    }
                }
            }
            else
            {
                TempData[TempDataKeys.ErrorMessage] = error ?? "Đã xảy ra lỗi khi cập nhật.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
