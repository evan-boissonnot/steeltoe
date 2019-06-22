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
using Steeltoe.Messaging.Converter;
using System;
using System.Threading.Tasks;

namespace Steeltoe.Stream.Binding
{
    public class OutboundContentTypeConvertingInterceptor : AbstractContentTypeInterceptor
    {
        private readonly IMessageConverter _messageConverter;

        public OutboundContentTypeConvertingInterceptor(string contentType, IMessageConverter messageConverter)
        : base(contentType)
        {
            _messageConverter = messageConverter;
        }

        public override Task<IMessage> DoPreSendAsync(IMessage message, IMessageChannel channel)
        {
            // If handler is a function, FunctionInvoker will already perform message
            // conversion.
            // In fact in the future we should consider propagating knowledge of the
            // default content type
            // to MessageConverters instead of interceptors
            if (message.Payload is byte[] && message.Headers.ContainsKey(MessageHeaders.CONTENT_TYPE))
            {
                return Task.FromResult(message);
            }

            // ===== 1.3 backward compatibility code part-1 ===

            // String oct = message.getHeaders().containsKey(MessageHeaders.CONTENT_TYPE)
            //               ? message.getHeaders().get(MessageHeaders.CONTENT_TYPE).toString()
            //               : null;
            //       String ct = message.getPayload() instanceof String
            // ? JavaClassMimeTypeUtils.mimeTypeFromObject(message.getPayload(),
            //                       ObjectUtils.nullSafeToString(oct)).toString()
            // : oct;

            // ===== END 1.3 backward compatibility code part-1 ===
            if (!message.Headers.ContainsKey(MessageHeaders.CONTENT_TYPE))
            {
                var messageHeaders = message.Headers as MessageHeaders;

                // Map<String, Object> headersMap = (Map<String, Object>)ReflectionUtils
                //        .getField(MessageConverterConfigurer.this.headersField,
                //                message.getHeaders());
                messageHeaders.RawHeaders[MessageHeaders.CONTENT_TYPE] = this.mimeType;
            }

            IMessage<byte[]> outboundMessage = message.Payload is byte[] ? (IMessage<byte[]>)message
                    : (IMessage<byte[]>)_messageConverter.ToMessage(message.Payload, message.Headers);
            if (outboundMessage == null)
            {
                throw new InvalidOperationException("Failed to convert message: '" + message + "' to outbound message.");
            }

            // ===== 1.3 backward compatibility code part-2 ===
            // if (ct != null && !ct.equals(oct) && oct != null)
            // {
            //    @SuppressWarnings("unchecked")

            // Map<String, Object> headersMap = (Map<String, Object>)ReflectionUtils
            //            .getField(MessageConverterConfigurer.this.headersField,
            //                    outboundMessage.getHeaders());
            //    headersMap.put(MessageHeaders.CONTENT_TYPE, MimeType.valueOf(ct));
            //    headersMap.put(BinderHeaders.BINDER_ORIGINAL_CONTENT_TYPE,
            //            MimeType.valueOf(oct));
            // }
            // ===== END 1.3 backward compatibility code part-2 ===
            return Task.FromResult<IMessage>(outboundMessage);
        }
    }
}
