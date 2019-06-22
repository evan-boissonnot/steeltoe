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
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Messaging.Support
{
    public class GenericMessage<T> : IMessage<T>
    {
        private readonly T payload;

        private readonly IMessageHeaders headers;

        public GenericMessage(T payload)
        : this(payload, new MessageHeaders(null))
        {
        }

        public GenericMessage(T payload, IDictionary<string, object> headers)
        : this(payload, new MessageHeaders(headers))
        {
        }

        public GenericMessage(T payload, IMessageHeaders headers)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            this.payload = payload;
            this.headers = headers;
        }

        public T Payload
        {
            get { return this.payload; }
        }

        public IMessageHeaders Headers
        {
            get { return this.headers; }
        }

        object IMessage.Payload => this.payload;

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }

            if (!(other is GenericMessage<T>))
            {
                return false;
            }

            GenericMessage<T> otherMsg = (GenericMessage<T>)other;
            return ObjectUtils.NullSafeEquals(this.payload, otherMsg.payload) && this.headers.Equals(otherMsg.headers);
        }

        public override int GetHashCode()
        {
            // Using nullSafeHashCode for proper array hashCode handling
            return (ObjectUtils.NullSafeHashCode(payload) * 23) + headers.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(GetType().Name);
            sb.Append(" [payload=");
            if (this.payload is byte[])
            {
                byte[] arr = (byte[])(object)this.payload;
                sb.Append("byte[").Append(arr.Length).Append("]");
            }
            else
            {
                sb.Append(this.payload);
            }

            sb.Append(", headers=").Append(this.headers).Append("]");
            return sb.ToString();
        }
    }
}
