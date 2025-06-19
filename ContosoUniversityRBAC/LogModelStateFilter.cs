using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ContosoUniversityRBAC
{

    // 1. 创建自定义Filter
    public class LogModelStateFilter : IActionFilter
    {
        private readonly ILogger<LogModelStateFilter> _logger;

        public LogModelStateFilter(ILogger<LogModelStateFilter> logger)
        {
            _logger = logger;
        }
        private static string GetErrorMessage(ModelError error)
        {
            return !string.IsNullOrEmpty(error.ErrorMessage)
                ? error.ErrorMessage
                : error.Exception?.Message ?? "Unknown error";
        }
        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 仅当ModelState无效时记录
            if (!context.ModelState.IsValid)
            {
               
         
                //var errors = new Dictionary<string, List<string>>();
                var errors = new Dictionary<string,  string>();
                foreach (var key in context.ModelState.Keys)
                {
                    var entry = context.ModelState[key];
                    if (entry.Errors.Count == 0) continue;

                    var errorMessages = new List<string>();
                    foreach (var error in entry.Errors)
                    {
                        errorMessages.Add(GetErrorMessage(error));
                    }

                    errors.Add(key, string.Join(",", errorMessages));
                }

                // 添加请求信息
                var path = context.HttpContext.Request.Path;
                var method = context.HttpContext.Request.Method;

                // 使用结构化日志记录
                _logger.LogWarning("####Request:{Method} {Path} Failed: {@Errors}",
                    method, path,  errors);
            }
        }
    }
}