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
using System;
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Stream.Binder
{
    public class BinderConfigurerTest
    {
        [Fact]
        public void CheckAssembly_ReturnsRegistration()
        {
            var path = Environment.CurrentDirectory + "\\" + "Steeltoe.Stream.TestBinder.dll";

            BinderRegistration reg = BinderConfigurer.CheckAssembly(path);
            Assert.NotNull(reg);
            Assert.Equal("testbinder", reg.Name);
            Assert.Contains("Steeltoe.Stream.TestBinder.Startup", reg.ConfigureType);
            Assert.Contains("Steeltoe.Stream.TestBinder", reg.ConfigureType);
        }

        [Fact]
        public void AddBinders_AddsRegistration()
        {
            var path = Environment.CurrentDirectory;
            List<BinderRegistration> regs = BinderConfigurer.AddBinders(path);
            Assert.NotNull(regs);
            Assert.Single(regs);
            Assert.Equal("testbinder", regs[0].Name);
            Assert.Contains("Steeltoe.Stream.TestBinder.Startup", regs[0].ConfigureType);
            Assert.Contains("Steeltoe.Stream.TestBinder", regs[0].ConfigureType);
        }

        [Fact]
        public void FindConfigureType_FindsType()
        {
            var path = Environment.CurrentDirectory + "\\" + "Steeltoe.Stream.TestBinder.dll";

            BinderRegistration reg = BinderConfigurer.CheckAssembly(path);
            Assert.NotNull(reg);
            var configType = BinderConfigurer.FindConfigureType(reg.ConfigureType);
            Assert.NotNull(configType);
            Assert.Equal("Startup", configType.Name);
        }

        [Fact]
        public void FindConstructor_FindsConstructor()
        {
            var path = Environment.CurrentDirectory + "\\" + "Steeltoe.Stream.TestBinder.dll";

            BinderRegistration reg = BinderConfigurer.CheckAssembly(path);
            Assert.NotNull(reg);
            var configType = BinderConfigurer.FindConfigureType(reg.ConfigureType);
            Assert.NotNull(configType);
            var constructor = BinderConfigurer.FindConstructor(configType);
            Assert.NotNull(constructor);
        }

        [Fact]
        public void FindConfigureServicesMethod_FindsMethod()
        {
            var path = Environment.CurrentDirectory + "\\" + "Steeltoe.Stream.TestBinder.dll";

            BinderRegistration reg = BinderConfigurer.CheckAssembly(path);
            Assert.NotNull(reg);
            var configType = BinderConfigurer.FindConfigureType(reg.ConfigureType);
            Assert.NotNull(configType);
            var method = BinderConfigurer.FindConfigureServicesMethod(configType);
            Assert.NotNull(method);
        }

        [Fact]
        public void ConfigureBinders_InvokesConfigureServics_AddsBinderToServicesCollection()
        {
            var path = Environment.CurrentDirectory;
            List<BinderRegistration> regs = BinderConfigurer.AddBinders(path);
            Assert.NotNull(regs);
            Assert.Single(regs);
            ServiceCollection services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder().Build();

            BinderConfigurer.ConfigureBinders(services, config, regs);
            var provider = services.BuildServiceProvider();
            var binder = provider.GetService<IBinder>();
            Assert.NotNull(binder);
            Assert.Equal("testbinder", binder.Name);
        }
    }
}
