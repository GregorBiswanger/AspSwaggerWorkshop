using AspRestApiWorkshop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace AspRestApiWorkshop.OperationFilters
{
    public class GetCampOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor.RouteValues["action"] != "GetCamp")
            {
                return;
            }

            var schema = context.SchemaGenerator.GenerateSchema(typeof(CampFriendlyModel),
                        context.SchemaRepository);

            if (operation.Responses.Any(a => a.Key == StatusCodes.Status200OK.ToString()))
            {
                operation.Responses[StatusCodes.Status200OK.ToString()].Content.Add(
                    "application/vnd.marvin.camp.friendly.hateoas+json",
                    new OpenApiMediaType() { Schema = schema });
            }
        }
    }
}





