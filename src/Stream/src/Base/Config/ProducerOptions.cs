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
    public class ProducerOptions : IProducerOptions
    {
        public bool AutoStartup { get; set; } = true;

        public string PartitionKeyExpression { get; set; }

        // public string PartitionKeyExtractorName { get; set; }

        // public string PartitionSelectorName { get; set; }
        public string PartitionSelectorExpression { get; set; }

        public int PartitionCount { get; set; } = 1;

        public string[] RequiredGroups { get; set; } = new string[] { };

        public HeaderMode HeaderMode { get; set; }

        public bool UseNativeEncoding { get; set; } = false;

        public bool ErrorChannelEnabled { get; set; } = false;

        public bool IsPartitioned
        {
            get
            {
                {
                    return this.PartitionKeyExpression != null;

                    // || this.partitionKeyExtractorName != null
                    //            || this.partitionKeyExtractorClass != null;
                }
            }
        }
    }
}
