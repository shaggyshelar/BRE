using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal static class TypeUtils
    {
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal const MethodAttributes PublicStatic = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static;

        private static readonly Assembly _mscorlib = typeof(object).Assembly;

        private static readonly Assembly _systemCore = typeof(Expression).Assembly;

        internal static bool AreEquivalent(Type t1, Type t2)
        {
            return t1 == t2 || t1.IsEquivalentTo(t2);
        }

        internal static bool AreReferenceAssignable(Type dest, Type src)
        {
            return TypeUtils.AreEquivalent(dest, src) || (!dest.IsValueType && !src.IsValueType && dest.IsAssignableFrom(src));
        }

        internal static bool CanCache(this Type t)
        {
            Assembly assembly = t.Assembly;
            if (assembly != TypeUtils._mscorlib && assembly != TypeUtils._systemCore)
            {
                return false;
            }
            if (t.IsGenericType)
            {
                Type[] genericArguments = t.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    Type t2 = genericArguments[i];
                    if (!t2.CanCache())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static MethodInfo FindConversionOperator(MethodInfo[] methods, Type typeFrom, Type typeTo, bool implicitOnly)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo methodInfo = methods[i];
                if ((methodInfo.Name == "op_Implicit" || (!implicitOnly && methodInfo.Name == "op_Explicit")) && TypeUtils.AreEquivalent(methodInfo.ReturnType, typeTo) && TypeUtils.AreEquivalent(methodInfo.GetParametersCached()[0].ParameterType, typeFrom))
                {
                    return methodInfo;
                }
            }
            return null;
        }

        internal static Type FindGenericType(Type definition, Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && TypeUtils.AreEquivalent(type.GetGenericTypeDefinition(), definition))
                {
                    return type;
                }
                if (definition.IsInterface)
                {
                    Type[] interfaces = type.GetInterfaces();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        Type type2 = interfaces[i];
                        Type type3 = TypeUtils.FindGenericType(definition, type2);
                        if (type3 != null)
                        {
                            return type3;
                        }
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        internal static MethodInfo GetBooleanOperator(Type type, string name)
        {
            MethodInfo methodValidated;
            while (true)
            {
                methodValidated = type.GetMethodValidated(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]
				{
					type
				}, null);
                if (methodValidated != null && methodValidated.IsSpecialName && !methodValidated.ContainsGenericParameters)
                {
                    break;
                }
                type = type.BaseType;
                if (!(type != null))
                {
                    goto Block_3;
                }
            }
            return methodValidated;
        Block_3:
            return null;
        }

        internal static Type GetNonNullableType(this Type type)
        {
            if (type.IsNullableType())
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        internal static Type GetNonRefType(this Type type)
        {
            if (!type.IsByRef)
            {
                return type;
            }
            return type.GetElementType();
        }

        internal static Type GetNullableType(Type type)
        {
            if (type.IsValueType && !type.IsNullableType())
            {
                return typeof(Nullable<>).MakeGenericType(new Type[]
				{
					type
				});
            }
            return type;
        }

        internal static MethodInfo GetUserDefinedCoercionMethod(Type convertFrom, Type convertToType, bool implicitOnly)
        {
            Type nonNullableType = convertFrom.GetNonNullableType();
            Type nonNullableType2 = convertToType.GetNonNullableType();
            MethodInfo[] methods = nonNullableType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo methodInfo = TypeUtils.FindConversionOperator(methods, convertFrom, convertToType, implicitOnly);
            if (methodInfo != null)
            {
                return methodInfo;
            }
            MethodInfo[] methods2 = nonNullableType2.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            methodInfo = TypeUtils.FindConversionOperator(methods2, convertFrom, convertToType, implicitOnly);
            if (methodInfo != null)
            {
                return methodInfo;
            }
            if (!TypeUtils.AreEquivalent(nonNullableType, convertFrom) || !TypeUtils.AreEquivalent(nonNullableType2, convertToType))
            {
                methodInfo = TypeUtils.FindConversionOperator(methods, nonNullableType, nonNullableType2, implicitOnly);
                if (methodInfo == null)
                {
                    methodInfo = TypeUtils.FindConversionOperator(methods2, nonNullableType, nonNullableType2, implicitOnly);
                }
                if (methodInfo != null)
                {
                    return methodInfo;
                }
            }
            return null;
        }

        internal static bool HasBuiltInEqualityOperator(Type left, Type right)
        {
            if (!left.IsInterface || right.IsValueType)
            {
                if (right.IsInterface && !left.IsValueType)
                {
                    return true;
                }
                if (!left.IsValueType && !right.IsValueType && (TypeUtils.AreReferenceAssignable(left, right) || TypeUtils.AreReferenceAssignable(right, left)))
                {
                    return true;
                }
                if (!TypeUtils.AreEquivalent(left, right))
                {
                    return false;
                }
                Type nonNullableType = left.GetNonNullableType();
                if (!(nonNullableType == typeof(bool)) && !TypeUtils.IsNumeric(nonNullableType) && !nonNullableType.IsEnum)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool HasIdentityPrimitiveOrNullableConversion(Type source, Type dest)
        {
            return TypeUtils.AreEquivalent(source, dest) || (source.IsNullableType() && TypeUtils.AreEquivalent(dest, source.GetNonNullableType())) || (dest.IsNullableType() && TypeUtils.AreEquivalent(source, dest.GetNonNullableType())) || (TypeUtils.IsConvertible(source) && TypeUtils.IsConvertible(dest) && dest.GetNonNullableType() != typeof(bool));
        }

        internal static bool HasReferenceConversion(Type source, Type dest)
        {
            if (source == typeof(void) || dest == typeof(void))
            {
                return false;
            }
            Type nonNullableType = source.GetNonNullableType();
            Type nonNullableType2 = dest.GetNonNullableType();
            if (!nonNullableType.IsAssignableFrom(nonNullableType2))
            {
                if (nonNullableType2.IsAssignableFrom(nonNullableType))
                {
                    return true;
                }
                if (source.IsInterface || dest.IsInterface)
                {
                    return true;
                }
                if (!TypeUtils.IsLegalExplicitVariantDelegateConversion(source, dest) && !(source == typeof(object)) && !(dest == typeof(object)))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool HasReferenceEquality(Type left, Type right)
        {
            return !left.IsValueType && !right.IsValueType && (left.IsInterface || right.IsInterface || TypeUtils.AreReferenceAssignable(left, right) || TypeUtils.AreReferenceAssignable(right, left));
        }

        internal static bool IsArithmetic(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsBool(Type type)
        {
            return type.GetNonNullableType() == typeof(bool);
        }

        private static bool IsContravariant(Type t)
        {
            return GenericParameterAttributes.None != (t.GenericParameterAttributes & GenericParameterAttributes.Contravariant);
        }

        internal static bool IsConvertible(Type type)
        {
            type = type.GetNonNullableType();
            if (type.IsEnum)
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsCovariant(Type t)
        {
            return GenericParameterAttributes.None != (t.GenericParameterAttributes & GenericParameterAttributes.Covariant);
        }

        private static bool IsDelegate(Type t)
        {
            return t.IsSubclassOf(typeof(Delegate));
        }

        internal static bool IsFloatingPoint(Type type)
        {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsImplicitBoxingConversion(Type source, Type destination)
        {
            return (source.IsValueType && (destination == typeof(object) || destination == typeof(ValueType))) || (source.IsEnum && destination == typeof(Enum));
        }

        internal static bool IsImplicitlyConvertible(Type source, Type destination)
        {
            return TypeUtils.AreEquivalent(source, destination) || TypeUtils.IsImplicitNumericConversion(source, destination) || TypeUtils.IsImplicitReferenceConversion(source, destination) || TypeUtils.IsImplicitBoxingConversion(source, destination) || TypeUtils.IsImplicitNullableConversion(source, destination);
        }

        private static bool IsImplicitNullableConversion(Type source, Type destination)
        {
            return destination.IsNullableType() && TypeUtils.IsImplicitlyConvertible(source.GetNonNullableType(), destination.GetNonNullableType());
        }

        private static bool IsImplicitNumericConversion(Type source, Type destination)
        {
            TypeCode typeCode = Type.GetTypeCode(source);
            TypeCode typeCode2 = Type.GetTypeCode(destination);
            switch (typeCode)
            {
                case TypeCode.Char:
                    switch (typeCode2)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                    break;
                case TypeCode.SByte:
                    switch (typeCode2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Byte:
                    switch (typeCode2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                    break;
                case TypeCode.Int16:
                    switch (typeCode2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    switch (typeCode2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                    break;
                case TypeCode.Int32:
                    switch (typeCode2)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    switch (typeCode2)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (typeCode2)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                    break;
                case TypeCode.Single:
                    return typeCode2 == TypeCode.Double;
                default:
                    return false;
            }
        }

        private static bool IsImplicitReferenceConversion(Type source, Type destination)
        {
            return destination.IsAssignableFrom(source);
        }

        internal static bool IsInteger(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsIntegerOrBool(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        private static bool IsInvariant(Type t)
        {
            return GenericParameterAttributes.None == (t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask);
        }

        internal static bool IsLegalExplicitVariantDelegateConversion(Type source, Type dest)
        {
            if (!TypeUtils.IsDelegate(source) || !TypeUtils.IsDelegate(dest) || !source.IsGenericType || !dest.IsGenericType)
            {
                return false;
            }
            Type genericTypeDefinition = source.GetGenericTypeDefinition();
            if (dest.GetGenericTypeDefinition() != genericTypeDefinition)
            {
                return false;
            }
            Type[] genericArguments = genericTypeDefinition.GetGenericArguments();
            Type[] genericArguments2 = source.GetGenericArguments();
            Type[] genericArguments3 = dest.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                Type type = genericArguments2[i];
                Type type2 = genericArguments3[i];
                if (!TypeUtils.AreEquivalent(type, type2))
                {
                    Type t = genericArguments[i];
                    if (TypeUtils.IsInvariant(t))
                    {
                        return false;
                    }
                    if (TypeUtils.IsCovariant(t))
                    {
                        if (!TypeUtils.HasReferenceConversion(type, type2))
                        {
                            return false;
                        }
                    }
                    else if (TypeUtils.IsContravariant(t) && (type.IsValueType || type2.IsValueType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static bool IsNumeric(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsSameOrSubclass(Type type, Type subType)
        {
            return TypeUtils.AreEquivalent(type, subType) || subType.IsSubclassOf(type);
        }

        internal static bool IsUnsigned(Type type)
        {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        internal static bool IsUnsignedInt(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsValidInstanceType(MemberInfo member, Type instanceType)
        {
            Type declaringType = member.DeclaringType;
            if (TypeUtils.AreReferenceAssignable(declaringType, instanceType))
            {
                return true;
            }
            if (instanceType.IsValueType)
            {
                if (TypeUtils.AreReferenceAssignable(declaringType, typeof(object)))
                {
                    return true;
                }
                if (TypeUtils.AreReferenceAssignable(declaringType, typeof(ValueType)))
                {
                    return true;
                }
                if (instanceType.IsEnum && TypeUtils.AreReferenceAssignable(declaringType, typeof(Enum)))
                {
                    return true;
                }
                if (declaringType.IsInterface)
                {
                    Type[] interfaces = instanceType.GetInterfaces();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        Type src = interfaces[i];
                        if (TypeUtils.AreReferenceAssignable(declaringType, src))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static void ValidateType(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format("Type {0} is a generic type definition", type));
            }
            if (type.ContainsGenericParameters)
            {
                throw new ArgumentException(string.Format("Type {0} contains generic parameters", type));
            }
        }
    }
}
