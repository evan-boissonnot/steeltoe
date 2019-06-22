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
using Steeltoe.Stream.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Steeltoe.Stream.Binder
{
    public class DefaultBinderFactory : IBinderFactory
    {
        private readonly IOptionsMonitor<BindingServiceOptions> optionsMonitor;
        private readonly IDictionary<string, IBinder> binders = new Dictionary<string, IBinder>();

        private BindingServiceOptions Options
        {
            get
            {
                return optionsMonitor.CurrentValue;
            }
        }

        // TODO: Add Listeners
        public DefaultBinderFactory(IOptionsMonitor<BindingServiceOptions> optionsMonitor, IEnumerable<IBinder> binders)
        {
            this.optionsMonitor = optionsMonitor;
            foreach (var binder in binders)
            {
                this.binders[binder.Name] = binder;
            }
        }

        public IBinder GetBinder(string name, Type bindingTargetType)
        {
            string binderName = !string.IsNullOrEmpty(name) ? name : Options.DefaultBinder;
            IBinder result = null;
            if (!string.IsNullOrEmpty(binderName) && this.binders.ContainsKey(binderName))
            {
                result = this.binders[binderName];
            }
            else if (binders.Count == 1)
            {
                result = binders.Values.Single<IBinder>();
            }
            else if (binders.Count > 1)
            {
                throw new InvalidOperationException("Multiple binders are available, however neither default nor per-destination binder name is provided.");
            }

            if (result == null)
            {
                if (string.IsNullOrEmpty(binderName))
                {
                    throw new InvalidOperationException("A default binder has been requested, but there is no binder available");
                }

                throw new InvalidOperationException("Unable to find Binder with name: " + binderName);
            }

            return result;
        }
    }
}
