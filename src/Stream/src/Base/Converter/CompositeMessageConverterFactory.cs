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
using Steeltoe.Messaging.Converter;
using Steeltoe.Stream.Config;
using System.Collections.Generic;

namespace Steeltoe.Stream.Converter
{
    public class CompositeMessageConverterFactory : IMessageConverterFactory
    {
        private IList<IMessageConverter> _converters;

        public CompositeMessageConverterFactory(IEnumerable<IMessageConverter> converters, JsonSerializerSettings settings = null)
        {
            _converters = new List<IMessageConverter>(converters);
            InitDefaultConverters(settings);
            DefaultContentTypeResolver resolver = new DefaultContentTypeResolver();
            resolver.DefaultMimeType = BindingOptions.DEFAULT_CONTENT_TYPE;
            foreach (var mc in converters)
            {
                if (mc is AbstractMessageConverter)
                {
                    ((AbstractMessageConverter)mc).ContentTypeResolver = resolver;
                }
            }
        }

        public IMessageConverter GetMessageConverterForType(MimeType mimeType)
        {
            List<IMessageConverter> converters = new List<IMessageConverter>();
            foreach (IMessageConverter converter in this._converters)
            {
                if (converter is AbstractMessageConverter)
                {
                    foreach (MimeType type in ((AbstractMessageConverter)converter).SupportedMimeTypes)
                    {
                        if (type.Includes(mimeType))
                        {
                            converters.Add(converter);
                        }
                    }
                }

                // else
                // {
                //    if (this.log.isDebugEnabled())
                //    {
                //        this.log.debug("Ommitted " + converter + " of type "
                //                + converter.getClass().toString() + " for '"
                //                + mimeType.toString()
                //                + "' as it is not an AbstractMessageConverter");
                //    }
                // }
            }

            if (converters.Count == 0)
            {
                throw new ConversionException("No message converter is registered for " + mimeType.ToString());
            }

            if (converters.Count > 1)
            {
                return new CompositeMessageConverter(converters);
            }
            else
            {
                return converters[0];
            }
        }

        public IMessageConverter MessageConverterForAllRegistered
        {
            get { return new CompositeMessageConverter(new List<IMessageConverter>(this._converters)); }
        }

        private void InitDefaultConverters(JsonSerializerSettings settings)
        {
            ApplicationJsonMessageMarshallingConverter applicationJsonConverter = new ApplicationJsonMessageMarshallingConverter(settings);
            applicationJsonConverter.StrictContentTypeMatch = true;
            this._converters.Add(applicationJsonConverter);
            this._converters.Add(new ObjectSupportingByteArrayMessageConverter());
            this._converters.Add(new ObjectStringMessageConverter());
        }
    }
}
