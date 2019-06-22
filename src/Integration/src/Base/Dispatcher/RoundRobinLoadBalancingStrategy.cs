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
using System.Collections.Generic;
using System.Threading;

namespace Steeltoe.Integration.Dispatcher
{
    public class RoundRobinLoadBalancingStrategy : ILoadBalancingStrategy
    {
        private int currentHandlerIndex = 0;

        public IEnumerator<IMessageHandler> GetHandlerEnumerator(IMessage message, List<IMessageHandler> handlers)
        {
            int size = handlers.Count;
            if (size < 2)
            {
                this.GetNextHandlerStartIndex(size);
                return handlers.GetEnumerator();
            }

            return this.BuildHandlerIterator(size, handlers);
        }

        private IEnumerator<IMessageHandler> BuildHandlerIterator(int size, List<IMessageHandler> handlers)
        {
            int nextHandlerStartIndex = GetNextHandlerStartIndex(size);
            var result = handlers.GetRange(nextHandlerStartIndex, handlers.Count - nextHandlerStartIndex);
            result.AddRange(handlers.GetRange(0, nextHandlerStartIndex));
            return result.GetEnumerator();
        }

        private int GetNextHandlerStartIndex(int size)
        {
            if (size > 0)
            {
                int indexTail = Interlocked.Increment(ref this.currentHandlerIndex) % size;
                return indexTail < 0 ? indexTail + size : indexTail;
            }
            else
            {
                return size;
            }
        }
    }
}
