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

using Steeltoe.Integration.Handler;
using Steeltoe.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Steeltoe.Stream.Binding
{
    public class StreamListenerMessageHandler : AbstractReplyProducingMessageHandler
    {
        private readonly IInvokableHandlerMethod _invocableHandlerMethod;

        private readonly bool _copyHeaders;

        public StreamListenerMessageHandler(IMessageChannel channel, IInvokableHandlerMethod invocableHandlerMethod, bool copyHeaders, IList<string> notPropagatedHeaders)
            : base(channel)
        {
            _invocableHandlerMethod = invocableHandlerMethod;
            _copyHeaders = copyHeaders;
            NotPropagatedHeaders = notPropagatedHeaders;
        }

        public StreamListenerMessageHandler(string channelName, IInvokableHandlerMethod invocableHandlerMethod, bool copyHeaders, IList<string> notPropagatedHeaders)
            : base(channelName)
        {
            _invocableHandlerMethod = invocableHandlerMethod;
            _copyHeaders = copyHeaders;
            NotPropagatedHeaders = notPropagatedHeaders;
        }

        public bool IsVoid
        {
            get { return _invocableHandlerMethod.IsVoid; }
        }

        protected override bool CopyRequestHeaders
        {
            get { return _copyHeaders; }
        }

        protected override object HandleRequestMessage(IMessage requestMessage)
        {
            try
            {
                return _invocableHandlerMethod.Invoke(requestMessage);
            }
            catch (Exception e)
            {
                if (e is MessagingException)
                {
                    throw;
                }
                else
                {
                    throw new MessagingException(
                        requestMessage,
                            "Exception thrown while invoking " + _invocableHandlerMethod.ShortLogMessage,
                            e);
                }
            }
        }
    }
}
