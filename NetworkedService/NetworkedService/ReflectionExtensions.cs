using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkedService
{
    public static class ReflectionExtensions
    {
        private class MethodNameComparer : IEqualityComparer<MethodInfo>
        {
            public bool Equals(MethodInfo x, MethodInfo y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(MethodInfo obj)
            {
                return obj.GetHashCode();
            }
        }

        public static IEnumerable<Type> GetAllInterfaces(this Type iface)
        {
            return iface.GetInterfaces().SelectMany(i => GetAllInterfaces(i))
                .Append(iface);
        }

        public static IEnumerable<MethodInfo> GetAllMethods(this Type iface, BindingFlags bindingFlags = BindingFlags.Default)
        {
            return iface.GetAllInterfaces()
                .SelectMany(i => i.GetMethods(bindingFlags))
                .Distinct(new MethodNameComparer());
        }

        public static MethodInfo GetNestedMethod(this Type iface, string methodName, BindingFlags bindingFlags)
        {
            var methods = iface.GetAllInterfaces()
                .Select(i => i.GetMethod(methodName, bindingFlags))
                .Where(m => m != null)
                .ToList();

            return methods.First();
        }
    }
}
