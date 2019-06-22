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

using System;

namespace Steeltoe.Stream.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StreamListenerAttribute : Attribute
    {
        private readonly string target;
        private readonly string condition;
        private readonly bool copyHeaders;

        public StreamListenerAttribute(string target, string condition, bool copyHeaders = true)
        {
            this.target = target;
            this.condition = condition;
            this.copyHeaders = copyHeaders;
        }

        public virtual string Target
        {
            get { return this.target; }
        }

        public virtual string Condition
        {
            get { return this.condition; }
        }

        public virtual bool CopyHeaders
        {
            get { return this.copyHeaders; }
        }
    }
}
