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

namespace Steeltoe.Messaging.Converter
{
    public class CompositeMessageConverter : ISmartMessageConverter
    {
        private readonly List<IMessageConverter> converters;

        public CompositeMessageConverter(ICollection<IMessageConverter> converters)
        {
            if (converters == null || converters.Count == 0)
            {
                throw new ArgumentException(nameof(converters));
            }

            this.converters = new List<IMessageConverter>(converters);
        }

        public object FromMessage(IMessage message, Type targetClass)
        {
            foreach (IMessageConverter converter in Converters)
            {
                object result = converter.FromMessage(message, targetClass);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public object FromMessage(IMessage message, Type targetClass, object conversionHint)
        {
            foreach (IMessageConverter converter in Converters)
            {
                object result = converter is ISmartMessageConverter ?
                    ((ISmartMessageConverter)converter).FromMessage(message, targetClass, conversionHint) :
                    converter.FromMessage(message, targetClass);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public IMessage ToMessage(object payload, IMessageHeaders headers)
        {
            foreach (IMessageConverter converter in Converters)
            {
                IMessage result = converter.ToMessage(payload, headers);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public IMessage ToMessage(object payload, IMessageHeaders headers, object conversionHint)
        {
            foreach (IMessageConverter converter in Converters)
            {
                IMessage result = converter is ISmartMessageConverter ?
                         ((ISmartMessageConverter)converter).ToMessage(payload, headers, conversionHint) :
                         converter.ToMessage(payload, headers);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public List<IMessageConverter> Converters
        {
            get { return this.converters; }
        }

        public override string ToString()
        {
            return "CompositeMessageConverter[converters=" + Converters.Count + "]";
        }

        public T FromMessage<T>(IMessage message, object conversionHint = null)
        {
            return (T)FromMessage(message, typeof(T), conversionHint);
        }

        public T FromMessage<T>(IMessage message)
        {
            return (T)FromMessage(message, typeof(T), null);
        }
    }
}
