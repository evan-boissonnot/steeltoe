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

using Newtonsoft.Json;
using Steeltoe.Common.Util;
using Steeltoe.Messaging;
using Steeltoe.Messaging.Converter;
using System;
using System.Reflection;

namespace Steeltoe.Stream.Converter
{
    public class ApplicationJsonMessageMarshallingConverter : NewtonJsonMessageConverter
    {
        // private final Map<ParameterizedTypeReference<?>, JavaType> typeCache = new ConcurrentHashMap<>();
        internal ApplicationJsonMessageMarshallingConverter(JsonSerializerSettings settings = null)
        {
            if (settings != null)
            {
                Settings = settings;
            }
        }

        protected override object ConvertToInternal(object payload, IMessageHeaders headers, object conversionHint)
        {
            if (payload is byte[])
            {
                return payload;
            }
            else if (payload is string)
            {
                return EncodingUtils.Utf8.GetBytes((string)payload);
            }
            else
            {
                return base.ConvertToInternal(payload, headers, conversionHint);
            }
        }

        protected override object ConvertFromInternal(IMessage message, Type targetClass, object conversionHint)
        {
            object result = null;
            if (conversionHint is ParameterInfo)
            {
                Type conversionHintType = ((ParameterInfo)conversionHint).ParameterType;
                if (IsIMessageGenericType(conversionHintType))
                {
                    /*
                     * Ensures that super won't attempt to create Message as a result of
                     * conversion and stays at payload conversion only. The Message will
                     * eventually be created in
                     * MessageMethodArgumentResolver.resolveArgument(..)
                     */
                    conversionHint = null;
                }

                // else if (((MethodParameter)conversionHint).getGenericParameterType() is ParameterizedType)
                // {
                //    ParameterizedTypeReference<Object> forType = ParameterizedTypeReference.forType(((MethodParameter)conversionHint).getGenericParameterType());
                //    result = ConvertParameterizedType(message, targetClass, forType);
                // }
            }

            // else if (conversionHint is ParameterizedTypeReference)
            // {
            //    result = ConvertParameterizedType(message, targetClass, (ParameterizedTypeReference)conversionHint);
            // }
            if (result == null)
            {
                if (message.Payload is byte[] && targetClass.IsAssignableFrom(typeof(string)))
                {
                    result = EncodingUtils.Utf8.GetString((byte[])message.Payload);
                }
                else
                {
                    result = base.ConvertFromInternal(message, targetClass, conversionHint);
                }
            }

            return result;
        }

        // private object ConvertParameterizedType(IMessage message, Type targetClass, ParameterizedTypeReference conversionHint)
        // {
        //    ObjectMapper objectMapper = this.getObjectMapper();
        //    object payload = message.Payload;
        //    try
        //    {
        //        JavaType type = this.typeCache.get(conversionHint);

        // if (type == null)
        //        {
        //            type = objectMapper.getTypeFactory().constructType((conversionHint).getType());
        //            this.typeCache.put(conversionHint, type);
        //        }

        // if (payload is byte[])
        //        {
        //            return objectMapper.readValue((byte[])payload, type);
        //        }

        // else if (payload is String)
        //        {
        //            return objectMapper.readValue((String)payload, type);
        //        }

        // else
        //        {
        //            return null;
        //        }
        //    }
        //    catch (IOException e)
        //    {
        //        throw new MessageConversionException("Cannot parse payload ", e);
        //    }
        // }
    }
}
