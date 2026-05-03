namespace HistoryGeoQuiz_PrimarySchool.Middleware
{
    /// <summary>
    /// Global exception handler middleware.
    /// Logs unhandled exceptions and allows the system's exception handler to show the error page.
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
                
                // Rethrow to let app.UseExceptionHandler handle it properly
                // while preserving the exception information for the Error page.
                throw;
            }
        }
    }
}
