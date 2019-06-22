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
using Steeltoe.Messaging.Support;
using System;
using System.Collections.Generic;

namespace Steeltoe.Integration.Support
{
    public class MessageBuilder<T> : AbstractMessageBuilder<T>
    {
        private readonly T payload;

        private readonly IMessage<T> originalMessage;

        private readonly IntegrationMessageHeaderAccessor headerAccessor;

        private volatile bool modified;

        private IList<string> readOnlyHeaders;

        internal MessageBuilder(T payload, IMessage<T> originalMessage)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            this.payload = payload;
            this.originalMessage = originalMessage;
            this.headerAccessor = new IntegrationMessageHeaderAccessor(originalMessage);
            if (originalMessage != null)
            {
                this.modified = !this.payload.Equals(originalMessage.Payload);
            }
        }

        public override T Payload
        {
            get { return this.payload; }
        }

        public override IDictionary<string, object> Headers
        {
            get { return this.headerAccessor.ToDictionary(); }
        }

        public static MessageBuilder<T> FromMessage(IMessage<T> message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new MessageBuilder<T>(message.Payload, message);
        }

        public static MessageBuilder<T> WithPayload(T payload)
        {
            return new MessageBuilder<T>(payload, null);
        }

        public override IMessageBuilder<T> SetHeader(string headerName, object headerValue)
        {
            this.headerAccessor.SetHeader(headerName, headerValue);
            return this;
        }

        public override IMessageBuilder<T> SetHeaderIfAbsent(string headerName, object headerValue)
        {
            this.headerAccessor.SetHeaderIfAbsent(headerName, headerValue);
            return this;
        }

        public override IMessageBuilder<T> RemoveHeaders(params string[] headerPatterns)
        {
            this.headerAccessor.RemoveHeaders(headerPatterns);
            return this;
        }

        public override IMessageBuilder<T> RemoveHeader(string headerName)
        {
            if (!this.headerAccessor.IsReadOnly(headerName))
            {
                this.headerAccessor.RemoveHeader(headerName);
            }

            // else if (logger.isInfoEnabled())
            // {
            //    logger.info("The header [" + headerName + "] is ignored for removal because it is is readOnly.");
            // }
            return this;
        }

        public override IMessageBuilder<T> CopyHeaders(IDictionary<string, object> headersToCopy)
        {
            this.headerAccessor.CopyHeaders(headersToCopy);
            return this;
        }

        public override IMessageBuilder<T> CopyHeadersIfAbsent(IDictionary<string, object> headersToCopy)
        {
            if (headersToCopy != null)
            {
                foreach (var entry in headersToCopy)
                {
                    string headerName = entry.Key;
                    if (!this.headerAccessor.IsReadOnly(headerName))
                    {
                        this.headerAccessor.SetHeaderIfAbsent(headerName, entry.Value);
                    }
                }
            }

            return this;
        }

        protected override List<List<object>> SequenceDetails
        {
            get { return (List<List<object>>)this.headerAccessor.GetHeader(IntegrationMessageHeaderAccessor.SEQUENCE_DETAILS); }
        }

        protected override object CorrelationId
        {
            get { return this.headerAccessor.GetCorrelationId(); }
        }

        protected override object SequenceNumber
        {
            get { return this.headerAccessor.GetSequenceNumber(); }
        }

        protected override object SequenceSize
        {
            get { return this.headerAccessor.GetSequenceSize(); }
        }

        // public override IMessageBuilder<T> PushSequenceDetails(object correlationId, int sequenceNumber, int sequenceSize)
        //    {
        //        base.PushSequenceDetails(correlationId, sequenceNumber, sequenceSize);
        //        return this;
        //    }

        // public override IMessageBuilder<T> PopSequenceDetails()
        //    {
        //        base.PopSequenceDetails();
        //        return this;
        //    }

        // public override IMessageBuilder<T> SetExpirationDate(long expirationDate)
        //    {
        //        base.setExpirationDate(expirationDate);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setExpirationDate(Date expirationDate)
        //    {
        //        base.setExpirationDate(expirationDate);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setCorrelationId(object correlationId)
        //    {
        //        base.setCorrelationId(correlationId);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setReplyChannel(MessageChannel replyChannel)
        //    {
        //        base.setReplyChannel(replyChannel);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setReplyChannelName(string replyChannelName)
        //    {
        //        base.setReplyChannelName(replyChannelName);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setErrorChannel(MessageChannel errorChannel)
        //    {
        //        super.setErrorChannel(errorChannel);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setErrorChannelName(string errorChannelName)
        //    {
        //        super.setErrorChannelName(errorChannelName);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setSequenceNumber(Integer sequenceNumber)
        //    {
        //        super.setSequenceNumber(sequenceNumber);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setSequenceSize(Integer sequenceSize)
        //    {
        //        super.setSequenceSize(sequenceSize);
        //        return this;
        //    }

        // public override IMessageBuilder<T> setPriority(Integer priority)
        //    {
        //        super.setPriority(priority);
        //        return this;
        //    }
        public IMessageBuilder<T> ReadOnlyHeaders(IList<string> readOnlyHeaders)
        {
            this.readOnlyHeaders = readOnlyHeaders;
            this.headerAccessor.SetReadOnlyHeaders(readOnlyHeaders);
            return this;
        }

        public override IMessage<T> Build()
        {
            if (!this.modified && !this.headerAccessor.Modified && this.originalMessage != null
                    && !ContainsReadOnly(this.originalMessage.Headers))
            {
                return originalMessage;
            }

            if (this.payload is Exception)
            {
                return (IMessage<T>)new ErrorMessage((Exception)(object)payload, this.headerAccessor.ToDictionary());
            }

            return new GenericMessage<T>(this.payload, this.headerAccessor.ToDictionary());
        }

        private bool ContainsReadOnly(IMessageHeaders headers)
        {
            if (readOnlyHeaders != null)
            {
                foreach (string readOnly in this.readOnlyHeaders)
                {
                    if (headers.ContainsKey(readOnly))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
