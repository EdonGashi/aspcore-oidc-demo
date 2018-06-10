using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthServer.Infrastructure
{
    internal static class SwaggerUtils
    {
        public class LowercaseDocumentFilter : IDocumentFilter
        {
            public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
            {
                var paths = swaggerDoc.Paths;

                //	generate the new keys
                var newPaths = new Dictionary<string, PathItem>();
                var removeKeys = new List<string>();
                foreach (var path in paths)
                {
                    var newKey = path.Key.ToLower();
                    if (newKey != path.Key)
                    {
                        removeKeys.Add(path.Key);
                        newPaths.Add(newKey, path.Value);
                    }
                }

                //	add the new keys
                foreach (var path in newPaths)
                {
                    swaggerDoc.Paths.Add(path.Key, path.Value);
                }

                //	remove the old keys
                foreach (var key in removeKeys)
                {
                    swaggerDoc.Paths.Remove(key);
                }
            }
        }

        public static Info CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new Info
            {
                Title = $"Sample API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = "A sample application with Swagger, Swashbuckle, and API versioning."
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}