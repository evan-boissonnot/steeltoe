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

namespace Steeltoe.Stream.Config
{
    public class ConsumerOptions : IConsumerOptions
    {
        public bool AutoStartup { get; set; } = true;

        public int Concurrency { get; set; } = 1;

        public bool Partitioned { get; set; } = false;

        public int InstanceCount { get; set; } = -1;

        public int InstanceIndex { get; set; } = -1;

        public int MaxAttempts { get; set; } = 3;

        public int BackOffInitialInterval { get; set; } = 1000;

        public int BackOffMaxInterval { get; set; } = 10000;

        public double BackOffMultiplier { get; set; } = 2.0;

        public bool DefaultRetryable { get; set; } = true;

        // private Map<Class<? extends Throwable>, Boolean> retryableExceptions = new LinkedHashMap<>();
        public HeaderMode HeaderMode { get; set; }

        public bool UseNativeDecoding { get; set; }

        public bool Multiplex { get; set; }
    }
}
