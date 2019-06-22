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
using Microsoft.Extensions.Options;
using Steeltoe.Stream.Binder;
using Steeltoe.Stream.Binding;
using Steeltoe.Stream.Config;
using Steeltoe.Stream.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Steeltoe.Stream.Extensions
{
    public static class EnableBindingsServiceCollection
    {
        public static IServiceCollection AddProcessorBinding(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddStreamBinding<IProcessor>(config);
        }

        public static IServiceCollection AddSinkBinding(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddStreamBinding<ISink>(config);
        }

        public static IServiceCollection AddSourceBinding(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddStreamBinding<ISource>(config);
        }

        public static IServiceCollection AddStreamBinding<T1>(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddStreamBinding(config, typeof(T1));
        }

        public static IServiceCollection AddStreamBinding<T1, T2>(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddStreamBinding(config, typeof(T1), typeof(T2));
        }

        public static IServiceCollection AddStreamBinding<T1, T2, T3>(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddStreamBinding(config, typeof(T1), typeof(T2), typeof(T3));
        }

        public static IServiceCollection AddStreamBinding(this IServiceCollection services, IConfiguration config, params Type[] bindings)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (bindings == null || bindings.Length == 0)
            {
                throw new ArgumentException("Must provide one or more bindings");
            }

            // Add core stream services to container
            services.AddStreamServices(config);

            // Add all the bindings to container
            services.AddBindingsToContainer(bindings);

            return services;
        }

        internal static void AddBindingsToContainer(this IServiceCollection services, Type[] bindings)
        {
            foreach (Type binding in bindings)
            {
                // Validate binding interface
                if (!binding.IsInterface || !binding.IsPublic || binding.IsGenericType)
                {
                    throw new ArgumentException($"Binding {binding} incorrectly defined");
                }

                // Add the binding to container
                services.AddBindingToContainer(binding);

                // Add all the channels from binding to container
                services.AddChannelsToContainer(binding);
            }
        }

        internal static void AddBindingToContainer(this IServiceCollection services, Type binding)
        {
            // Add a IBindable for this binding
            services.AddSingleton<IBindable>((p) =>
            {
                var bindingService = p.GetRequiredService<IBindingService>();
                IEnumerable<IBindingTargetFactory> bindingTargetFactories = p.GetServices<IBindingTargetFactory>();
                return new BindableProxyFactory(binding, bindingService, bindingTargetFactories);
            });

            // Add Binding
            services.AddSingleton(binding, (p) =>
            {
                // Find the bindabe for this binding
                IEnumerable<IBindable> bindables = p.GetServices<IBindable>();
                var bindable = bindables.SingleOrDefault((b) => b.Binding == binding);
                if (bindable == null)
                {
                    throw new InvalidOperationException("Unable to find bindable for binding");
                }
                return BindableProxyGenerator.CreateProxy((BindableProxyFactory)bindable);
            });
        }

        internal static void AddChannelsToContainer(this IServiceCollection services, Type binding)
        {
            var channels = BindingHelpers.CollectChannels(binding);
            foreach (var chan in channels.Values)
            {
                // Add channel defined in binding
                services.AddSingleton(chan.ChannelType, (p) =>
                {
                    var impl = p.GetRequiredService(binding);
                    return chan.FactoryMethod.Invoke(impl, new object[0]);
                });
            }
        }

        internal static void AddStreamServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions();
            services.AddSingleton<IOptionsChangeTokenSource<BindingServiceOptions>>(new ConfigurationChangeTokenSource<BindingServiceOptions>(Options.DefaultName, config));
            services.AddSingleton<IConfigureOptions<BindingServiceOptions>>(new BindingServiceConfigureOptions(Options.DefaultName, config));

            services.AddBinders(config);

            services.AddSingleton<IBinderFactory, DefaultBinderFactory>();
            services.AddSingleton<IBindingService, BindingService>();
            services.AddSingleton<IMessageChannelConfigurer, MessageConverterConfigurer>();  // TODO: Others to add?
            services.AddSingleton<IBindingTargetFactory, MessageSourceBindingTargetFactory>();
            services.AddSingleton<IBindingTargetFactory, SubscribableChannelBindingTargetFactory>();
        }
    }
}
