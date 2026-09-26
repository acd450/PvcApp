using Microsoft.OpenApi.Models;
using PlexVideoConverter.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlexVideoConverter.Models;

public class CustomSwaggerDocuments : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        context.SchemaGenerator.GenerateSchema(typeof(QueueStatusArgs), context.SchemaRepository);
    }
}