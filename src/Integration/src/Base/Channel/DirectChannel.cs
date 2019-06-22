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

namespace Steeltoe.Integration.Channel
{
    public class DirectChannel : AbstractSubscribableChannel
    {
        private readonly UnicastingDispatcher dispatcher = new UnicastingDispatcher();

        private int maxSubscribers;

        public int MaxSubscribers
        {
            get
            {
                return maxSubscribers;
            }

            set
            {
                this.maxSubscribers = value;
                this.dispatcher.MaxSubscribers = value;
            }
        }

        public bool Failover
        {
            get
            {
                return this.dispatcher.Failover;
            }

            set
            {
                this.dispatcher.Failover = value;
            }
        }

        public DirectChannel()
        : this(new RoundRobinLoadBalancingStrategy())
        {
        }

        public DirectChannel(ILoadBalancingStrategy loadBalancingStrategy)
        {
            this.dispatcher.LoadBalancingStrategy = loadBalancingStrategy;
        }

        protected override IMessageDispatcher GetDispatcher()
        {
            return this.dispatcher;
        }

        // @Override
        // protected void onInit()
        //     {
        //         super.onInit();
        //         if (this.maxSubscribers == null)
        //         {
        //             Integer max = this.getIntegrationProperty(IntegrationProperties.CHANNELS_MAX_UNICAST_SUBSCRIBERS,
        //                     Integer.class);
        // this.setMaxSubscribers(max);
        // }
    }
}
