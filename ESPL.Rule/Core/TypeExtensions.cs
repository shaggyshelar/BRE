using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal static class TypeExtensions
    {
        private static readonly CacheDict<MethodBase, ParameterInfo[]> _ParamInfoCache = new CacheDict<MethodBase, ParameterInfo[]>(75);

        internal static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType)
        {
            DynamicMethod dynamicMethod = methodInfo as DynamicMethod;
            if (dynamicMethod != null)
            {
                return dynamicMethod.CreateDelegate(delegateType);
            }
            return Delegate.CreateDelegate(delegateType, methodInfo);
        }

        internal static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target)
        {
            DynamicMethod dynamicMethod = methodInfo as DynamicMethod;
            if (dynamicMethod != null)
            {
                return dynamicMethod.CreateDelegate(delegateType, target);
            }
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
        }

        internal static MethodInfo GetMethodValidated(this Type type, string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            MethodInfo method = type.GetMethod(name, bindingAttr, binder, types, modifiers);
            if (!method.MatchesArgumentTypes(types))
            {
                return null;
            }
            return method;
        }

        internal static ParameterInfo[] GetParametersCached(this MethodBase method)
        {
            ParameterInfo[] array = null;
            lock (TypeExtensions._ParamInfoCache)
            {
                if (!TypeExtensions._ParamInfoCache.TryGetValue(method, out array))
                {
                    array = method.GetParameters();
                    Type declaringType = method.DeclaringType;
                    if (declaringType != null && declaringType.CanCache())
                    {
                        TypeExtensions._ParamInfoCache[method] = array;
                    }
                }
            }
            return array;
        }

        internal static Type GetReturnType(this MethodBase mi)
        {
            if (!mi.IsConstructor)
            {
                return ((MethodInfo)mi).ReturnType;
            }
            return mi.DeclaringType;
        }

        internal static bool IsByRefParameter(this ParameterInfo pi)
        {
            return pi.ParameterType.IsByRef || (pi.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out;
        }

        private static bool MatchesArgumentTypes(this MethodInfo mi, Type[] argTypes)
        {
            if (mi == null || argTypes == null)
            {
                return false;
            }
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length != argTypes.Length)
            {
                return false;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!TypeUtils.AreReferenceAssignable(parameters[i].ParameterType, argTypes[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
