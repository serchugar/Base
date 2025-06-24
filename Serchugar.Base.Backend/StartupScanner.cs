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

// TODO: Add and modify XML Comments
public static class StartupScanner
{
    private static readonly ConcurrentDictionary<Type, Func<object, object?>> KeyedProperties = new();
    private static readonly ConcurrentDictionary<Type, string> ControllerRoutes = new();
    
    public static void DiscoverKeyedEntities(this IApplicationBuilder app, Assembly? assembly = null)
    {
        Assembly asm = assembly ?? Assembly.GetCallingAssembly();
        
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

    public static object? GetEntityKey(object entity)
    {
        Type type = entity.GetType();
        
        if (KeyedProperties.TryGetValue(type, out var getter))
            return getter(entity);
        
        return null;
    }
    
    public static string GetControllerRoute(Type controllerType) =>
        ControllerRoutes.GetValueOrDefault(controllerType)!;
}