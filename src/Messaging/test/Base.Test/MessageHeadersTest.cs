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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace Steeltoe.Messaging.Test
{
    public class MessageHeadersTest
    {
        [Fact]
        public void TestTimestamp()
        {
            MessageHeaders headers = new MessageHeaders(null);
            Assert.NotNull(headers.Timestamp);
        }

        [Fact]
        public void TestTimestampOverwritten()
        {
            MessageHeaders headers1 = new MessageHeaders(null);
            Thread.Sleep(50);
            MessageHeaders headers2 = new MessageHeaders(headers1);
            Assert.NotEqual(headers1.Timestamp, headers2.Timestamp);
        }

        [Fact]
        public void TestTimestampProvided()
        {
            MessageHeaders headers = new MessageHeaders(null, null, 10L);
            Assert.Equal(10L, (long)headers.Timestamp);
        }

        [Fact]
        public void TestTimestampProvidedNullValue()
        {
            IDictionary<string, object> input = new Dictionary<string, object>() { { MessageHeaders.TIMESTAMP, 1L } };
            MessageHeaders headers = new MessageHeaders(input, null, null);
            Assert.NotNull(headers.Timestamp);
        }

        [Fact]
        public void TestTimestampNone()
        {
            MessageHeaders headers = new MessageHeaders(null, null, -1L);
            Assert.Null(headers.Timestamp);
        }

        [Fact]
        public void TestIdOverwritten()
        {
            MessageHeaders headers1 = new MessageHeaders(null);
            MessageHeaders headers2 = new MessageHeaders(headers1);
            Assert.NotEqual(headers1.Id, headers2.Id);
        }

        [Fact]
        public void TestId()
        {
            MessageHeaders headers = new MessageHeaders(null);
            Assert.NotNull(headers.Id);
        }

        [Fact]
        public void TestIdProvided()
        {
            Guid id = Guid.NewGuid();
            MessageHeaders headers = new MessageHeaders(null, id, null);
            Assert.Equal(id, headers.Id);
        }

        [Fact]
        public void TestIdProvidedNullValue()
        {
            var id = Guid.NewGuid();
            IDictionary<string, object> input = new Dictionary<string, object>() { { MessageHeaders.ID, id } };
            MessageHeaders headers = new MessageHeaders(input, null, null);
            Assert.NotNull(headers.Id);
        }

        [Fact]
        public void TestIdNone()
        {
            MessageHeaders headers = new MessageHeaders(null, MessageHeaders.ID_VALUE_NONE, null);
            Assert.Null(headers.Id);
        }

        [Fact]
        public void TestNonTypedAccessOfHeaderValue()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("test", 123);
            MessageHeaders headers = new MessageHeaders(map);
            Assert.Equal(123, headers["test"]);
        }

        [Fact]
        public void TestTypedAccessOfHeaderValue()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("test", 123);
            MessageHeaders headers = new MessageHeaders(map);
            Assert.Equal(123, headers.Get<int>("test"));
        }

        [Fact]
        public void TestHeaderValueAccessWithIncorrectType()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("test", 123);
            MessageHeaders headers = new MessageHeaders(map);
            Assert.Throws<InvalidCastException>(() => headers.Get<string>("test"));
        }

        [Fact]
        public void TestNullHeaderValue()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            MessageHeaders headers = new MessageHeaders(map);
            headers.TryGetValue("nosuchattribute", out object val);
            Assert.Null(val);
        }

        [Fact]
        public void TestNullHeaderValueWithTypedAccess()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            MessageHeaders headers = new MessageHeaders(map);
            Assert.Null(headers.Get<string>("nosuchattribute"));
        }

    [Fact]
        public void TestHeaderKeys()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("key1", "val1");
            map.Add("key2", 123);
            MessageHeaders headers = new MessageHeaders(map);
            var keys = headers.Keys;
            Assert.True(keys.Contains("key1"));
            Assert.True(keys.Contains("key2"));
        }

        [Fact]
        public void SubclassWithCustomIdAndNoTimestamp()
        {
            var id = Guid.NewGuid();
            MessageHeaders headers = new MyMH(id);
            Assert.Equal(id, headers.Id);
            Assert.Single(headers);
        }

        private class MyMH : MessageHeaders
        {
            public MyMH(Guid id)
                : base(null, id, -1L)
            {
            }
        }
    }
}
