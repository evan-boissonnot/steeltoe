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
using Steeltoe.Messaging.Support;
using System;

namespace Steeltoe.Messaging.Converter
{
    public class SimpleMessageConverter : IMessageConverter
    {
        public object FromMessage(IMessage message, Type targetClass)
        {
            object payload = message.Payload;
            return ClassUtils.IsAssignableValue(targetClass, payload) ? payload : null;
        }

        public T FromMessage<T>(IMessage message)
        {
            return (T)FromMessage(message, typeof(T));
        }

        public IMessage ToMessage(object payload, IMessageHeaders headers)
        {
            if (headers != null)
            {
                MessageHeaderAccessor accessor = MessageHeaderAccessor.GetAccessor<MessageHeaderAccessor>(headers, typeof(MessageHeaderAccessor));
                if (accessor != null && accessor.Mutable)
                {
                    return MessageBuilder<object>.CreateMessage(payload, accessor.MessageHeaders);
                }
            }

            return MessageBuilder<object>.WithPayload(payload).CopyHeaders(headers).Build();
        }
    }
}
