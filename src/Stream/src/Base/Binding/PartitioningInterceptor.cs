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

using Steeltoe.Integration.Support;
using Steeltoe.Messaging;
using Steeltoe.Messaging.Support;
using Steeltoe.Stream.Binder;
using Steeltoe.Stream.Config;
using System.Threading.Tasks;

namespace Steeltoe.Stream.Binding
{
    public class PartitioningInterceptor : AbstractChannelInterceptor
    {
        private readonly BindingOptions _bindingOptions;

        private readonly PartitionHandler _partitionHandler;
        private readonly IMessageBuilderFactory _messageBuilderFactory = new MutableMessageBuilderFactory();

        public PartitioningInterceptor(BindingOptions bindingOptions, IPartitionKeyExtractorStrategy partitionKeyExtractorStrategy, IPartitionSelectorStrategy partitionSelectorStrategy)
        {
            _bindingOptions = bindingOptions;
            _partitionHandler = new PartitionHandler(
                    null, // TODO: ExpressionUtils.createStandardEvaluationContext(MessageConverterConfigurer.this.beanFactory),
                    _bindingOptions.Producer,
                    partitionKeyExtractorStrategy,
                    partitionSelectorStrategy);
        }

        public int PartitionCount
        {
            get { return _partitionHandler.PartitionCount; }
            set { _partitionHandler.PartitionCount = value; }
        }

        public override Task<IMessage> PreSendAsync(IMessage message, IMessageChannel channel)
        {
            IMessage<object> objMessage = (IMessage<object>)message;

            if (!message.Headers.ContainsKey(BinderHeaders.PARTITION_OVERRIDE))
            {
                int partition = _partitionHandler.DeterminePartition(message);
                return Task.FromResult<IMessage>(_messageBuilderFactory
                        .FromMessage(objMessage)
                        .SetHeader(BinderHeaders.PARTITION_HEADER, partition).Build());
            }
            else
            {
                return Task.FromResult<IMessage>(_messageBuilderFactory
                        .FromMessage(objMessage)
                        .SetHeader(BinderHeaders.PARTITION_HEADER, message.Headers[BinderHeaders.PARTITION_OVERRIDE])
                        .RemoveHeader(BinderHeaders.PARTITION_OVERRIDE).Build());
            }
        }
    }
}
