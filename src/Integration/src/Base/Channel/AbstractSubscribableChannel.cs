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

using Steeltoe.Integration.Dispatcher;
using Steeltoe.Messaging;
using System;
using System.Threading.Tasks;

namespace Steeltoe.Integration.Channel
{
    public abstract class AbstractSubscribableChannel : AbstractMessageChannel, ISubscribableChannel
    {
        public int SubscriberCount
        {
            get { return GetRequiredDispatcher().HandlerCount; }
        }

        public bool Subscribe(IMessageHandler handler)
        {
            IMessageDispatcher dispatcher = GetRequiredDispatcher();
            bool added = dispatcher.AddHandler(handler);
            AdjustCounterIfNecessary(dispatcher, added ? 1 : 0);
            return added;
        }

        public bool Unsubscribe(IMessageHandler handler)
        {
            IMessageDispatcher dispatcher = GetRequiredDispatcher();
            bool removed = dispatcher.RemoveHandler(handler);
            AdjustCounterIfNecessary(dispatcher, removed ? -1 : 0);
            return removed;
        }

        protected override async Task<bool> DoSendAsync(IMessage message, long timeout)
        {
            try
            {
                return await GetRequiredDispatcher().DispatchAsync(message);
            }
            catch (MessageDispatchingException e)
            {
                string description = e.Message + " for channel '" + Name + "'.";
                throw new MessageDeliveryException(message, description, e);
            }
        }

        protected abstract IMessageDispatcher GetDispatcher();

        private void AdjustCounterIfNecessary(IMessageDispatcher dispatcher, int delta)
        {
            // if (delta != 0)
            // {
            //    if (logger.isInfoEnabled())
            //    {
            //        logger.info("Channel '" + this.Name + "' has " + dispatcher.HandlerCount  + " subscriber(s).");
            //    }
            // }
        }

        private IMessageDispatcher GetRequiredDispatcher()
        {
            IMessageDispatcher dispatcher = GetDispatcher();
            if (dispatcher == null)
            {
                throw new InvalidOperationException("'dispatcher' must not be null");
            }

            return dispatcher;
        }
    }
}
