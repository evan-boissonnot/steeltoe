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

using Steeltoe.Messaging;
using Steeltoe.Stream.Config;

namespace Steeltoe.Stream.Binder
{
    public class PartitionHandler
    {
        private readonly IEvaluationContext _evaluationContext;

        private readonly ProducerOptions _producerOptions;

        private readonly IPartitionKeyExtractorStrategy _partitionKeyExtractorStrategy;

        private readonly IPartitionSelectorStrategy _partitionSelectorStrategy;

        private volatile int _partitionCount;

        public PartitionHandler(
            IEvaluationContext evaluationContext,
                ProducerOptions options,
                IPartitionKeyExtractorStrategy partitionKeyExtractorStrategy,
                IPartitionSelectorStrategy partitionSelectorStrategy)
        {
            _evaluationContext = evaluationContext;
            _producerOptions = options;
            _partitionKeyExtractorStrategy = partitionKeyExtractorStrategy;
            _partitionSelectorStrategy = partitionSelectorStrategy;
            _partitionCount = _producerOptions.PartitionCount;
        }

        public int PartitionCount
        {
            get { return _partitionCount; }
            set { _partitionCount = value; }
        }

        public int DeterminePartition(IMessage message)
        {
            return -1;

            // Object key = extractKey(message);

            // int partition;
            //          if (this.producerProperties.getPartitionSelectorExpression() != null)
            //          {
            //              partition = this.producerProperties.getPartitionSelectorExpression()
            //                      .getValue(this.evaluationContext, key, Integer.class);
            // }
            // else {
            // partition = this.partitionSelectorStrategy.selectPartition(key,

            // this.partitionCount);
            //  }
            //// protection in case a user selector returns a negative.
            // return Math.abs(partition % this.partitionCount);
        }

        private object ExtractKey(IMessage message)
        {
            return null;

            // Object key = invokeKeyExtractor(message);
            // if (key == null && this.producerProperties.getPartitionKeyExpression() != null)
            // {
            //    key = this.producerProperties.getPartitionKeyExpression()
            //            .getValue(this.evaluationContext, message);
            // }
            // Assert.notNull(key, "Partition key cannot be null");

            // return key;
        }

        private object InvokeKeyExtractor(IMessage message)
        {
            return null;

            // if (this.partitionKeyExtractorStrategy != null)
            // {
            //    return this.partitionKeyExtractorStrategy.extractKey(message);
            // }
            // return null;
        }
    }
}
