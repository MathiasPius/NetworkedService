using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkedService
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetAllInterfaces(this Type iface)
        {
            return iface.GetInterfaces().SelectMany(i => GetAllInterfaces(i))
                .Append(iface);
        }

        public static IEnumerable<MethodInfo> GetAllMethods(this Type iface, BindingFlags bindingFlags = BindingFlags.Default)
        {
            return iface.GetAllInterfaces().SelectMany(i => i.GetMethods(bindingFlags));
        }

        public static MethodInfo GetNestedMethod(this Type iface, string methodName, BindingFlags bindingFlags)
        {
            var methods = iface.GetAllInterfaces()
                .Select(i => i.GetMethod(methodName, bindingFlags))
                .Where(m => m != null)
                .ToList();

            return methods.Single();
        }
    }
}
