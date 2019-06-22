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

using Steeltoe.Integration.Support;
using Steeltoe.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steeltoe.Integration.Dispatcher
{
    public class UnicastingDispatcher : AbstractDispatcher
    {
        private volatile bool failover = true;

        private volatile ILoadBalancingStrategy loadBalancingStrategy;

        public UnicastingDispatcher()
        {
        }

        public bool Failover
        {
            get { return failover; }
            set { failover = value; }
        }

        public ILoadBalancingStrategy LoadBalancingStrategy
        {
            get { return this.loadBalancingStrategy; }
            set { this.loadBalancingStrategy = value; }
        }

        public async override Task<bool> DispatchAsync(IMessage message)
        {
            return await DoDispatchAsync(message);
        }

        private async Task<bool> DoDispatchAsync(IMessage message)
        {
            if (await TryOptimizedDispatch(message))
            {
                return true;
            }

            bool success = false;
            IEnumerator<IMessageHandler> handlerIterator = this.GetHandlerEnumerator(message);
            if (!handlerIterator.MoveNext())
            {
                throw new MessageDispatchingException(message, "Dispatcher has no subscribers");
            }

            List<Exception> exceptions = new List<Exception>();
            do
            {
                IMessageHandler handler = handlerIterator.Current;
                try
                {
                    await handler.HandleMessageAsync(message);
                    success = true; // we have a winner.
                }
                catch (Exception e)
                {
                    Exception runtimeException = IntegrationUtils.WrapInDeliveryExceptionIfNecessary(message, "Dispatcher failed to deliver Message", e);
                    exceptions.Add(runtimeException);
                    this.HandleExceptions(exceptions, message, !handlerIterator.MoveNext());
                }
            }
            while (!success && handlerIterator.MoveNext());
            return success;
        }

        private IEnumerator<IMessageHandler> GetHandlerEnumerator(IMessage message)
        {
            if (this.loadBalancingStrategy != null)
            {
                return this.loadBalancingStrategy.GetHandlerEnumerator(message, this.Handlers);
            }

            return this.Handlers.GetEnumerator();
        }

        private void HandleExceptions(List<Exception> allExceptions, IMessage message, bool isLast)
        {
            if (isLast || !this.failover)
            {
                if (allExceptions != null && allExceptions.Count == 1)
                {
                    throw allExceptions[0];
                }

                throw new AggregateMessageDeliveryException(message, "All attempts to deliver Message to MessageHandlers failed.", allExceptions);
            }
        }
    }
}
