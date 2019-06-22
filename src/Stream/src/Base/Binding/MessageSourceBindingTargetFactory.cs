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

using Steeltoe.Messaging.Converter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Steeltoe.Stream.Binding
{
    public class MessageSourceBindingTargetFactory : AbstractBindingTargetFactory<IPollableMessageSource>
    {
        private IList<IMessageChannelConfigurer> messageConfigurer;
        private ISmartMessageConverter messageConverter;

        public MessageSourceBindingTargetFactory(ISmartMessageConverter messageConverter, IEnumerable<IMessageChannelConfigurer> messageSourceConfigurer)
        {
            this.messageConfigurer = messageSourceConfigurer.ToList();
            this.messageConverter = messageConverter;
        }

        public override IPollableMessageSource CreateInput(string name)
        {
            DefaultPollableMessageSource chan = new DefaultPollableMessageSource(messageConverter);
            foreach (var configurer in messageConfigurer)
            {
                configurer.ConfigurePolledMessageSource(chan, name);
            }

            return chan;
        }

        public override IPollableMessageSource CreateOutput(string name)
        {
            throw new InvalidOperationException();
        }
    }
}
