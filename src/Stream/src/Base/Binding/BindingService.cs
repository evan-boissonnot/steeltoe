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

using Microsoft.Extensions.Options;
using Steeltoe.Stream.Binder;
using Steeltoe.Stream.Config;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Steeltoe.Stream.Binding
{
    public class BindingService : IBindingService
    {
        private IBinderFactory binderFactory;
        private IOptionsMonitor<BindingServiceOptions> optionsMonitor;
        private IDictionary<string, IBinding> producerBindings = new Dictionary<string, IBinding>();
        private IDictionary<string, List<IBinding>> consumerBindings = new Dictionary<string, List<IBinding>>();

        private BindingServiceOptions Options
        {
            get
            {
                return optionsMonitor.CurrentValue;
            }
        }

        public BindingService(IOptionsMonitor<BindingServiceOptions> optionsMonitor, IBinderFactory binderFactory)
        {
            this.optionsMonitor = optionsMonitor;
            this.binderFactory = binderFactory;
        }

        public async Task<ICollection<IBinding>> BindConsumer(object inputChan, string name)
        {
            List<IBinding> bindings = new List<IBinding>();
            IBinder binder = GetBinder(name, inputChan.GetType());
            ConsumerOptions consumerOptions = Options.GetConsumerOptions(name);

            string bindingTarget = Options.GetBindingDestination(name);
            if (consumerOptions.Multiplex)
            {
                bindings.Add(await DoBindConsumer(inputChan, name, binder, consumerOptions, bindingTarget));
            }
            else
            {
                var bindingTargets = bindingTarget == null ? new string[0] : bindingTarget.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var target in bindingTargets)
                {
                    IBinding binding = null;
                    if (inputChan is IPollableMessageSource)
                    {
                        binding = await DoBindPollableConsumer(inputChan, name, binder, consumerOptions, bindingTarget);
                    }
                    else
                    {
                        binding = await DoBindConsumer(inputChan, name, binder, consumerOptions, bindingTarget);
                    }

                    bindings.Add(binding);
                }
            }

            consumerBindings[name] = new List<IBinding>(bindings);
            return bindings;
        }

        public async Task<IBinding> BindProducer(object outputChan, string name)
        {
            string bindingTarget = Options.GetBindingDestination(name);
            IBinder binder = GetBinder(name, outputChan.GetType());
            ProducerOptions producerOptions = Options.GetProducerOptions(name);
            IBinding binding = await DoBindProducer(outputChan, bindingTarget, binder, producerOptions);
            producerBindings[name] = binding;
            return binding;
        }

        public async Task<IBinding> DoBindConsumer(object inputChan, string name, IBinder binder, ConsumerOptions consumerOptions, string bindingTarget)
        {
            if (Options.BindingRetryInterval <= 0)
            {
                return await binder.BindConsumer(bindingTarget, Options.GetGroup(name), inputChan, consumerOptions);
            }
            else
            {
                return await DoBindConsumerWithRetry(inputChan, name, binder, consumerOptions, bindingTarget);
            }
        }

        public async Task<IBinding> DoBindConsumerWithRetry(object inputChan, string name, IBinder binder, ConsumerOptions consumerOptions, string bindingTarget)
        {
            // TODO: Java code never stops retrying the bind
            do
            {
                try
                {
                    return await binder.BindConsumer(bindingTarget, Options.GetGroup(name), inputChan, consumerOptions);
                }
                catch (Exception)
                {
                    // log
                    Thread.Sleep(Options.BindingRetryInterval * 1000);
                }
            }
            while (true);
        }

        public async Task<IBinding> DoBindProducer(object outputChan, string bindingTarget, IBinder binder, ProducerOptions producerOptions)
        {
            if (Options.BindingRetryInterval <= 0)
            {
                return await binder.BindProducer(bindingTarget, outputChan, producerOptions);
            }
            else
            {
                return await DoBindProducerWithRetry(outputChan, bindingTarget, binder, producerOptions);
            }
        }

        public async Task<IBinding> DoBindProducerWithRetry(object outputChan, string bindingTarget, IBinder binder, ProducerOptions producerOptions)
        {
            // TODO: Java code never stops retrying the bind
            do
            {
                try
                {
                    return await binder.BindProducer(bindingTarget, outputChan, producerOptions);
                }
                catch (Exception)
                {
                    // log
                    Thread.Sleep(Options.BindingRetryInterval * 1000);
                }
            }
            while (true);
        }

        public async Task<IBinding> DoBindPollableConsumer(object inputChan, string name, IBinder binder, ConsumerOptions consumerOptions, string bindingTarget)
        {
            var pollableBinder = (IPollableConsumerBinder)binder;

            if (Options.BindingRetryInterval <= 0)
            {
                return await pollableBinder.BindPollableConsumer(bindingTarget, Options.GetGroup(name), inputChan, consumerOptions);
            }
            else
            {
                return await DoBindPollableConsumerWithRetry(inputChan, name, pollableBinder, consumerOptions, bindingTarget);
            }
        }

        public async Task<IBinding> DoBindPollableConsumerWithRetry(object inputChan, string name, IPollableConsumerBinder binder, ConsumerOptions consumerOptions, string bindingTarget)
        {
            // TODO: Java code never stops retrying the bind
            do
            {
                try
                {
                    return await binder.BindPollableConsumer(bindingTarget, Options.GetGroup(name), inputChan, consumerOptions);
                }
                catch (Exception)
                {
                    // log
                    Thread.Sleep(Options.BindingRetryInterval * 1000);
                }
            }
            while (true);
        }

        protected IBinder GetBinder(string channelName, Type bindableType)
        {
            string configName = Options.GetBinder(channelName);
            return binderFactory.GetBinder(configName, bindableType);
        }
    }
}
