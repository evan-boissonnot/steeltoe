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
using Microsoft.Extensions.Options;
using System;

namespace Steeltoe.Stream.Config
{
    public class BindingServiceConfigureOptions : ConfigureNamedOptions<BindingServiceOptions>
    {
        private const string PREFIX = "spring:cloud:stream";
        private IConfiguration _config;

        public BindingServiceConfigureOptions(string name, IConfiguration config)
            : base(name, null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        public override void Configure(string name, BindingServiceOptions options)
        {
            var section = _config.GetSection(PREFIX);
            if (Name == null || name == Name)
            {
                section.Bind(options);
                options.Configuration = section;
            }
        }
    }
}
