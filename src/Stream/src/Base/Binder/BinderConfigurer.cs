// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Stream.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Steeltoe.Stream.Binder
{
    public static class BinderConfigurer
    {
        public static void AddBinders(IServiceCollection services, IConfiguration config)
        {
            var entry = Assembly.GetEntryAssembly();
            List<BinderRegistration> registrations = new List<BinderRegistration>();

            if (entry != null)
            {
                var reg = AddBinders(Path.GetDirectoryName(entry.Location));
                registrations.AddRange(reg);
            }

            if (entry.Location != Environment.CurrentDirectory)
            {
                var reg = AddBinders(Environment.CurrentDirectory);
                registrations.AddRange(reg);
            }

            ConfigureBinders(services, config, registrations);
        }

        internal static void ConfigureBinders(IServiceCollection services, IConfiguration config, List<BinderRegistration> registrations)
        {
            foreach (var reg in registrations)
            {
                Type type = FindConfigureType(reg.ConfigureType);
                var constr = FindConstructor(type);
                if (constr != null)
                {
                    var inst = constr.Invoke(new object[] { config });
                    if (inst != null)
                    {
                        var method = FindConfigureServicesMethod(type);
                        if (method != null)
                        {
                            method.Invoke(inst, new object[] { services });
                        }
                    }
                }
            }
        }

        internal static MethodInfo FindConfigureServicesMethod(Type type)
        {
            return type.GetMethod("ConfigureServices", new Type[] { typeof(IServiceCollection) });
        }

        internal static Type FindConfigureType(string typeName)
        {
            return Type.GetType(typeName, true);
        }

        internal static ConstructorInfo FindConstructor(Type type)
        {
            return type.GetConstructor(new Type[] { typeof(IConfiguration) });
        }

        internal static List<BinderRegistration> AddBinders(string location)
        {
            List<BinderRegistration> registrations = new List<BinderRegistration>();
            DirectoryInfo dirinfo = new DirectoryInfo(location);
            foreach (var file in dirinfo.EnumerateFiles("*.dll"))
            {
                try
                {
                    var reg = CheckAssembly(file.FullName);
                    if (reg != null)
                    {
                        registrations.Add(reg);
                    }
                }
                catch (Exception)
                {
                    // log
                }
            }

            return registrations;
        }

        internal static BinderRegistration CheckAssembly(string assemblyPath)
        {
            var assembly = Assembly.LoadFile(assemblyPath);

            if (assembly != null)
            {
                var attribute = assembly.GetCustomAttribute<BinderAttribute>();
                if (attribute != null)
                {
                    return new BinderRegistration(attribute.Name, attribute.ConfigureType);
                }
            }

            return null;
        }
    }
}
