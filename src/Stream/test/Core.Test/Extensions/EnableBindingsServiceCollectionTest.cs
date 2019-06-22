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
using Steeltoe.Messaging;
using Steeltoe.Stream.Messaging;
using System;
using System.Linq;
using Xunit;

namespace Steeltoe.Stream.Extensions
{
    public class EnableBindingsServiceCollectionTest
    {
        [Fact]
        public void AddStreamBinding_ThrowsOnNulls()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => EnableBindingsServiceCollection.AddStreamBinding(services, null));
            Assert.Throws<ArgumentNullException>(() => EnableBindingsServiceCollection.AddStreamBinding(new ServiceCollection(), null));
            Assert.Throws<ArgumentException>(() => EnableBindingsServiceCollection.AddStreamBinding(new ServiceCollection(), new ConfigurationBuilder().Build(), null));
            Assert.Throws<ArgumentException>(() => EnableBindingsServiceCollection.AddStreamBinding(new ServiceCollection(), new ConfigurationBuilder().Build(), new Type[0]));
        }

        [Fact]
        public void AddBindingsToContainer_ThrowsOnInvalidBindingType()
        {
            var container = new ServiceCollection();
            Assert.Throws<ArgumentException>(() => EnableBindingsServiceCollection.AddBindingsToContainer(container, new Type[] { typeof(EnableBindingsServiceCollection) }));
            Assert.Throws<ArgumentException>(() => EnableBindingsServiceCollection.AddBindingsToContainer(container, new Type[] { typeof(ITestGeneric<string>) }));
            Assert.Throws<ArgumentException>(() => EnableBindingsServiceCollection.AddBindingsToContainer(container, new Type[] { typeof(ITest) }));
        }

        [Fact]
        public void AddBindingsToContainer_AddsToContainer()
        {
            var container = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();
            container.AddStreamServices(config);
            container.AddBindingsToContainer(new Type[] { typeof(ISink), typeof(ISource) });
            var built = container.BuildServiceProvider();
            Assert.NotNull(built.GetService<ISink>());
            Assert.NotNull(built.GetService<ISource>());
        }

        [Fact]
        public void AddChannelsToContainer_AddsToContainer()
        {
            var container = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();
            container.AddStreamServices(config);
            container.AddBindingToContainer(typeof(ISink));
            container.AddChannelsToContainer(typeof(ISink));
            var built = container.BuildServiceProvider();
            Assert.NotNull(built.GetService<ISubscribableChannel>());

            container = new ServiceCollection();
            container.AddStreamServices(config);
            container.AddBindingToContainer(typeof(ISource));
            container.AddChannelsToContainer(typeof(ISource));
            built = container.BuildServiceProvider();
            Assert.NotNull(built.GetService<IMessageChannel>());

            container = new ServiceCollection();
            container.AddStreamServices(config);
            container.AddBindingToContainer(typeof(IProcessor));
            container.AddChannelsToContainer(typeof(IProcessor));
            built = container.BuildServiceProvider();
            Assert.NotNull(built.GetService<IMessageChannel>());
            Assert.NotNull(built.GetService<ISubscribableChannel>());
        }

        [Fact]
        public void AddChannels_CustomBinding_AddsExpected()
        {
            var container = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();
            container.AddStreamServices(config);
            container.AddBindingToContainer(typeof(IBarista));
            container.AddChannelsToContainer(typeof(IBarista));
            var built = container.BuildServiceProvider();
            Assert.NotNull(built.GetService<ISubscribableChannel>());
            var messageChannels = built.GetServices<IMessageChannel>().ToList();
            Assert.NotNull(messageChannels);
            Assert.Equal(2, messageChannels.Count);
        }

        public interface ITestGeneric<T>
        {
        }

        private interface ITest
        {
        }
    }
}
