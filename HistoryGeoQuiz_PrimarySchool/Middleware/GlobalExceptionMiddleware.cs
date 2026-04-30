namespace HistoryGeoQuiz_PrimarySchool.Middleware
{
    /// <summary>
    /// Global exception handler middleware.
    /// Catches unhandled exceptions, logs them, and redirects to a user-friendly error page.
    /// Prevents raw stack traces from leaking to users in production.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                // Avoid redirect loops on error page itself
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Home/Error");
                }
            }
        }
    }
}
