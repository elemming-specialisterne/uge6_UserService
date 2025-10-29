using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Warehouse_UserService.Scalar
{
    internal class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var schemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
            if (schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
            {
                var bearerScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization"
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = bearerScheme;

                foreach (var path in document.Paths.Values)
                {
                    foreach (var operation in path.Operations.Values)
                    {
                        operation.Security ??= new List<OpenApiSecurityRequirement>();
                        operation.Security.Add(new OpenApiSecurityRequirement
                        {
                            [bearerScheme] = Array.Empty<string>()
                        });
                    }
                }
            }
        }
    }
}