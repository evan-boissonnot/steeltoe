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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Integration.Handler
{
    public abstract class AbstractMessageProducingHandler : AbstractMessageHandler, IMessageProducer, IHeaderPropagation
    {
        private string _outputChannelName;

        private IMessageChannel _outputChannel;

        private List<string> _notPropagatedHeaders = new List<string>();

        private bool _noHeadersPropagation = false;

        private bool _selectiveHeaderPropagation = false;

        public AbstractMessageProducingHandler(string outputChannelName)
        {
            if (string.IsNullOrEmpty(outputChannelName))
            {
                throw new ArgumentException(nameof(outputChannelName));
            }

            _outputChannelName = outputChannelName;
        }

        public AbstractMessageProducingHandler(IMessageChannel outputChannel)
        {
            if (outputChannel == null)
            {
                throw new ArgumentNullException(nameof(outputChannel));
            }

            _outputChannel = outputChannel;
        }

        public IMessageChannel OutputChannel
        {
            get
            {
                if (_outputChannel != null)
                {
                    return _outputChannel;
                }

                if (_outputChannelName != null)
                {
                    _outputChannel = ChannelResolver.ResolveDestination(_outputChannelName);
                    _outputChannelName = null;
                }

                return _outputChannel;
            }
        }

        public string OutputChannelName
        {
            get
            {
                if (!string.IsNullOrEmpty(_outputChannelName))
                {
                    return _outputChannelName;
                }

                if (_outputChannel != null)
                {
                    return _outputChannel.Name;
                }

                return _outputChannelName;
            }
        }

        public IList<string> NotPropagatedHeaders
        {
            get
            {
                return new List<string>(_notPropagatedHeaders);
            }

            set
            {
                UpdateNotPropagatedHeaders(value, false);
            }
        }

        public void AddNotPropagatedHeaders(IList<string> headers)
        {
            UpdateNotPropagatedHeaders(headers, true);
        }

        protected virtual bool CopyRequestHeaders
        {
            get
            {
                return true;
            }
        }

        protected void UpdateNotPropagatedHeaders(IList<string> headers, bool merge)
        {
            HashSet<string> headerPatterns = new HashSet<string>();

            if (merge && _notPropagatedHeaders.Count > 0)
            {
                foreach (var h in _notPropagatedHeaders)
                {
                    headerPatterns.Add(h);
                }
            }

            if (headers.Count > 0)
            {
                foreach (var h in headers)
                {
                    if (string.IsNullOrEmpty(h))
                    {
                        throw new ArgumentException("null or empty elements are not allowed in 'headers'");
                    }

                    headerPatterns.Add(h);
                }

                _notPropagatedHeaders = headerPatterns.ToList();
            }

            bool hasAsterisk = headerPatterns.Contains("*");

            if (hasAsterisk)
            {
                _notPropagatedHeaders = new List<string>() { "*" };
                _noHeadersPropagation = true;
            }

            if (_notPropagatedHeaders.Count > 0)
            {
                _selectiveHeaderPropagation = true;
            }
        }

        protected async virtual Task SendOutputs(object reply, IMessage requestMessage)
        {
            if (reply == null)
            {
                return;
            }

            if (reply is IEnumerable)
            {
                IEnumerable multiReply = (IEnumerable)reply;
                if (ShouldSplitOutput(multiReply))
                {
                    foreach (object r in multiReply)
                    {
                        await ProduceOutput(r, requestMessage);
                    }
                }
            }

            await ProduceOutput(reply, requestMessage);
        }

        protected async virtual Task ProduceOutput(object reply, IMessage requestMessage)
        {
            IMessageHeaders requestHeaders = requestMessage.Headers;
            object replyChannel = requestHeaders.ReplyChannel;
            if (OutputChannel == null)
            {
                // TODO: Add SLIP Header support
                //                Map <?, ?> routingSlipHeader = requestHeaders.get(IntegrationMessageHeaderAccessor.ROUTING_SLIP, Map.class);
                //          if (routingSlipHeader != null) {
                //              Assert.isTrue(routingSlipHeader.size() == 1,
                //                      "The RoutingSlip header value must be a SingletonMap");
                //              Object key = routingSlipHeader.keySet().iterator().next();
                //        Object value = routingSlipHeader.values().iterator().next();
                //        Assert.isInstanceOf(List.class, key, "The RoutingSlip key must be List");
                //              Assert.isInstanceOf(Integer.class, value, "The RoutingSlip value must be Integer");
                //              List<?> routingSlip = (List <?>) key;
                //              AtomicInteger routingSlipIndex = new AtomicInteger((Integer)value);
                //        replyChannel = getOutputChannelFromRoutingSlip(reply, requestMessage, routingSlip, routingSlipIndex);
                //              if (replyChannel != null) {
                //                  reply = addRoutingSlipHeader(reply, routingSlip, routingSlipIndex);
                //    }
                // }
                if (replyChannel == null && reply is IMessage)
                {
                    IMessage replyMessage = reply as IMessage;
                    replyChannel = replyMessage.Headers.ReplyChannel;
                }
            }

            await SendOutput(CreateOutputMessage(reply, requestHeaders), replyChannel, false);

            // await DoProduceOutput(requestMessage, requestHeaders, reply, replyChannel);
        }

        protected async virtual Task SendOutput(object output, object replyChannelArg, bool useArgChannel)
        {
            object replyChannel = replyChannelArg;
            IMessageChannel outChannel = OutputChannel;
            if (!useArgChannel && outChannel != null)
            {
                replyChannel = outChannel;
            }

            if (replyChannel == null)
            {
                throw new DestinationResolutionException("no output-channel or replyChannel header available");
            }

            if (replyChannel is IMessageChannel)
            {
                if (output is IMessage)
                {
                    await Send((IMessageChannel)replyChannel, (IMessage)output);
                }
                else
                {
                    await ConvertAndSend((IMessageChannel)replyChannel, output);
                }
            }
            else if (replyChannel is string)
            {
                if (output is IMessage)
                {
                    await Send((string)replyChannel, (IMessage)output);
                }
                else
                {
                    await ConvertAndSend((string)replyChannel, output);
                }
            }
            else
            {
                throw new MessagingException("replyChannel must be a IMessageChannel or String");
            }
        }

        protected virtual bool ShouldSplitOutput(IEnumerable reply)
        {
            foreach (object next in reply)
            {
                if (next is IMessage)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual IMessage CreateOutputMessage(object output, IMessageHeaders requestHeaders)
        {
            IMessageBuilder<object> builder = null;
            if (output is IMessage)
            {
                if (_noHeadersPropagation || !CopyRequestHeaders)
                {
                    return (IMessage)output;
                }

                IMessage<object> objMessage = (IMessage<object>)output;
                builder = this.MessageBuilderFactory.FromMessage<object>(objMessage);
            }
            else
            {
                builder = this.MessageBuilderFactory.WithPayload(output);
            }

            if (!_noHeadersPropagation && CopyRequestHeaders)
            {
                builder.FilterAndCopyHeadersIfAbsent(requestHeaders, _selectiveHeaderPropagation ? _notPropagatedHeaders.ToArray() : null);
            }

            return builder.Build();
        }

        private async Task ConvertAndSend(string replyChannel, object output)
        {
            throw new NotImplementedException();
        }

        private async Task Send(string replyChannel, IMessage output)
        {
            throw new NotImplementedException();
        }

        private async Task ConvertAndSend(IMessageChannel replyChannel, object output)
        {
            throw new NotImplementedException();
        }

        private async Task Send(IMessageChannel replyChannel, IMessage output)
        {
            throw new NotImplementedException();
        }
    }
}
