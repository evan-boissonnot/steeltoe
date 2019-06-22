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

using Steeltoe.Common.Util;
using Steeltoe.Messaging;
using System;
using System.Collections.Generic;

namespace Steeltoe.Integration.Support
{
    public abstract class AbstractMessageBuilder<T> : IMessageBuilder<T>
    {
        public virtual IMessageBuilder<T> SetExpirationDate(long expirationDate)
        {
            return SetHeader(IntegrationMessageHeaderAccessor.EXPIRATION_DATE, expirationDate);
        }

        public virtual IMessageBuilder<T> SetExpirationDate(DateTime expirationDate)
        {
            if (expirationDate != null)
            {
                var datetime = new DateTimeOffset(expirationDate);
                return SetHeader(IntegrationMessageHeaderAccessor.EXPIRATION_DATE, datetime.ToUnixTimeMilliseconds());
            }
            else
            {
                return SetHeader(IntegrationMessageHeaderAccessor.EXPIRATION_DATE, null);
            }
        }

        public virtual IMessageBuilder<T> SetCorrelationId(object correlationId)
        {
            return SetHeader(IntegrationMessageHeaderAccessor.CORRELATION_ID, correlationId);
        }

        public virtual IMessageBuilder<T> PushSequenceDetails(object correlationId, int sequenceNumber, int sequenceSize)
        {
            object incomingCorrelationId = this.CorrelationId;
            List<List<object>> incomingSequenceDetails = SequenceDetails;
            if (incomingCorrelationId != null)
            {
                if (incomingSequenceDetails == null)
                {
                    incomingSequenceDetails = new List<List<object>>();
                }
                else
                {
                    incomingSequenceDetails = new List<List<object>>(incomingSequenceDetails);
                }

                incomingSequenceDetails.Add(new List<object>() { incomingCorrelationId, SequenceNumber, SequenceSize });

                // incomingSequenceDetails = incomingSequenceDetails.AsReadOnly();
            }

            if (incomingSequenceDetails != null)
            {
                SetHeader(IntegrationMessageHeaderAccessor.SEQUENCE_DETAILS, incomingSequenceDetails);
            }

            return SetCorrelationId(correlationId)
                    .SetSequenceNumber(sequenceNumber)
                    .SetSequenceSize(sequenceSize);
        }

        public virtual IMessageBuilder<T> PopSequenceDetails()
        {
            List<List<object>> incomingSequenceDetails = SequenceDetails;
            if (incomingSequenceDetails == null)
            {
                return this;
            }
            else
            {
                incomingSequenceDetails = new List<List<object>>(incomingSequenceDetails);
            }

            List<object> sequenceDetails = incomingSequenceDetails[incomingSequenceDetails.Count - 1];
            incomingSequenceDetails.RemoveAt(incomingSequenceDetails.Count - 1);
            if (sequenceDetails.Count != 3)
            {
                throw new InvalidOperationException("Wrong sequence details (not created by MessageBuilder?)");
            }

            SetCorrelationId(sequenceDetails[0]);
            int? sequenceNumber = sequenceDetails[1] as int?;
            int? sequenceSize = sequenceDetails[2] as int?;
            if (sequenceNumber.HasValue)
            {
                SetSequenceNumber(sequenceNumber.Value);
            }

            if (sequenceSize.HasValue)
            {
                SetSequenceSize(sequenceSize.Value);
            }

            if (incomingSequenceDetails.Count > 0)
            {
                SetHeader(IntegrationMessageHeaderAccessor.SEQUENCE_DETAILS, incomingSequenceDetails);
            }
            else
            {
                RemoveHeader(IntegrationMessageHeaderAccessor.SEQUENCE_DETAILS);
            }

            return this;
        }

        public virtual IMessageBuilder<T> SetReplyChannel(IMessageChannel replyChannel)
        {
            return SetHeader(MessageHeaders.REPLY_CHANNEL, replyChannel);
        }

        public virtual IMessageBuilder<T> SetReplyChannelName(string replyChannelName)
        {
            return SetHeader(MessageHeaders.REPLY_CHANNEL, replyChannelName);
        }

        public virtual IMessageBuilder<T> SetErrorChannel(IMessageChannel errorChannel)
        {
            return SetHeader(MessageHeaders.ERROR_CHANNEL, errorChannel);
        }

        public virtual IMessageBuilder<T> SetErrorChannelName(string errorChannelName)
        {
            return SetHeader(MessageHeaders.ERROR_CHANNEL, errorChannelName);
        }

        public virtual IMessageBuilder<T> SetSequenceNumber(int sequenceNumber)
        {
            return SetHeader(IntegrationMessageHeaderAccessor.SEQUENCE_NUMBER, sequenceNumber);
        }

        public virtual IMessageBuilder<T> SetSequenceSize(int sequenceSize)
        {
            return SetHeader(IntegrationMessageHeaderAccessor.SEQUENCE_SIZE, sequenceSize);
        }

        public virtual IMessageBuilder<T> SetPriority(int priority)
        {
            return SetHeader(IntegrationMessageHeaderAccessor.PRIORITY, priority);
        }

        public virtual IMessageBuilder<T> FilterAndCopyHeadersIfAbsent(IDictionary<string, object> headersToCopy, params string[] headerPatternsToFilter)
        {
            IDictionary<string, object> headers = headersToCopy;

            if (headerPatternsToFilter.Length > 0)
            {
                var copy = new Dictionary<string, object>(headersToCopy);
                foreach (var entry in copy)
                {
                    if (PatternMatchUtils.SimpleMatch(headerPatternsToFilter, entry.Key))
                    {
                        headers.Remove(entry.Key);
                    }
                }
            }

            return CopyHeadersIfAbsent(headers);
        }

        public abstract T Payload { get; }

        public abstract IDictionary<string, object> Headers { get; }

        public abstract IMessageBuilder<T> SetHeader(string headerName, object headerValue);

        public abstract IMessageBuilder<T> SetHeaderIfAbsent(string headerName, object headerValue);

        public abstract IMessageBuilder<T> RemoveHeaders(params string[] headerPatterns);

        public abstract IMessageBuilder<T> RemoveHeader(string headerName);

        public abstract IMessageBuilder<T> CopyHeaders(IDictionary<string, object> headersToCopy);

        public abstract IMessageBuilder<T> CopyHeadersIfAbsent(IDictionary<string, object> headersToCopy);

        public abstract IMessage<T> Build();

        protected abstract List<List<object>> SequenceDetails { get; }

        protected abstract object CorrelationId { get; }

        protected abstract object SequenceNumber { get; }

        protected abstract object SequenceSize { get; }
    }
}
