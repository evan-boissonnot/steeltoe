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
using System;
using System.Collections.Generic;

namespace Steeltoe.Stream.Config
{
    public class BindingServiceOptions
    {
        internal IConfiguration _config;

        internal IConfiguration Configuration
        {
            get
            {
                return _config;
            }

            set
            {
                _config = value;
                Initialize();
            }
        }

        internal void Initialize()
        {
            Configuration.Bind(this);

            // Build up bindings
            Bindings.Clear();
            var bindings = Configuration.GetSection("bindings").GetChildren();
            foreach (var section in bindings)
            {
                string bindingName = section.Key;
                var bindingsConfig = Configuration.GetSection("bindings:" + bindingName);
                var options = new BindingOptions();
                bindingsConfig.Bind(options);
                Bindings[bindingName] = options;
            }

            Binders.Clear();
            var binders = Configuration.GetSection("binders").GetChildren();
            foreach (var section in binders)
            {
                string binderName = section.Key;
                var bindersConfig = Configuration.GetSection("binders:" + binderName);
                var options = new BinderOptions();
                bindersConfig.Bind(options);

                if (options.Type == null)
                {
                    options.Type = binderName;
                }

                Binders[binderName] = options;
            }
        }

        public BindingServiceOptions()
        {
        }

        // @Value("${INSTANCE_INDEX:${CF_INSTANCE_INDEX:0}}")
        public int InstanceIndex { get; set; }

        public int InstanceCount { get; set; } = 1;

        public string DefaultBinder { get; set; }

        public int BindingRetryInterval { get; set; } = 30;

        public bool OverrideCloudConnectors { get; set; } = false;

        public IList<string> DynamicDestinations { get; set; } = new List<string>();

        public IDictionary<string, BinderOptions> Binders { get; set; } = new Dictionary<string, BinderOptions>();

        public IDictionary<string, BindingOptions> Bindings { get; set; } = new Dictionary<string, BindingOptions>();

        public string GetBinder(string name)
        {
            return GetBindingOptions(name).Binder;
        }

        public IDictionary<string, object> AsDictionary()
        {
            IDictionary<string, object> options = new Dictionary<string, object>
            {
                ["instanceIndex"] = InstanceIndex,
                ["instanceCount"] = InstanceCount,
                ["defaultBinder"] = DefaultBinder,

                ["dynamicDestinations"] = DynamicDestinations
            };
            foreach (var entry in Bindings)
            {
                options.Add(entry.Key, entry.Value);
            }

            foreach (var entry in Binders)
            {
                options.Add(entry.Key, entry.Value);
            }

            return options;
        }

        public BindingOptions GetBindingOptions(string name)
        {
            MakeBindingIfNecessary(name);
            BindingOptions options = Bindings[name];
            if (options.Destination == null)
            {
                options.Destination = name;
            }

            return options;
        }

        public ConsumerOptions GetConsumerOptions(string inputBindingName)
        {
            if (inputBindingName == null)
            {
                throw new ArgumentNullException(nameof(inputBindingName));
            }

            BindingOptions bindingOptions = GetBindingOptions(inputBindingName);
            ConsumerOptions consumerOptions = bindingOptions.Consumer;
            if (consumerOptions == null)
            {
                consumerOptions = new ConsumerOptions();
                bindingOptions.Consumer = consumerOptions;
            }

            // propagate instance count and instance index if not already set
            if (consumerOptions.InstanceCount < 0)
            {
                consumerOptions.InstanceCount = InstanceCount;
            }

            if (consumerOptions.InstanceIndex < 0)
            {
                consumerOptions.InstanceIndex = InstanceIndex;
            }

            return consumerOptions;
        }

        public ProducerOptions GetProducerOptions(string outputBindingName)
        {
            if (outputBindingName == null)
            {
                throw new ArgumentNullException(nameof(outputBindingName));
            }

            BindingOptions bindingOptions = GetBindingOptions(outputBindingName);
            ProducerOptions producerOptions = bindingOptions.Producer;
            if (producerOptions == null)
            {
                producerOptions = new ProducerOptions();
                bindingOptions.Producer = producerOptions;
            }

            return producerOptions;
        }

        public string GetGroup(string bindingName)
        {
            return GetBindingOptions(bindingName).Group;
        }

        public string GetBindingDestination(string bindingName)
        {
            return GetBindingOptions(bindingName).Destination;
        }

        public void UpdateProducerOptions(string bindingName, ProducerOptions producerOptions)
        {
            if (Bindings.ContainsKey(bindingName))
            {
                Bindings[bindingName].Producer = producerOptions;
            }
        }

        internal void MakeBindingIfNecessary(string bindingName)
        {
            if (!Bindings.ContainsKey(bindingName))
            {
                BindToDefaults(bindingName);
            }
        }

        internal void BindToDefaults(string binding)
        {
            BindingOptions options = new BindingOptions();
            var defaultConfig = Configuration.GetSection("default");
            defaultConfig.Bind(options);
            Bindings[binding] = options;
        }
    }
}
