using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;

namespace Gateway.Helpers;

internal static class SwaggerHelper
{
  internal static void MergeComponents(this OpenApiComponents target, OpenApiComponents source)
  {
    foreach (var schema in source.Schemas)
    {
      if (!target.Schemas.ContainsKey(schema.Key))
      {
        target.Schemas.Add(schema.Key, schema.Value);
      }
    }

    foreach (var parameter in source.Parameters)
    {
      if (!target.Parameters.ContainsKey(parameter.Key))
      {
        target.Parameters.Add(parameter.Key, parameter.Value);
      }
    }

    foreach (var response in source.Responses)
    {
      if (!target.Responses.ContainsKey(response.Key))
      {
        target.Responses.Add(response.Key, response.Value);
      }
    }

    foreach (var example in source.Examples)
    {
      if (!target.Examples.ContainsKey(example.Key))
      {
        target.Examples.Add(example.Key, example.Value);
      }
    }

     foreach (var schema in source.SecuritySchemes)
    {
      if (!target.SecuritySchemes.ContainsKey(schema.Key))
      {
        target.SecuritySchemes.Add(schema.Key, schema.Value);
      }
    }
  }

  internal static void MergePaths(this OpenApiPaths target, OpenApiPaths source, string rewritePrefix)
  {
    foreach (var path in source)
    {
      // Dynamically rewrite the path with the prefix (e.g., /api/v1/auth/login)
      var newPath = Regex.Replace(path.Key, @"^/api/(?<version>v\d+)", m =>
      {
        var version = m.Groups["version"].Value;
        return $"/api/{version}/{rewritePrefix}";
      });

      target.Add(newPath, path.Value);
    }
  }


  internal static void MergeSecurity(this IList<OpenApiSecurityRequirement> target, IList<OpenApiSecurityRequirement> source)
  {
    foreach (var path in source)
    {
      target.Add(path);
    }
  }
}