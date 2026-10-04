using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    public static class AppDomainExtensions
    {
        public static ICollection<Type> GetMatchingTypesInAssembly(this Assembly assembly, Predicate<Type> predicate)
        {
            ICollection<Type> types = new List<Type>();
            try
            {
                types = assembly.GetTypes().Where(i => i != null && predicate(i) && i.Assembly == assembly).ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Type theType in ex.Types)
                {
                    try
                    {
                        if (theType != null && predicate(theType) && theType.Assembly == assembly)
                        {
                            types.Add(theType);
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Ignore
                    }
                }
            }
            return types;
        }

        public static IEnumerable<Type> GetAllDerivedTypes<T>(this AppDomain appDomain)
        {
            // TypeCache is Unity's maintained index of loaded types. It avoids touching
            // unloaded assemblies and handles ReflectionTypeLoadException for us.
            // The appDomain parameter is kept so existing call sites still compile.
            return TypeCache.GetTypesDerivedFrom<T>().ToList();
        }

        public static Type GetTypeFromName(this AppDomain appDomain, string className)
        {
#pragma warning disable UAC0005 // No TypeCache equivalent for lookup by name; assembly may be unloaded, so null-check the result
            foreach (var assembly in appDomain.GetAssemblies())
#pragma warning restore UAC0005
            {
                var type = assembly.GetType(className);

                if (type != null)
                {
                    return type;
                }
            }

            // Didn't find the type in any assemblies
            return null;
        }
    }
}