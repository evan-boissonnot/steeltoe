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

using Steeltoe.Stream.Binder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using System.Threading.Tasks;

namespace Steeltoe.Stream.Binding
{
    public class BindableProxyFactory : IBindable, IBindableProxyFactory
    {
        private IBindingService _bindingService;
        private IList<IBindingTargetFactory> _bindingTargetFactories;
        private IDictionary<string, Channel> _bindingChannels;
        private IDictionary<string, object> _inputChannels = new Dictionary<string, object>();
        private IDictionary<string, object> _outputChannels = new Dictionary<string, object>();
        private ConcurrentDictionary<MethodInfo, object> _cache = new ConcurrentDictionary<MethodInfo, object>();

        public Type Binding { get; }

        public object Invoke(MethodInfo invocation)
        {
            if (_cache.TryGetValue(invocation, out object channel))
            {
                return channel;
            }

            var chan = _bindingChannels.Values.SingleOrDefault((c) => c.FactoryMethod == invocation);
            if (chan.Name == null)
            {
                return null;
            }

            if (chan.IsInput)
            {
                return _cache.GetOrAdd(invocation, _inputChannels[chan.Name]);
            }
            else
            {
                return _cache.GetOrAdd(invocation, _outputChannels[chan.Name]);
            }
        }

        public BindableProxyFactory(Type binding, IBindingService bindingService, IEnumerable<IBindingTargetFactory> bindingTargetFactories)
        {
            Binding = binding;
            _bindingService = bindingService;
            _bindingTargetFactories = bindingTargetFactories.ToList();
            Initialize();
        }

        public ICollection<string> Inputs => _inputChannels.Keys;

        public ICollection<string> Outputs => _outputChannels.Keys;

        public async Task<ICollection<IBinding>> CreateAndBindInputs()
        {
            List<IBinding> inputBindings = new List<IBinding>();
            foreach (var inputChan in _inputChannels)
            {
                var result = await _bindingService.BindConsumer(inputChan.Value, inputChan.Key);
                inputBindings.AddRange(result);
            }

            return inputBindings;
        }

        public Task<ICollection<IBinding>> CreateAndBindOutputs()
        {
            throw new NotImplementedException();
        }

        public Task UnbindInputs()
        {
            throw new NotImplementedException();
        }

        public Task UnbindOutputs()
        {
            throw new NotImplementedException();
        }

        internal void Initialize()
        {
            _bindingChannels = BindingHelpers.CollectChannels(Binding);
            foreach (var chan in _bindingChannels.Values)
            {
                var factory = FindChannelFactory(chan.ChannelType);
                if (chan.IsInput)
                {
                    _inputChannels.Add(chan.Name, factory.CreateInput(chan.Name));
                }
                else
                {
                    _outputChannels.Add(chan.Name, factory.CreateOutput(chan.Name));
                }
            }
        }

        internal IBindingTargetFactory FindChannelFactory(Type channelType)
        {
            IBindingTargetFactory result = null;

            foreach (var factory in _bindingTargetFactories)
            {
                if (factory.CanCreate(channelType))
                {
                    if (result == null)
                    {
                        result = factory;
                    }
                    else
                    {
                        throw new InvalidOperationException("Multiple factories found for binding target type: " + channelType);
                    }
                }
            }

            if (result == null)
            {
                throw new InvalidOperationException("No factory found for binding target type: " + channelType);
            }

            return result;
        }
    }
}
