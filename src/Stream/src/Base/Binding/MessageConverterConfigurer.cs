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
using Steeltoe.Integration.Channel;
using Steeltoe.Messaging;
using Steeltoe.Stream.Binder;
using Steeltoe.Stream.Config;
using Steeltoe.Stream.Converter;
using System;

namespace Steeltoe.Stream.Binding
{
    public class MessageConverterConfigurer : IMessageChannelConfigurer
    {
        private IOptionsMonitor<BindingServiceOptions> _optionsMonitor;
        private IMessageConverterFactory _messageConverterFactory;
        private IServiceProvider _services;

        private BindingServiceOptions Options
        {
            get
            {
                return _optionsMonitor.CurrentValue;
            }
        }

        public MessageConverterConfigurer(IOptionsMonitor<BindingServiceOptions> optionsMonitor, IServiceProvider services, IMessageConverterFactory messageConverterFactory)
        {
            this._optionsMonitor = optionsMonitor;
            this._services = services;
            this._messageConverterFactory = messageConverterFactory;
        }

        public void ConfigureInputChannel(IMessageChannel messageChannel, string channelName)
        {
            ConfigureMessageChannel(messageChannel, channelName, true);
        }

        public void ConfigureOutputChannel(IMessageChannel messageChannel, string channelName)
        {
            ConfigureMessageChannel(messageChannel, channelName, false);
        }

        public void ConfigurePolledMessageSource(IPollableMessageSource binding, string name)
        {
            BindingOptions bindingOptions = Options.GetBindingOptions(name);
            string contentType = bindingOptions.ContentType;
            ConsumerOptions consumerOptions = bindingOptions.Consumer;
            if ((consumerOptions == null || !consumerOptions.UseNativeDecoding)
                    && binding is DefaultPollableMessageSource)
            {
                ((DefaultPollableMessageSource)binding).AddInterceptor(
                    new InboundContentTypeEnhancingInterceptor(contentType));
            }
        }

        private void ConfigureMessageChannel(IMessageChannel channel, string channelName, bool inbound)
        {
            AbstractMessageChannel messageChannel = channel as AbstractMessageChannel;
            if (messageChannel == null)
            {
                throw new ArgumentException(nameof(channel) + " not an AbstractMessageChannel");
            }

            BindingOptions bindingOptions = this.Options.GetBindingOptions(channelName);
            string contentType = bindingOptions.ContentType;
            ProducerOptions producerOptions = bindingOptions.Producer;
            if (!inbound && producerOptions != null
                    && producerOptions.IsPartitioned)
            {
                messageChannel.AddInterceptor(
                    new PartitioningInterceptor(
                        bindingOptions,
                        GetPartitionKeyExtractorStrategy(),
                        GetPartitionSelectorStrategy()));
            }

            ConsumerOptions consumerOptions = bindingOptions.Consumer;
            if (IsNativeEncodingNotSet(producerOptions, consumerOptions, inbound))
            {
                if (inbound)
                {
                    messageChannel.AddInterceptor(
                        new InboundContentTypeEnhancingInterceptor(contentType));
                }
                else
                {
                    messageChannel.AddInterceptor(
                        new OutboundContentTypeConvertingInterceptor(
                            contentType,
                            _messageConverterFactory.MessageConverterForAllRegistered));
                }
            }
        }

        private IPartitionSelectorStrategy GetPartitionSelectorStrategy()
        {
            // TODO: Named strategies required?
            var result = _services.GetService(typeof(IPartitionSelectorStrategy));
            if (result == null)
            {
                throw new InvalidOperationException("Partioning enabled, unable to obtain `IPartitionSelectorStrategy` from service container.");
            }

            return (IPartitionSelectorStrategy)result;
        }

        private IPartitionKeyExtractorStrategy GetPartitionKeyExtractorStrategy()
        {
            // TODO: Named strategies required?
            var result = _services.GetService(typeof(IPartitionKeyExtractorStrategy));
            if (result == null)
            {
                throw new InvalidOperationException("Partioning enabled, unable to obtain `IPartitionKeyExtractorStrategy` from service container.");
            }

            return (IPartitionKeyExtractorStrategy)result;
        }

        private bool IsNativeEncodingNotSet(ProducerOptions producerOptions, ConsumerOptions consumerOptions, bool input)
        {
            if (input)
            {
                return consumerOptions == null
                        || !consumerOptions.UseNativeDecoding;
            }
            else
            {
                return producerOptions == null
                        || !producerOptions.UseNativeEncoding;
            }
        }
    }
}
