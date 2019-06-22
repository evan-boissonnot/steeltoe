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
using System.Threading.Tasks;

namespace Steeltoe.Stream.Binding
{
    public class InboundContentTypeEnhancingInterceptor : AbstractContentTypeInterceptor
    {
        public InboundContentTypeEnhancingInterceptor(string contentType)
        : base(contentType)
        {
        }

        public override Task<IMessage> DoPreSendAsync(IMessage message, IMessageChannel channel)
        {
            var messageHeaders = message.Headers as MessageHeaders;

            // Map<String, Object> headersMap = (Map<String, Object>)ReflectionUtils
            //        .getField(MessageConverterConfigurer.this.headersField,
            //                message.getHeaders());
            MimeType contentType = this.mimeType;

            /*
             * NOTE: The below code for BINDER_ORIGINAL_CONTENT_TYPE is to support legacy
             * message format established in 1.x version of the framework and should/will
             * no longer be supported in 3.x
             //*/

            // if (message.getHeaders()
            //                .containsKey(BinderHeaders.BINDER_ORIGINAL_CONTENT_TYPE))
            //        {
            //            Object ct = message.getHeaders()
            //                    .get(BinderHeaders.BINDER_ORIGINAL_CONTENT_TYPE);
            //            contentType = ct instanceof String ? MimeType.valueOf((String)ct)
            // : (ct == null ? this.mimeType : (MimeType)ct);
            //            headersMap.put(MessageHeaders.CONTENT_TYPE, contentType);
            //            headersMap.remove(BinderHeaders.BINDER_ORIGINAL_CONTENT_TYPE);
            //        }
            // == end legacy note
            if (!message.Headers.ContainsKey(MessageHeaders.CONTENT_TYPE))
            {
                messageHeaders.RawHeaders.Add(MessageHeaders.CONTENT_TYPE, contentType);
            }
            else if (message.Headers.TryGetValue(MessageHeaders.CONTENT_TYPE, out object header))
            {
                if (header is string)
                {
                    messageHeaders.RawHeaders[MessageHeaders.CONTENT_TYPE] = MimeType.ToMimeType((string)header);
                }
            }

            return Task.FromResult(message);
        }
    }
}
