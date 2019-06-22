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
    public abstract class AbstractDispatcher : IMessageDispatcher
    {
        protected readonly IList<IMessageHandler> handlers = new List<IMessageHandler>();

        protected object _lock = new object();

        protected volatile int maxSubscribers = int.MaxValue;

        protected volatile IMessageHandler theOneHandler;

        public int MaxSubscribers
        {
            get { return maxSubscribers; }
            set { maxSubscribers = value; }
        }

        protected List<IMessageHandler> Handlers
        {
            get
            {
                lock (_lock)
                {
                    return new List<IMessageHandler>(this.handlers);
                }
            }
        }

        public virtual bool AddHandler(IMessageHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (handlers.Count == MaxSubscribers)
            {
                throw new ArgumentException("Maximum subscribers exceeded");
            }

            lock (_lock)
            {
                this.handlers.Add(handler);
                if (this.handlers.Count == 1)
                {
                    this.theOneHandler = handler;
                }
                else
                {
                    this.theOneHandler = null;
                }
            }

            return true;
        }

        public virtual bool RemoveHandler(IMessageHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            lock (_lock)
            {
                bool removed = this.handlers.Remove(handler);
                if (this.handlers.Count == 1)
                {
                    this.theOneHandler = handlers[0];
                }
                else
                {
                    this.theOneHandler = null;
                }

                return removed;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + " with handlers: " + this.handlers.Count;
        }

        public virtual int HandlerCount
        {
            get { return this.handlers.Count; }
        }

        public abstract Task<bool> DispatchAsync(IMessage message);

        protected async Task<bool> TryOptimizedDispatch(IMessage message)
        {
            IMessageHandler handler = this.theOneHandler;
            if (handler != null)
            {
                try
                {
                    await handler.HandleMessageAsync(message);
                    return true;
                }
                catch (Exception e)
                {
                    throw IntegrationUtils.WrapInDeliveryExceptionIfNecessary(message, "Dispatcher failed to deliver Message", e);
                }
            }

            return false;
        }
    }
}
