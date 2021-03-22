using AspRestApiWorkshop.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspRestApiWorkshop.OperationFilters
{
    public class CreateCampOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor.RouteValues["action"] != "Insert")
            {
                return;
            }

            var schema = context.SchemaGenerator.GenerateSchema(typeof(CampModelForSimpleCreation),
                        context.SchemaRepository);

            operation.RequestBody.Content.Add(
                "application/vnd.marvin.simplecamp+json",
                new OpenApiMediaType() { Schema = schema });
        }
    }
}



