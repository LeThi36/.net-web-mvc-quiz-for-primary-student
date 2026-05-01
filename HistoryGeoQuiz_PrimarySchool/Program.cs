using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Implement.Auth;
using HistoryGeoQuiz_PrimarySchool.Services.Implement.Admin;
using HistoryGeoQuiz_PrimarySchool.Services.Implement.Teacher;
using HistoryGeoQuiz_PrimarySchool.Services.Implement.Student;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Auth;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Student;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Teacher;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Core;
using HistoryGeoQuiz_PrimarySchool.Options;
using HistoryGeoQuiz_PrimarySchool.Services.Implement.Storage;

var builder = WebApplication.CreateBuilder(args);

// Register CodePages for legacy encodings (Windows-1258, ExcelDataReader)
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

// Add services to the container.
builder.Services.AddControllersWithViews();

// P5: Register MemoryCache for dropdown data caching (10 min)
builder.Services.AddMemoryCache();

// Configure Options
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

// Configure Entity Framework with PostgreSQL (Supabase)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories (Data Access Layer)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register application services (Business Logic Layer)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Register Storage services
builder.Services.AddSingleton<StoragePathResolver>();
builder.Services.AddScoped<IFileStorageService, CloudinaryStorageService>();

// Add session support with security hardening
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // [S7.3] FIX: Secure cookie settings
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = ".HGQPS.Session"; // Custom name to avoid fingerprinting
});

var app = builder.Build();

// Register global exception middleware
app.UseMiddleware<HistoryGeoQuiz_PrimarySchool.Middleware.GlobalExceptionMiddleware>();

// [S3] FIX: Developer Exception Page only in Development, proper error handling in Production
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // [S10] FIX: HTTPS enforcement in production
    app.UseHsts();
}

// [S7.2] FIX: Security headers to prevent common web attacks
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

// [S10] FIX: HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
