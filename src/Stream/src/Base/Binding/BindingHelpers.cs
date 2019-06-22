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

using Steeltoe.Stream.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Steeltoe.Stream.Binding
{
    public static class BindingHelpers
    {
        public static IDictionary<string, Channel> CollectChannels(Type binding)
        {
            IDictionary<string, Channel> channels = new Dictionary<string, Channel>();
            CollectFromProperties(binding, channels);
            CollectFromMethods(binding, channels);
            return channels;
        }

        internal static void CollectFromProperties(Type binding, IDictionary<string, Channel> channels)
        {
            var infos = binding.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var info in infos)
            {
                MethodInfo getMethod = info.GetGetMethod();
                if (getMethod != null)
                {
                    if (info.GetCustomAttribute(typeof(InputAttribute)) is InputAttribute attribute)
                    {
                        var chan = new Channel()
                        {
                            IsInput = true,
                            Name = attribute.Name ?? info.Name,
                            ChannelType = getMethod.ReturnType,
                            FactoryMethod = getMethod
                        };

                        AddChannel(chan, channels);
                    }

                    if (info.GetCustomAttribute(typeof(OutputAttribute)) is OutputAttribute attribute2)
                    {
                        var chan = new Channel()
                        {
                            IsInput = false,
                            Name = attribute2.Name ?? info.Name,
                            ChannelType = getMethod.ReturnType,
                            FactoryMethod = getMethod
                        };

                        AddChannel(chan, channels);
                    }
                }
            }

            foreach (Type iface in binding.GetInterfaces())
            {
                CollectFromProperties(iface, channels);
            }
        }

        internal static void CollectFromMethods(Type binding, IDictionary<string, Channel> channels)
        {
            var meths = binding.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var meth in meths)
            {
                if (meth.GetCustomAttribute(typeof(InputAttribute)) is InputAttribute attribute)
                {
                    var chan = new Channel()
                    {
                        IsInput = true,
                        Name = attribute.Name ?? meth.Name,
                        ChannelType = meth.ReturnType,
                        FactoryMethod = meth
                    };

                    AddChannel(chan, channels);
                }

                if (meth.GetCustomAttribute(typeof(OutputAttribute)) is OutputAttribute attribute2)
                {
                    var chan = new Channel()
                    {
                        IsInput = false,
                        Name = attribute2.Name ?? meth.Name,
                        ChannelType = meth.ReturnType,
                        FactoryMethod = meth
                    };

                    AddChannel(chan, channels);
                }
            }

            foreach (Type iface in binding.GetInterfaces())
            {
                CollectFromMethods(iface, channels);
            }
        }

        internal static void AddChannel(Channel chan, IDictionary<string, Channel> channels)
        {
            if (channels.ContainsKey(chan.Name))
            {
                throw new InvalidOperationException("Duplicate channel with name: " + chan.Name);
            }

            channels.Add(chan.Name, chan);
        }
    }

    public struct Channel
    {
        public bool IsInput;
        public string Name;
        public Type ChannelType;
        public MethodInfo FactoryMethod;
    }
}