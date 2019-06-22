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
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Stream.Config
{
    public class BindingServiceOptionsTest
    {
        [Fact]
        public void Initialize_ConfiguresOptionsCorrectly()
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "spring:cloud:stream:instanceCount", "100" },
                { "spring:cloud:stream:instanceIndex", "1" },
                { "spring:cloud:stream:defaultBinder", "defaultBinder" },
                { "spring:cloud:stream:bindingRetryInterval", "500" },
                { "spring:cloud:stream:default:destination", "destination" },
                { "spring:cloud:stream:default:group", "group" },
                { "spring:cloud:stream:default:contentType", "contentType" },
                { "spring:cloud:stream:default:binder", "binder" },
                { "spring:cloud:stream:bindings:input:destination", "inputdestination" },
                { "spring:cloud:stream:bindings:input:group", "inputgroup" },
                { "spring:cloud:stream:bindings:input:contentType", "inputcontentType" },
                { "spring:cloud:stream:bindings:input:binder", "inputbinder" },
                { "spring:cloud:stream:bindings:input:consumer:autoStartup", "false" },
                { "spring:cloud:stream:bindings:input:consumer:concurrency", "10" },
                { "spring:cloud:stream:bindings:input:consumer:partitioned", "true" },
                { "spring:cloud:stream:bindings:input:consumer:headerMode", "headers" },
                { "spring:cloud:stream:bindings:input:consumer:maxAttempts", "10" },
                { "spring:cloud:stream:bindings:input:consumer:backOffInitialInterval", "10" },
                { "spring:cloud:stream:bindings:input:consumer:backOffMaxInterval", "10" },
                { "spring:cloud:stream:bindings:input:consumer:backOffMultiplier", "5.0" },
                { "spring:cloud:stream:bindings:input:consumer:defaultRetryable", "false" },
                { "spring:cloud:stream:bindings:input:consumer:instanceIndex", "10" },
                { "spring:cloud:stream:bindings:input:consumer:instanceCount", "10" },
                { "spring:cloud:stream:bindings:input:consumer:retryableExceptions", "notused" },
                { "spring:cloud:stream:bindings:input:consumer:useNativeDecoding", "true" },
                { "spring:cloud:stream:bindings:output:destination", "outputdestination" },
                { "spring:cloud:stream:bindings:output:group", "outputgroup" },
                { "spring:cloud:stream:bindings:output:contentType", "outputcontentType" },
                { "spring:cloud:stream:bindings:output:binder", "outputbinder" },
                { "spring:cloud:stream:bindings:output:producer:autoStartup", "false" },
                { "spring:cloud:stream:bindings:output:producer:partitionKeyExpression", "partitionKeyExpression" },
                { "spring:cloud:stream:bindings:output:producer:partitionSelectorExpression", "partitionSelectorExpression" },
                { "spring:cloud:stream:bindings:output:producer:partitionKeyExtractorName", "partitionKeyExtractorName" },
                { "spring:cloud:stream:bindings:output:producer:partitionSelectorName", "partitionSelectorName" },
                { "spring:cloud:stream:bindings:output:producer:partitionCount", "10" },
                { "spring:cloud:stream:bindings:output:producer:requiredGroups", "requiredGroups" },
                { "spring:cloud:stream:bindings:output:producer:headerMode", "headers" },
                { "spring:cloud:stream:bindings:output:producer:useNativeEncoding", "true" },
                { "spring:cloud:stream:bindings:output:producer:errorChannelEnabled", "true" },
                { "spring:cloud:stream:binders:foobar:inheritEnvironment", "false" },
                { "spring:cloud:stream:binders:foobar:defaultCandidate", "false" },
                { "spring:cloud:stream:binders:foobar:environment:key1", "value1" },
                { "spring:cloud:stream:binders:foobar:environment:key2", "value2" },
            });

            var config = builder.Build().GetSection("spring:cloud:stream");
            var options = new BindingServiceOptions
            {
                Configuration = config
            };

            var input = options.GetBindingOptions("input");
            Assert.NotNull(input);

            // TODO: Verify all values
            var output = options.GetBindingOptions("output");
            Assert.NotNull(output);

            // TODO: Verify all values
            var binder = options.Binders["foobar"];
            Assert.NotNull(binder);

            // TODO: Verify all values
        }
    }
}
