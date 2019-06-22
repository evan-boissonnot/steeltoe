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
    public class MutableMessageBuilder<T> : AbstractMessageBuilder<T>
    {
        private readonly MutableMessage<T> _mutableMessage;

        private readonly IDictionary<string, object> _headers;

        private MutableMessageBuilder(IMessage<T> message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message is MutableMessage<T>)
            {
                _mutableMessage = (MutableMessage<T>)message;
            }
            else
            {
                _mutableMessage = new MutableMessage<T>(message.Payload, message.Headers);
            }

            _headers = _mutableMessage.RawHeaders;
        }

        public override T Payload
        {
            get { return _mutableMessage.Payload; }
        }

        public override IDictionary<string, object> Headers
        {
            get { return this._headers; }
        }

        public static MutableMessageBuilder<T> WithPayload(T payload)
        {
            return WithPayload(payload, true);
        }

        public static MutableMessageBuilder<T> WithPayload(T payload, bool generateHeaders)
        {
            MutableMessage<T> message;
            if (generateHeaders)
            {
                message = new MutableMessage<T>(payload);
            }
            else
            {
                message = new MutableMessage<T>(payload, new MutableMessageHeaders(null, MessageHeaders.ID_VALUE_NONE, -1L));
            }

            return FromMessage(message);
        }

        public static MutableMessageBuilder<T> FromMessage(IMessage<T> message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new MutableMessageBuilder<T>(message);
        }

        public override IMessageBuilder<T> SetHeader(string headerName, object headerValue)
        {
            if (headerName == null)
            {
                throw new ArgumentNullException(nameof(headerName));
            }

            if (headerValue == null)
            {
                this.RemoveHeader(headerName);
            }
            else
            {
                _headers[headerName] = headerValue;
            }

            return this;
        }

        public override IMessageBuilder<T> SetHeaderIfAbsent(string headerName, object headerValue)
        {
            if (!this._headers.ContainsKey(headerName))
            {
                this._headers.Add(headerName, headerValue);
            }

            return this;
        }

        public override IMessageBuilder<T> RemoveHeaders(params string[] headerPatterns)
        {
            List<string> headersToRemove = new List<string>();
            foreach (string pattern in headerPatterns)
            {
                if (!string.IsNullOrEmpty(pattern))
                {
                    if (pattern.Contains("*"))
                    {
                        headersToRemove.AddRange(GetMatchingHeaderNames(pattern, this._headers));
                    }
                    else
                    {
                        headersToRemove.Add(pattern);
                    }
                }
            }

            foreach (string headerToRemove in headersToRemove)
            {
                RemoveHeader(headerToRemove);
            }

            return this;
        }

        public override IMessageBuilder<T> RemoveHeader(string headerName)
        {
            if (!string.IsNullOrEmpty(headerName))
            {
                _headers.Remove(headerName);
            }

            return this;
        }

        public override IMessageBuilder<T> CopyHeaders(IDictionary<string, object> headersToCopy)
        {
            if (headersToCopy != null)
            {
                foreach (var header in headersToCopy)
                {
                    _headers.Add(header);
                }
            }

            return this;
        }

        public override IMessageBuilder<T> CopyHeadersIfAbsent(IDictionary<string, object> headersToCopy)
        {
            if (headersToCopy != null)
            {
                foreach (var entry in headersToCopy)
                {
                    SetHeaderIfAbsent(entry.Key, entry.Value);
                }
            }

            return this;
        }

        protected override List<List<object>> SequenceDetails
        {
            get
            {
                if (_headers.TryGetValue(IntegrationMessageHeaderAccessor.SEQUENCE_DETAILS, out object result))
                {
                    return (List<List<object>>)result;
                }

                return null;
            }
        }

        protected override object CorrelationId
        {
            get
            {
                if (_headers.TryGetValue(IntegrationMessageHeaderAccessor.CORRELATION_ID, out object result))
                {
                    return result;
                }

                return null;
            }
        }

        protected override object SequenceNumber
        {
            get
            {
                if (_headers.TryGetValue(IntegrationMessageHeaderAccessor.SEQUENCE_NUMBER, out object result))
                {
                    return result;
                }

                return null;
            }
        }

        protected override object SequenceSize
        {
            get
            {
                if (_headers.TryGetValue(IntegrationMessageHeaderAccessor.SEQUENCE_SIZE, out object result))
                {
                    return result;
                }

                return null;
            }
        }

        public override IMessage<T> Build()
        {
            return _mutableMessage;
        }

        private List<string> GetMatchingHeaderNames(string pattern, IDictionary<string, object> headers)
        {
            List<string> matchingHeaderNames = new List<string>();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (PatternMatchUtils.SimpleMatch(pattern, header.Key))
                    {
                        matchingHeaderNames.Add(header.Key);
                    }
                }
            }

            return matchingHeaderNames;
        }
    }
}
