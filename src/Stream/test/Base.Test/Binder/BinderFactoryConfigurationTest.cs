﻿// Copyright 2017 the original author or authors.
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
using Steeltoe.Messaging;
using System;
using Xunit;

namespace Steeltoe.Stream.Binder
{
    public class BinderFactoryConfigurationTest : AbstractTest
    {
        [Fact]
        public void LoadBinderTypeRegistryWithOneBinder()
        {
            var searchDirectories = GetSearchDirectories("StubBinder1");
            var provider = CreateStreamsContainer(searchDirectories, "spring:cloud:stream:defaultBinder=binder1")
                            .BuildServiceProvider();
            var typeRegistry = provider.GetService<IBinderTypeRegistry>();
            Assert.Equal(1, typeRegistry.GetAll().Count);

            Assert.Contains("binder1", typeRegistry.GetAll().Keys);
            var factory = provider.GetService<IBinderFactory>();
            Assert.NotNull(factory);
            var binder1 = factory.GetBinder("binder1", typeof(IMessageChannel));
            Assert.NotNull(binder1);

            var defaultBinder = factory.GetBinder(null, typeof(IMessageChannel));
            Assert.Same(binder1, defaultBinder);
        }

        [Fact]
        public void LoadBinderTypeRegistryWithOneBinderWithBinderSpecificConfigurationData()
        {
            var searchDirectories = GetSearchDirectories("StubBinder1");
            var provider = CreateStreamsContainer(searchDirectories, "binder1:name=foo")
                            .BuildServiceProvider();

            var factory = provider.GetService<IBinderFactory>();

            _ = factory.GetBinder("binder1", typeof(IMessageChannel));

            var config = provider.GetService<IConfiguration>().GetSection("binder1");
            Assert.Equal("foobar", config["name"]);
        }

        [Fact]
        public void LoadBinderTypeRegistryWithTwoBinders()
        {
            var searchDirectories = GetSearchDirectories("StubBinder1", "StubBinder2");
            var provider = CreateStreamsContainer(searchDirectories)
                            .BuildServiceProvider();

            var typeRegistry = provider.GetService<IBinderTypeRegistry>();

            Assert.NotNull(typeRegistry);
            Assert.Equal(2, typeRegistry.GetAll().Count);
            Assert.Contains("binder1", typeRegistry.GetAll().Keys);
            Assert.Contains("binder2", typeRegistry.GetAll().Keys);
            var factory = provider.GetService<IBinderFactory>();
            Assert.NotNull(factory);
            Assert.Throws<InvalidOperationException>(() => factory.GetBinder(null, typeof(IMessageChannel)));

            var binder1 = factory.GetBinder("binder1", typeof(IMessageChannel));
            Assert.NotNull(binder1);
            var binder2 = factory.GetBinder("binder2", typeof(IMessageChannel));
            Assert.NotNull(binder1);
            Assert.NotSame(binder1, binder2);
        }

        [Fact]
        public void LoadBinderTypeRegistryWithCustomNonDefaultCandidate()
        {
            var searchDirectories = GetSearchDirectories("StubBinder1", "TestBinder");
            var provider = CreateStreamsContainer(
                searchDirectories,
                "spring.cloud.stream.binders.custom.configureclass=" + "Steeltoe.Stream.TestBinder.Startup, Steeltoe.Stream.TestBinder",
                "spring.cloud.stream.binders.custom.defaultCandidate=false",
                "spring.cloud.stream.binders.custom.inheritEnvironment=false",
                "spring.cloud.stream.defaultbinder=binder1")
                .BuildServiceProvider();
            var typeRegistry = provider.GetService<IBinderTypeRegistry>();
            Assert.NotNull(typeRegistry);
            Assert.Equal(2, typeRegistry.GetAll().Count);
            Assert.Contains("binder1", typeRegistry.GetAll().Keys);
            var factory = provider.GetService<IBinderFactory>();
            Assert.NotNull(factory);

            var defaultBinder = factory.GetBinder(null, typeof(IMessageChannel));
            Assert.NotNull(defaultBinder);
            Assert.Equal("binder1", defaultBinder.Name);

            var binder1 = factory.GetBinder("binder1", typeof(IMessageChannel));
            Assert.NotNull(binder1);
            Assert.Equal("binder1", binder1.Name);

            Assert.Same(binder1, defaultBinder);
        }

        [Fact]
        public void LoadDefaultBinderWithTwoBinders()
        {
            var searchDirectories = GetSearchDirectories("StubBinder1", "StubBinder2");
            var provider = CreateStreamsContainer(searchDirectories, "spring.cloud.stream.defaultBinder=binder2")
                            .BuildServiceProvider();
            var typeRegistry = provider.GetService<IBinderTypeRegistry>();
            Assert.NotNull(typeRegistry);
            Assert.Equal(2, typeRegistry.GetAll().Count);
            Assert.Contains("binder1", typeRegistry.GetAll().Keys);
            Assert.Contains("binder2", typeRegistry.GetAll().Keys);

            var factory = provider.GetService<IBinderFactory>();
            Assert.NotNull(factory);

            var binder1 = factory.GetBinder("binder1", typeof(IMessageChannel));
            Assert.Equal("binder1", binder1.Name);
            var binder2 = factory.GetBinder("binder2", typeof(IMessageChannel));
            Assert.Equal("binder2", binder2.Name);
            var defaultBinder = factory.GetBinder(null, typeof(IMessageChannel));
            Assert.Same(defaultBinder, binder2);
        }
    }
}
