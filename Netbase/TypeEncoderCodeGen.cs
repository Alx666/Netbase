using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netbase.CodeGen
{
    internal class TypeEncoderCodeGen
    {
        public string Code { get; private set; }
        public TypeEncoderCodeGen(string sNamespace, List<Type> hTypes)
        {
            StringBuilder hTypesCode = new StringBuilder();

            foreach (Type hType in hTypes)
            {
                StringBuilder   hFields         = new StringBuilder();
                PropertyInfo[]  hProperties     = hType.GetProperties();
                FieldInfo[]     hFieldss        = hType.GetFields();
                string          sFamily         = hType.IsClass ? "class" : "struct";

                foreach (PropertyInfo hProperty in hProperties)
                {
                    hFields.AppendFormat("public {0} {1} { get; set; }{2}", hProperty.PropertyType.AsKeyword(), hProperty.Name, Environment.NewLine);
                }

                foreach (FieldInfo hField in hFieldss)
                {
                    hFields.AppendFormat("public {0} {1};{2}", hField.FieldType.AsKeyword(), hField.Name, Environment.NewLine);
                }

                hTypesCode.AppendFormat(@"
                public {0} {1}
                {{
                    {2}
                }}
                ", sFamily, hType.Name, hFields);
            }


            StringBuilder hFormatters = new StringBuilder();
            
            foreach(Type hType in hTypes)
            {
                PropertyInfo[] hProperties  = hType.GetProperties();
                FieldInfo[] hFields         = hType.GetFields();
                StringBuilder hWrites       = new StringBuilder();

                foreach (PropertyInfo hProperty in hProperties)
                {
                    hWrites.AppendFormat("hThis.Write(x.{0});{1}", hProperty.Name, Environment.NewLine);
                }

                foreach (FieldInfo hField in hFields)
                {
                    hWrites.AppendFormat("hThis.Write(x.{0});{1}", hField.Name, Environment.NewLine);
                }

                StringBuilder hReads = new StringBuilder();

                foreach (PropertyInfo hProperty in hProperties)
                {
                    hReads.AppendFormat("x.{0} = hThis.Read{1}();{2}", hProperty.Name, hProperty.PropertyType.Name, Environment.NewLine);
                }

                foreach (FieldInfo hField in hFields)
                {
                    hReads.AppendFormat("x.{0} = hThis.Read{1}();{2}", hField.Name, hField.FieldType.Name, Environment.NewLine);
                }

                hFormatters.AppendFormat(@"

                        public static void Write(this BinaryWriter hThis, {0} x)
                        {{
                            {1}
                        }}

//                        public static void Write(this BinaryWriter hThis, {0}[] x)
//                        {{                            
//                            ushort uCount = x == null ? (ushort)0 : (ushort)x.Length;
//
//                            hThis.Write(uCount);
//
//                            for(int i = 0; i < x.Length; i++)
//                            {{
//                                hThis.Write(x[i]);
//                            }}
//                        }}

                        public static {0} Read{0}(this BinaryReader hThis)
                        {{
                            {0} x = new {0}();
                            {2}
                            return x;
                        }}

//                        public static {0}[] ReadArray{0}(this BinaryReader hThis)
//                        {{
//                            int iCount = hThis.ReadUInt16();
//                            {0}[] x = new {0}[iCount];
//
//                            for(int i = 0; i < iCount; i++)
//                            {{
//                                x[i] = hThis.Read{0}();
//                            }}
//
//                            return x;
//                        }}

                ", hType.AsKeyword(), hWrites,  hReads);
            }

            Code = string.Format(@"
            using System.IO;

            namespace {0}
            {{
                    {1}

                    public static class BinaryExtensions
                    {{
                        {2}
                    }}
            }}", 
            sNamespace,
            hTypesCode.ToString(), 
            hFormatters.ToString());        
        }
    }
}
