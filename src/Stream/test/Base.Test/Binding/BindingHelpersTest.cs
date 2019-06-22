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

using Steeltoe.Stream.Messaging;
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Stream.Binding
{
    public class BindingHelpersTest
    {
        [Fact]
        public void CollectFromMethods_AddsExpected()
        {
            IDictionary<string, Channel> cha = new Dictionary<string, Channel>();
            BindingHelpers.CollectFromMethods(typeof(ISink), cha);
            Assert.Empty(cha);

            BindingHelpers.CollectFromMethods(typeof(IBarista), cha);
            Assert.NotEmpty(cha);
            Assert.Equal(3, cha.Count);
            Assert.NotNull(cha["ColdDrinks"].Name);
            Assert.NotNull(cha["HotDrinks"].Name);
            Assert.NotNull(cha["Orders"].Name);
        }

        [Fact]
        public void CollectFromProperties_AddsExpected()
        {
            IDictionary<string, Channel> cha = new Dictionary<string, Channel>();
            BindingHelpers.CollectFromProperties(typeof(IBarista), cha);
            Assert.Empty(cha);

            BindingHelpers.CollectFromProperties(typeof(ISink), cha);
            Assert.NotEmpty(cha);
            Assert.Single(cha);
            Assert.NotNull(cha["input"].Name);

            cha.Clear();
            BindingHelpers.CollectFromProperties(typeof(ISource), cha);
            Assert.NotEmpty(cha);
            Assert.Single(cha);
            Assert.NotNull(cha["output"].Name);

            cha.Clear();
            BindingHelpers.CollectFromProperties(typeof(IProcessor), cha);
            Assert.NotEmpty(cha);
            Assert.Equal(2, cha.Count);
            Assert.NotNull(cha["output"].Name);
            Assert.NotNull(cha["input"].Name);
        }

        [Fact]
        public void CollectProperties_AddsInherited()
        {
            IDictionary<string, Channel> cha = new Dictionary<string, Channel>();
            BindingHelpers.CollectFromProperties(typeof(IProcessor), cha);
            Assert.Equal(2, cha.Count);
        }
    }
}
