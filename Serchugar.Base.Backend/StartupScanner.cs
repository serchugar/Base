using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Serchugar.Base.Backend;

/// <summary>
/// Provides utility methods for scanning assemblies to discover entity key properties and controller routes at application startup.
/// </summary>
public static class StartupScanner
{
    private static readonly ConcurrentDictionary<Type, Func<object, object?>> KeyedProperties = new();
    private static readonly ConcurrentDictionary<Type, string> ControllerRoutes = new();
    
    /// <summary>
    /// Scans the specified assembly (or all loaded assemblies if none is provided) to discover entity types with a property marked by <see cref="KeyAttribute"/>.
    /// Stores a compiled delegate for retrieving the key property value for each discovered entity type.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance used to access application services.</param>
    /// <param name="assembly">The assembly to scan for entity types. If <c>null</c>, all loaded assemblies are scanned.</param>
    public static void DiscoverKeyedEntities(this IApplicationBuilder app, Assembly? assembly = null)
    {
        IEnumerable<Assembly> assemblies = assembly != null
            ? [assembly]
            : AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly asm in assemblies)
        {
            IEnumerable<Type> assemblyClasses = asm.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic);

            foreach (Type type in assemblyClasses)
            {
                PropertyInfo? keyProp = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p => p.IsDefined(typeof(KeyAttribute), inherit: true));
            
                if (keyProp == null) continue;
            
                var param = Expression.Parameter(typeof(object), "entity");
                var cast = Expression.Convert(param, type);
                var property = Expression.Property(cast, keyProp);
                var convertResult = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<object, object?>>(convertResult, param);
                var getter = lambda.Compile();
            
                KeyedProperties[type] = getter;
            }
        }
    }

    /// <summary>
    /// Scans the specified assembly (or the calling assembly if none is provided) to discover controller types and their route templates.
    /// Stores the route prefix for each controller type for later use (e.g., generating Location headers).
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance used to access application services.</param>
    /// <param name="assembly">The assembly to scan for controller types. If <c>null</c>, the calling assembly is used.</param>
    public static void DiscoverControllerRoutes(this IApplicationBuilder app, Assembly? assembly = null)
    {
        Assembly asm = assembly ?? Assembly.GetCallingAssembly();
        
        var actionProvider = app.ApplicationServices
            .GetRequiredService<IActionDescriptorCollectionProvider>();

        var byController = actionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Where(cad => cad.ControllerTypeInfo.Assembly == asm).GroupBy(cad => cad.ControllerTypeInfo.AsType());

        foreach (var grp in byController)
        {
            var controllerType = grp.Key;
            
            var classRoute = controllerType.GetCustomAttribute<RouteAttribute>(inherit: true);
            if (classRoute == null || string.IsNullOrWhiteSpace(classRoute.Template)) continue;
            string template = classRoute.Template;
            
            var anyAction = grp.FirstOrDefault(cad => cad.AttributeRouteInfo != null);
            if (anyAction == null) continue;
            var routeValues = anyAction.RouteValues;
            
            string resolved = System.Text.RegularExpressions.Regex.Replace(
                template,
                @"\[(\w+)\]",
                m =>
                {
                    var key = m.Groups[1].Value;
                    return routeValues.TryGetValue(key, out var value)
                        ? value!
                        : m.Value;
                });

            int idx = resolved.IndexOf("/{", StringComparison.Ordinal);
            string prefix = idx >= 0
                ? resolved[..idx]
                : resolved;

            ControllerRoutes[controllerType] = prefix;
        }
    }

    /// <summary>
    /// Retrieves the key value of the specified entity object.
    /// </summary>
    /// <param name="entity">The entity object whose key value is to be retrieved.</param>
    /// <returns>
    /// The key value of the entity, or <c>null</c> if the entity type is not recognized.
    /// </returns>
    public static object? GetEntityKey(object entity)
    {
        Type type = entity.GetType();
        
        if (KeyedProperties.TryGetValue(type, out var getter))
            return getter(entity);
        
        return null;
    }
    
    /// <summary>
    /// Retrieves the route prefix for the specified controller type.
    /// </summary>
    /// <param name="controllerType">The controller type whose route prefix is to be retrieved.</param>
    /// <returns>
    /// The route prefix of the controller, or <c>null</c> if the controller type is not recognized.
    /// </returns>
    public static string GetControllerRoute(Type controllerType) =>
        ControllerRoutes.GetValueOrDefault(controllerType)!;
}