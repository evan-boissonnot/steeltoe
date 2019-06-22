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

using System;
using System.Collections.Generic;

namespace Steeltoe.Messaging.Support
{
    public sealed class MessageBuilder<T>
    {
        private readonly T payload;

        private IMessage<T> originalMessage;

        private MessageHeaderAccessor headerAccessor;

        private MessageBuilder(IMessage<T> originalMessage)
        {
            if (originalMessage == null)
            {
                throw new ArgumentNullException(nameof(originalMessage));
            }

            this.payload = originalMessage.Payload;
            this.originalMessage = originalMessage;
            this.headerAccessor = new MessageHeaderAccessor(originalMessage);
        }

        private MessageBuilder(T payload, MessageHeaderAccessor accessor)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (accessor == null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            this.payload = payload;
            this.originalMessage = null;
            this.headerAccessor = accessor;
        }

        public static MessageBuilder<T> FromMessage(IMessage<T> message)
        {
            return new MessageBuilder<T>(message);
        }

        public static MessageBuilder<T> WithPayload(T payload)
        {
            return new MessageBuilder<T>(payload, new MessageHeaderAccessor());
        }

        public static IMessage<T> CreateMessage(T payload, IMessageHeaders messageHeaders)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (messageHeaders == null)
            {
                throw new ArgumentNullException(nameof(messageHeaders));
            }

            if (payload is Exception)
            {
                return (IMessage<T>)new ErrorMessage((Exception)(object)payload, messageHeaders);
            }
            else
            {
                return new GenericMessage<T>(payload, messageHeaders);
            }
        }

        public MessageBuilder<T> SetHeaders(MessageHeaderAccessor accessor)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            this.headerAccessor = accessor;
            return this;
        }

        public MessageBuilder<T> SetHeader(string headerName, object headerValue)
        {
            this.headerAccessor.SetHeader(headerName, headerValue);
            return this;
        }

        public MessageBuilder<T> SetHeaderIfAbsent(string headerName, object headerValue)
        {
            this.headerAccessor.SetHeaderIfAbsent(headerName, headerValue);
            return this;
        }

        public MessageBuilder<T> RemoveHeaders(params string[] headerPatterns)
        {
            this.headerAccessor.RemoveHeaders(headerPatterns);
            return this;
        }

        public MessageBuilder<T> RemoveHeader(string headerName)
        {
            this.headerAccessor.RemoveHeader(headerName);
            return this;
        }

        public MessageBuilder<T> CopyHeaders(IDictionary<string, object> headersToCopy)
        {
            this.headerAccessor.CopyHeaders(headersToCopy);
            return this;
        }

        public MessageBuilder<T> CopyHeadersIfAbsent(IDictionary<string, object> headersToCopy)
        {
            this.headerAccessor.CopyHeadersIfAbsent(headersToCopy);
            return this;
        }

        public MessageBuilder<T> SetReplyChannel(IMessageChannel replyChannel)
        {
            this.headerAccessor.ReplyChannel = replyChannel;
            return this;
        }

        public MessageBuilder<T> SetReplyChannelName(string replyChannelName)
        {
            this.headerAccessor.ReplyChannelName = replyChannelName;
            return this;
        }

        public MessageBuilder<T> SetErrorChannel(IMessageChannel errorChannel)
        {
            this.headerAccessor.ErrorChannel = errorChannel;
            return this;
        }

        public MessageBuilder<T> SetErrorChannelName(string errorChannelName)
        {
            this.headerAccessor.ErrorChannelName = errorChannelName;
            return this;
        }

        public IMessage<T> Build()
        {
            if (this.originalMessage != null && !this.headerAccessor.Modified)
            {
                return this.originalMessage;
            }

            IMessageHeaders headersToUse = this.headerAccessor.ToMessageHeaders();
            if (this.payload is Exception)
            {
                return (IMessage<T>)new ErrorMessage((Exception)(object)this.payload, headersToUse);
            }
            else
            {
                return new GenericMessage<T>(this.payload, headersToUse);
            }
        }
    }
}
