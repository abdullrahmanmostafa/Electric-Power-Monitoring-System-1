using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Electric_Power_Monitoring_System
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // الحصول على اسم الـ Controller و Action
            var controllerName = context.MethodInfo.DeclaringType?.Name;
            var actionName = context.MethodInfo.Name;

            // قائمة الـ Controllers التي تحتاج إلى X-User-Id
            var targetControllers = new[] { "HubsController", "NotificationsController", "ConsumptionController", "CompareController" };
            // أو نضيف استثناء للـ ReadingsController
            if (controllerName == "ReadingsController")
                return; // لا نضيف الهيدر لـ ReadingsController

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-User-Id",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "User identifier (required for authenticated endpoints)"
            });
        }
    }
}