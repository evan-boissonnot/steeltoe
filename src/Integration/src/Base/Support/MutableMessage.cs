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
using System.Text;

namespace Steeltoe.Integration.Support
{
    public class MutableMessage<T> : IMessage<T>
    {
        private readonly T _payload;

        private readonly MutableMessageHeaders _headers;

        public MutableMessage(T payload)
            : this(payload, (Dictionary<string, object>)null)
        {
        }

        public MutableMessage(T payload, IDictionary<string, object> headers)
        : this(payload, new MutableMessageHeaders(headers))
        {
        }

        public MutableMessage(T payload, MutableMessageHeaders headers)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            _payload = payload;
            _headers = headers;
        }

        public IMessageHeaders Headers
        {
            get { return _headers; }
        }

        public T Payload
        {
            get { return _payload; }
        }

        object IMessage.Payload => Payload;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(GetType().Name);
            sb.Append(" [payload=");
            if (_payload is byte[])
            {
                sb.Append("byte[").Append(((byte[])(object)_payload).Length).Append("]");
            }
            else
            {
                sb.Append(this._payload);
            }

            sb.Append(", headers=").Append(this._headers).Append("]");
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return (_headers.GetHashCode() * 23) + ObjectUtils.NullSafeHashCode(_payload);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj != null && obj is MutableMessage<T>)
            {
                MutableMessage<T> other = (MutableMessage<T>)obj;
                Guid? thisId = _headers.Id;
                Guid? otherId = other._headers.Id;
                return ObjectUtils.NullSafeEquals(thisId, otherId) &&
                        _headers.Equals(other._headers) && _payload.Equals(other._payload);
            }

            return false;
        }

        protected internal IDictionary<string, object> RawHeaders
        {
            get { return _headers.RawHeaders; }
        }
    }
}
