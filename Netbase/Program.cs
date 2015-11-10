using Netbase.CodeGen;
using Netbase.Shared;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;


namespace Netbase
{
 

    class Program
    {
        static void Main(string[] hArgs)
        {

            if(hArgs.Length != 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("Netbase \"definitionsfile.dll\" \"outputname\"");
                return;
            }

            string sAssemblyFile        = hArgs[0];
            string sOutputAssemblyName  = hArgs[1];
            string sSharedNamespace     = sOutputAssemblyName + ".Protocol";
            string sServiceNamespace    = sOutputAssemblyName + ".Service";
                                    
            Assembly hToGenerate                    = Assembly.Load(File.ReadAllBytes(sAssemblyFile));

            
            //List<ServiceCodeGen> hServiceContracts  = (from hT in hToGenerate.GetTypes()
            //                                           from hA in hT.GetCustomAttributes<ServiceContract>(false)
            //                                           select new ServiceCodeGen(hT, hA, sSharedNamespace, sServiceNamespace)).ToList();

            try
            {
                RpcServicePair hPair = (from hT in hToGenerate.GetTypes()
                                        from hA in hT.GetCustomAttributes<ServiceContract>(false)
                                        select new RpcServicePair(sServiceNamespace, sSharedNamespace, hA, hA.Service.GetCustomAttribute<CallbackContract>())).Single();

                IEnumerable<Type> hTypesToEncode = from hT in hToGenerate.GetTypes()
                                    from hA in hT.GetCustomAttributes<DataContract>(false)
                                    select hT;

                TypeEncoderCodeGen hEncoders = new TypeEncoderCodeGen(sSharedNamespace, hTypesToEncode.ToList());


                List<string> hCode = new List<string>();
                hCode.AddRange(hPair.SharedCode);
                hCode.Add(hEncoders.Code);

                CompilerResults hResult = BuildSharedAssemblyEx(sSharedNamespace + ".dll", "v3.5", hCode.ToArray());
                DumpOutput(hResult);
                Console.WriteLine();

                hResult = BuildServerAssembly(hResult.CompiledAssembly, sServiceNamespace + ".dll", "v4.0", hPair.ServiceCode.ToArray());
                DumpOutput(hResult);
            }
            catch (Exception hEx)
            {
                Console.WriteLine(hEx.Message);
            }

            Console.ReadLine();
        }

        private static void DumpOutput(CompilerResults hResult)
        {
            if (hResult.Errors.Count > 0)
            {
                foreach (CompilerError hError in hResult.Errors)
                {
                    Console.WriteLine(hError.ToString());
                    Process.Start("notepad.exe", hError.FileName);
                }
            }
            else
            {
                Console.WriteLine("Generated {0}.dll", new AssemblyName(hResult.CompiledAssembly.FullName).Name);

                foreach (Type hT in hResult.CompiledAssembly.ExportedTypes)
                {
                    Console.WriteLine("\t{0}", hT);
                }
            }
        }

         
        private static CompilerResults BuildSharedAssemblyEx(string sAssemblyName, string sFrameworkVersion, string[] hCode)
        {
            CompilerParameters hBuildParams = new CompilerParameters();
            hBuildParams.OutputAssembly = sAssemblyName;
            hBuildParams.GenerateExecutable = false;
            hBuildParams.GenerateInMemory = false;
#if DEBUG
            hBuildParams.IncludeDebugInformation = true;
            hBuildParams.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
#else
            hBuildParams.IncludeDebugInformation    = false;
            hBuildParams.CompilerOptions            = "/optimize";
#endif
            
            string[] hDependencies;

            if (sFrameworkVersion == "v3.5")
            {
                hDependencies = new string[]
                {
                    @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll",
                    @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll",
                    @"C:\windows\microsoft.net\framework\v2.0.50727\mscorlib.dll",
                    @"Netbase.Shared.dll",
                };
            }
            else
            { 
                hDependencies = (from hN in Assembly.GetExecutingAssembly().GetReferencedAssemblies() select Assembly.Load(hN).Location).ToArray();
            }

             hBuildParams.ReferencedAssemblies.AddRange(hDependencies);


            Dictionary<string, string> hOptions = new Dictionary<string, string>();
            hOptions.Add("CompilerVersion", sFrameworkVersion);

            using (CodeDomProvider hCodeDom = CodeDomProvider.CreateProvider("CSharp", hOptions))
            {
                return hCodeDom.CompileAssemblyFromSource(hBuildParams, hCode);
            }
        }

        private static CompilerResults BuildSharedAssembly(string sAssemblyName, string sFrameworkVersion, string[] hCode)
        {
            CompilerParameters hBuildParams         = new CompilerParameters();
            hBuildParams.OutputAssembly             = sAssemblyName;
            hBuildParams.GenerateExecutable         = false;
            hBuildParams.GenerateInMemory           = false;
#if DEBUG
            hBuildParams.IncludeDebugInformation    = true;
            hBuildParams.TempFiles                  = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
#else
            hBuildParams.IncludeDebugInformation    = false;
            hBuildParams.CompilerOptions            = "/optimize";
#endif
            Assembly hRootAssembly = sFrameworkVersion == "v3.5" ? Assembly.Load(Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(hA => hA.Name == "Netbase.Shared").Single()) : Assembly.GetExecutingAssembly();

            foreach (AssemblyName hAssemblyName in hRootAssembly.GetReferencedAssemblies())
            {
                Assembly hLoaded = Assembly.Load(hAssemblyName);
                hBuildParams.ReferencedAssemblies.Add(hLoaded.Location);
            }

            Dictionary<string, string> hOptions = new Dictionary<string, string>();
            hOptions.Add("CompilerVersion", sFrameworkVersion);

            using (CodeDomProvider hCodeDom = CodeDomProvider.CreateProvider("CSharp", hOptions))
            {
                return hCodeDom.CompileAssemblyFromSource(hBuildParams, hCode);
            }
        }

        private static CompilerResults BuildServerAssembly(Assembly hProtocolAssembly, string sAssemblyName, string sFrameworkVersion, string[] hCode)
        {
            CompilerParameters hBuildParams = new CompilerParameters();
            hBuildParams.OutputAssembly = sAssemblyName;
            hBuildParams.GenerateExecutable = false;
            hBuildParams.GenerateInMemory = false;
#if DEBUG
            hBuildParams.IncludeDebugInformation = true;
            hBuildParams.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
#else
            hBuildParams.IncludeDebugInformation    = false;
            hBuildParams.CompilerOptions            = "/optimize";
#endif
            Assembly hExecutingAssembly = Assembly.GetExecutingAssembly();

            foreach (AssemblyName hAssemblyName in hExecutingAssembly.GetReferencedAssemblies())
            {
                hBuildParams.ReferencedAssemblies.Add(Assembly.Load(hAssemblyName).Location);
            }

            hBuildParams.ReferencedAssemblies.Add(hExecutingAssembly.Location);
            hBuildParams.ReferencedAssemblies.Add(hProtocolAssembly.Location);
            hBuildParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            hBuildParams.ReferencedAssemblies.Add(typeof(Netbase.Server.ServerIOCP<>).Assembly.Location);            

            Dictionary<string, string> hOptions = new Dictionary<string, string>();
            hOptions.Add("CompilerVersion", sFrameworkVersion);

            using (CodeDomProvider hCodeDom = CodeDomProvider.CreateProvider("CSharp", hOptions))
            {
                return hCodeDom.CompileAssemblyFromSource(hBuildParams, hCode);
            }
        }        
    }
}
