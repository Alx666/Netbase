using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Netbase.CodeGen
{
    public static class TypeExtensions
    {
        public static StringBuilder AppendCode(this StringBuilder hSb, params object[] hParams)
        {
            for (int i = 0; i < hParams.Length; i++)
            {
                hSb.Append(hParams[i]);
            }

            return hSb.AppendLine();
        }

        public static string GetParametersString(this MethodInfo hMethod, bool bDeclaration)
        {            
            ParameterInfo[] hParams = hMethod.GetParameters();

            if (hParams.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder hSb = new StringBuilder();

                for (int i = 0; i < hParams.Length; i++)
                {
                    hSb.AppendFormat("{0} {1}, ", bDeclaration ? hParams[i].ParameterType.AsKeyword() : string.Empty, hParams[i].Name);
                }

                hSb.Remove(hSb.Length - 2, 2);

                return hSb.ToString();
            }
            
        }

        public static string GetParametersTypeofs(this MethodInfo hMethod)
        {
            ParameterInfo[] hParams = hMethod.GetParameters();

            if (hParams.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder hSb = new StringBuilder();

                for (int i = 0; i < hParams.Length; i++)
                {
                    hSb.AppendFormat("typeof({0}), ", hParams[i].ParameterType.AsKeyword());
                }

                hSb.Remove(hSb.Length - 2, 2);

                return hSb.ToString();
            }
        }

        public static string AsKeyword(this Type hName)
        {            
            switch (hName.Name)
            {
                case "Void":    return "void";
                case "Boolean": return "bool";
                case "Byte":    return "byte";
                case "SByte":   return "sbyte";
                case "Char":    return "char";
                case "Int16":   return "short";
                case "UInt16":  return "ushort";
                case "Int32":   return "int";
                case "UInt32":  return "uint";
                case "Int64":   return "long";
                case "UInt64":  return "ulong";
                case "Single":  return "float";
                case "Double":  return "double";
                case "Decimal": return "decimal";
                case "String":  return "string";
                default: return hName.Name;                
            }
        }
    }
}
