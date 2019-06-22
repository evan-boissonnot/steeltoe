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
using Steeltoe.Messaging.Converter;
using Steeltoe.Messaging.Support;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steeltoe.Integration.Channel
{
    public abstract class AbstractMessageChannel : IMessageChannel
    {
        protected const long INDEFINITE_TIMEOUT = -1L;

        protected readonly ChannelInterceptorList _interceptors;

        protected List<Type> _dataTypes = new List<Type>();

        protected IMessageConverter _messageConverter;

        protected IMessageBuilderFactory _messageBuilderFactory = new DefaultMessageBuilderFactory();

        public AbstractMessageChannel()
            : this(null)
        {
        }

        public AbstractMessageChannel(string name)
        {
            _interceptors = new ChannelInterceptorList();
            Name = name;
        }

        public string Name { get; set; } // Full Channel name?

        public IList<Type> DataTypes
        {
            get { return _dataTypes; }
        }

        public IMessageConverter MessageConverter
        {
            get { return _messageConverter; }
            set { _messageConverter = value; }
        }

        public IMessageBuilderFactory MessageBuilderFactory
        {
            get
            {
                if (_messageBuilderFactory == null)
                {
                    _messageBuilderFactory = new DefaultMessageBuilderFactory();
                }

                return _messageBuilderFactory;
            }

            set
            {
                _messageBuilderFactory = value;
            }
        }

        public void SetInterceptors(IList<IChannelInterceptor> interceptors)
        {
            _interceptors.Set(interceptors);
        }

        public void AddInterceptor(IChannelInterceptor interceptor)
        {
            _interceptors.Add(interceptor);
        }

        public void AddInterceptor(int index, IChannelInterceptor interceptor)
        {
            _interceptors.Add(index, interceptor);
        }

        public IList<IChannelInterceptor> ChannelInterceptors
        {
            get { return _interceptors.Interceptors; }
        }

        public bool RemoveInterceptor(IChannelInterceptor interceptor)
        {
            return _interceptors.Remove(interceptor);
        }

        public IChannelInterceptor RemoveInterceptor(int index)
        {
            return _interceptors.Remove(index);
        }

        public async Task<bool> SendAsync(IMessage message)
        {
            return await SendAsync(message, -1);
        }

        public async Task<bool> SendAsync(IMessage message, long timeout)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Payload == null)
            {
                throw new ArgumentNullException("Message payload is null!");
            }

            Stack<IChannelInterceptor> interceptorStack = null;
            bool sent = false;

            // bool metricsProcessed = false;
            // MetricsContext metricsContext = null;
            // bool countsAreEnabled = this.countsEnabled;
            ChannelInterceptorList interceptorList = _interceptors;

            // AbstractMessageChannelMetrics metrics = this.channelMetrics;
            // SampleFacade sample = null;
            try
            {
                if (_dataTypes.Count > 0)
                {
                    message = this.ConvertPayloadIfNecessary(message);
                }

                // bool debugEnabled = this.loggingEnabled && logger.isDebugEnabled();
                // if (debugEnabled)
                // {
                //    logger.debug("preSend on channel '" + this + "', message: " + message);
                // }
                if (interceptorList.Count > 0)
                {
                    interceptorStack = new Stack<IChannelInterceptor>();
                    message = await interceptorList.PreSendAsync(message, this, interceptorStack);
                    if (message == null)
                    {
                        return false;
                    }
                }

                // if (countsAreEnabled)
                // {
                //    metricsContext = metrics.beforeSend();
                //    if (this.metricsCaptor != null)
                //    {
                //        sample = this.metricsCaptor.start();
                //    }
                //    sent = doSend(message, timeout);
                //    if (sample != null)
                //    {
                //        sample.stop(sendTimer(sent));
                //    }
                //    metrics.afterSend(metricsContext, sent);
                //    metricsProcessed = true;
                // }
                // else
                // {
                sent = await DoSendAsync(message, timeout);

                // }

                // if (debugEnabled)
                // {
                //    logger.debug("postSend (sent=" + sent + ") on channel '" + this + "', message: " + message);
                // }
                if (interceptorStack != null)
                {
                    await interceptorList.PostSendAsync(message, this, sent);
                    await interceptorList.AfterSendCompletionAsync(message, this, sent, null, interceptorStack);
                }

                return sent;
            }
            catch (Exception e)
            {
                // if (countsAreEnabled && !metricsProcessed)
                // {
                //    if (sample != null)
                //    {
                //        sample.stop(buildSendTimer(false, e.getClass().getSimpleName()));
                //    }
                //    metrics.afterSend(metricsContext, false);
                // }
                if (interceptorStack != null)
                {
                    await interceptorList.AfterSendCompletionAsync(message, this, sent, e, interceptorStack);
                }

                throw IntegrationUtils.WrapInDeliveryExceptionIfNecessary(message, "failed to send Message to channel '" + Name + "'", e);
            }
        }

        protected abstract Task<bool> DoSendAsync(IMessage message, long timeout);

        private IMessage ConvertPayloadIfNecessary(IMessage message)
        {
            // first pass checks if the payload type already matches any of the datatypes
            foreach (Type datatype in _dataTypes)
            {
                if (datatype.IsAssignableFrom(message.Payload.GetType()))
                {
                    return message;
                }
            }

            if (this._messageConverter != null)
            {
                // second pass applies conversion if possible, attempting datatypes in order
                foreach (Type datatype in _dataTypes)
                {
                    object converted = _messageConverter.FromMessage(message, datatype);
                    if (converted != null)
                    {
                        if (converted is IMessage)
                        {
                            return (IMessage)converted;
                        }
                        else
                        {
                            return _messageBuilderFactory
                                    .WithPayload(converted)
                                    .CopyHeaders(message.Headers)
                                    .Build();
                        }
                    }
                }
            }

            throw new MessageDeliveryException(
                message,
                "Channel '" + Name + "' expected one of the following datataypes [" + string.Join(",", _dataTypes) + "], but received [" + message.Payload.GetType() + "]");
        }

        protected class ChannelInterceptorList
        {
            private readonly List<IChannelInterceptor> _interceptors = new List<IChannelInterceptor>();
            private object _lock = new object();

            public ChannelInterceptorList()
            {
            }

            public bool Set(IList<IChannelInterceptor> interceptors)
            {
                lock (_lock)
                {
                    this._interceptors.Clear();
                    _interceptors.AddRange(interceptors);
                    return true;
                }
            }

            public int Count
            {
                get
                {
                    lock (_lock)
                    {
                        return _interceptors.Count;
                    }
                }
            }

            public bool Add(IChannelInterceptor interceptor)
            {
                lock (_lock)
                {
                    _interceptors.Add(interceptor);
                    return true;
                }
            }

            public void Add(int index, IChannelInterceptor interceptor)
            {
                lock (_lock)
                {
                    _interceptors.Insert(index, interceptor);
                }
            }

            public async Task<IMessage> PreSendAsync(IMessage messageArg, IMessageChannel channel, Stack<IChannelInterceptor> interceptorStack)
            {
                IMessage message = messageArg;

                IList<IChannelInterceptor> interceptors = Interceptors;

                foreach (var interceptor in interceptors)
                {
                    IMessage previous = message;
                    message = await interceptor.PreSendAsync(message, channel);
                    if (message == null)
                    {
                        // if (this.logger.isDebugEnabled())
                        // {
                        //    this.logger.debug(interceptor.getClass().getSimpleName() + " returned null from preSend, i.e. precluding the send.");
                        // }
                        await AfterSendCompletionAsync(previous, channel, false, (Exception)null, interceptorStack);
                        return null;
                    }

                    interceptorStack.Push(interceptor);
                }

                return message;
            }

            public async Task PostSendAsync(IMessage message, IMessageChannel channel, bool sent)
            {
                IList<IChannelInterceptor> interceptors = Interceptors;

                foreach (var interceptor in interceptors)
                {
                    await interceptor.PostSendAsync(message, channel, sent);
                }
            }

            public async Task AfterSendCompletionAsync(IMessage message, IMessageChannel channel, bool sent, Exception ex, Stack<IChannelInterceptor> interceptorStack)
            {
                IList<IChannelInterceptor> interceptors = Interceptors;

                foreach (var interceptor in interceptors)
                {
                    try
                    {
                        await interceptor.AfterSendCompletionAsync(message, channel, sent, ex);
                    }
                    catch (Exception)
                    {
                        // this.logger.error("Exception from afterSendCompletion in " + interceptor, var9);
                    }
                }
            }

            public async Task<bool> PreReceiveAsync(IMessageChannel channel, Stack<IChannelInterceptor> interceptorStack)
            {
                IList<IChannelInterceptor> interceptors = Interceptors;

                foreach (var interceptor in interceptors)
                {
                    if (!await interceptor.PreReceiveAsync(channel))
                    {
                        await AfterReceiveCompletionAsync((IMessage)null, channel, (Exception)null, interceptorStack);
                        return false;
                    }

                    interceptorStack.Push(interceptor);
                }

                return true;
            }

            public async Task<IMessage> PostReceiveAsync(IMessage messageArg, IMessageChannel channel)
            {
                IMessage message = messageArg;
                IList<IChannelInterceptor> interceptors = Interceptors;

                foreach (var interceptor in interceptors)
                {
                    message = await interceptor.PostReceiveAsync(message, channel);
                    if (message == null)
                    {
                        return null;
                    }
                }

                return message;
            }

            public async Task AfterReceiveCompletionAsync(IMessage message, IMessageChannel channel, Exception ex, Stack<IChannelInterceptor> interceptorStack)
            {
                foreach (var interceptor in interceptorStack)
                {
                    try
                    {
                        await interceptor.AfterReceiveCompletionAsync(message, channel, ex);
                    }
                    catch (Exception)
                    {
                        // this.logger.error("Exception from afterReceiveCompletion in " + interceptor, var8);
                    }
                }
            }

            public IList<IChannelInterceptor> Interceptors
            {
                get
                {
                    lock (_lock)
                    {
                        return new List<IChannelInterceptor>(_interceptors);
                    }
                }
            }

            public bool Remove(IChannelInterceptor interceptor)
            {
                lock (_lock)
                {
                    return _interceptors.Remove(interceptor);
                }
            }

            public IChannelInterceptor Remove(int index)
            {
                lock (_lock)
                {
                    if (index < 0 || index >= _interceptors.Count)
                    {
                        return null;
                    }

                    IChannelInterceptor current = _interceptors[index];
                    _interceptors.RemoveAt(index);
                    return current;
                }
            }
        }
    }
}
