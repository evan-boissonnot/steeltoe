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

using Steeltoe.Common.Util;
using Steeltoe.Messaging.Support;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace Steeltoe.Messaging.Test.Support
{
    public class MessageHeaderAccessorTest
    {
        [Fact]
        public void NewEmptyHeaders()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            Assert.Equal(0, accessor.ToDictionary().Count);
        }

        [Fact]
        public void ExistingHeaders()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("foo", "bar");
            map.Add("bar", "baz");
            GenericMessage<string> message = new GenericMessage<string>("payload", map);

            MessageHeaderAccessor accessor = new MessageHeaderAccessor(message);
            IMessageHeaders actual = accessor.MessageHeaders;

            Assert.Equal(3, actual.Count);
            Assert.Equal("bar", actual.Get<string>("foo"));
            Assert.Equal("baz", actual.Get<string>("bar"));
        }

        [Fact]
        public void ExistingHeadersModification()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("foo", "bar");
            map.Add("bar", "baz");
            GenericMessage<string> message = new GenericMessage<string>("payload", map);

            Thread.Sleep(50);

            MessageHeaderAccessor accessor = new MessageHeaderAccessor(message);
            accessor.SetHeader("foo", "BAR");
            IMessageHeaders actual = accessor.MessageHeaders;

            Assert.Equal(3, actual.Count);
            Assert.NotEqual(message.Headers.Id, actual.Id);
            Assert.Equal("BAR", actual.Get<string>("foo"));
            Assert.Equal("baz", actual.Get<string>("bar"));
        }

        [Fact]
        public void TestRemoveHeader()
        {
            IMessage message = new GenericMessage<string>("payload", SingletonMap("foo", "bar"));
            MessageHeaderAccessor accessor = new MessageHeaderAccessor(message);
            accessor.RemoveHeader("foo");
            IDictionary<string, object> headers = accessor.ToDictionary();
            Assert.False(headers.ContainsKey("foo"));
        }

        [Fact]
        public void TestRemoveHeaderEvenIfNull()
        {
            IMessage<string> message = new GenericMessage<string>("payload", SingletonMap("foo", null));
            MessageHeaderAccessor accessor = new MessageHeaderAccessor(message);
            accessor.RemoveHeader("foo");
            IDictionary<string, object> headers = accessor.ToDictionary();
            Assert.False(headers.ContainsKey("foo"));
        }

        [Fact]
        public void RemoveHeaders()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("foo", "bar");
            map.Add("bar", "baz");
            GenericMessage<string> message = new GenericMessage<string>("payload", map);
            MessageHeaderAccessor accessor = new MessageHeaderAccessor(message);

            accessor.RemoveHeaders("fo*");

            IMessageHeaders actual = accessor.MessageHeaders;
            Assert.Equal(2, actual.Count);
            Assert.Null(actual.Get<string>("foo"));
            Assert.Equal("baz", actual.Get<string>("bar"));
        }

        [Fact]
        public void CopyHeaders()
        {
            IDictionary<string, object> map1 = new Dictionary<string, object>();
            map1.Add("foo", "bar");
            GenericMessage<string> message = new GenericMessage<string>("payload", map1);
            MessageHeaderAccessor accessor = new MessageHeaderAccessor(message);

            IDictionary<string, object> map2 = new Dictionary<string, object>();
            map2.Add("foo", "BAR");
            map2.Add("bar", "baz");
            accessor.CopyHeaders(map2);

            IMessageHeaders actual = accessor.MessageHeaders;
            Assert.Equal(3, actual.Count);
            Assert.Equal("BAR", actual.Get<string>("foo"));
            Assert.Equal("baz", actual.Get<string>("bar"));
        }

        [Fact]
        public void CopyHeadersIfAbsent()
        {
            IDictionary<string, object> map1 = new Dictionary<string, object>();
            map1.Add("foo", "bar");
            GenericMessage<string> message = new GenericMessage<string>("payload", map1);
            MessageHeaderAccessor accessor = new MessageHeaderAccessor(message);

            IDictionary<string, object> map2 = new Dictionary<string, object>();
            map2.Add("foo", "BAR");
            map2.Add("bar", "baz");
            accessor.CopyHeadersIfAbsent(map2);

            IMessageHeaders actual = accessor.MessageHeaders;
            Assert.Equal(3, actual.Count);
            Assert.Equal("bar", actual.Get<string>("foo"));
            Assert.Equal("baz", actual.Get<string>("bar"));
        }

        [Fact]
        public void CopyHeadersFromNullMap()
        {
            MessageHeaderAccessor headers = new MessageHeaderAccessor();
            headers.CopyHeaders(null);
            headers.CopyHeadersIfAbsent(null);

            Assert.Equal(1, headers.MessageHeaders.Count);
            Assert.Contains("id", headers.MessageHeaders.Keys);
        }

        [Fact]
        public void ToDictionary()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();

            accessor.SetHeader("foo", "bar1");
            IDictionary<string, object> map1 = accessor.ToDictionary();

            accessor.SetHeader("foo", "bar2");
            IDictionary<string, object> map2 = accessor.ToDictionary();

            accessor.SetHeader("foo", "bar3");
            IDictionary<string, object> map3 = accessor.ToDictionary();

            Assert.Equal(1, map1.Count);
            Assert.Equal(1, map2.Count);
            Assert.Equal(1, map3.Count);

            Assert.Equal("bar1", map1["foo"]);
            Assert.Equal("bar2", map2["foo"]);
            Assert.Equal("bar3", map3["foo"]);
        }

        [Fact]
        public void LeaveMutable()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            accessor.SetHeader("foo", "bar");
            accessor.LeaveMutable = true;
            IMessageHeaders headers = accessor.MessageHeaders;
            IMessage<string> message = MessageBuilder<string>.CreateMessage("payload", headers);

            accessor.SetHeader("foo", "baz");

            Assert.Equal("baz", headers.Get<string>("foo"));
            Assert.Same(accessor, MessageHeaderAccessor.GetAccessor<MessageHeaderAccessor>(message, typeof(MessageHeaderAccessor)));
        }

        [Fact]
        public void LeaveMutableDefaultBehavior()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            accessor.SetHeader("foo", "bar");
            IMessageHeaders headers = accessor.MessageHeaders;
            IMessage<string> message = MessageBuilder<string>.CreateMessage("payload", headers);

            Assert.Throws<InvalidOperationException>(() => accessor.LeaveMutable = true);

            Assert.Throws<InvalidOperationException>(() => accessor.SetHeader("foo", "baz"));

            Assert.Equal("bar", headers.Get<string>("foo"));
            Assert.Same(accessor, MessageHeaderAccessor.GetAccessor<MessageHeaderAccessor>(message, typeof(MessageHeaderAccessor)));
        }

        [Fact]
        public void GetAccessor()
        {
            MessageHeaderAccessor expected = new MessageHeaderAccessor();
            IMessage<string> message = MessageBuilder<string>.CreateMessage("payload", expected.MessageHeaders);
            Assert.Same(expected, MessageHeaderAccessor.GetAccessor<MessageHeaderAccessor>(message, typeof(MessageHeaderAccessor)));
        }

        [Fact]
        public void GetMutableAccessorSameInstance()
        {
            TestMessageHeaderAccessor expected = new TestMessageHeaderAccessor();
            expected.LeaveMutable = true;
            IMessage<string> message = MessageBuilder<string>.CreateMessage("payload", expected.MessageHeaders);

            MessageHeaderAccessor actual = MessageHeaderAccessor.GetMutableAccessor(message);
            Assert.NotNull(actual);
            Assert.True(actual.Mutable);
            Assert.Same(expected, actual);
        }

        [Fact]
        public void GetMutableAccessorNewInstance()
        {
            IMessage message = MessageBuilder<string>.WithPayload("payload").Build();

            MessageHeaderAccessor actual = MessageHeaderAccessor.GetMutableAccessor(message);
            Assert.NotNull(actual);
            Assert.True(actual.Mutable);
        }

        [Fact]
        public void GetMutableAccessorNewInstanceMatchingType()
        {
            TestMessageHeaderAccessor expected = new TestMessageHeaderAccessor();
            IMessage message = MessageBuilder<string>.CreateMessage("payload", expected.MessageHeaders);

            MessageHeaderAccessor actual = MessageHeaderAccessor.GetMutableAccessor(message);
            Assert.NotNull(actual);
            Assert.True(actual.Mutable);
            Assert.Equal(typeof(TestMessageHeaderAccessor), actual.GetType());
        }

        [Fact]
        public void TimestampEnabled()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            accessor.EnableTimestamp = true;
            Assert.NotNull(accessor.MessageHeaders.Timestamp);
        }

        [Fact]
        public void TimestampDefaultBehavior()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            Assert.Null(accessor.MessageHeaders.Timestamp);
        }

        [Fact]
        public void IdGeneratorCustom()
        {
            Guid id = Guid.NewGuid();
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            accessor.IdGenerator = new TestIdGenerator()
            {
                Id = id
            };
            Assert.Equal(id, accessor.MessageHeaders.Id);
        }

        [Fact]
        public void IdGeneratorDefaultBehavior()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            Assert.NotNull(accessor.MessageHeaders.Id);
        }

        [Fact]
        public void IdTimestampWithMutableHeaders()
        {
            MessageHeaderAccessor accessor = new MessageHeaderAccessor();
            accessor.IdGenerator = new TestIdGenerator()
            {
                Id = MessageHeaders.ID_VALUE_NONE
            };
            accessor.EnableTimestamp = false;
            accessor.LeaveMutable = true;
            IMessageHeaders headers = accessor.MessageHeaders;

            Assert.Null(headers.Id);
            Assert.Null(headers.Timestamp);

            Guid id = Guid.NewGuid();
            accessor.IdGenerator = new TestIdGenerator()
            {
                Id = id
            };

            accessor.EnableTimestamp = true;
            accessor.SetImmutable();

            Assert.Equal(id, accessor.MessageHeaders.Id);
            Assert.NotNull(headers.Timestamp);
        }

        private static IDictionary<string, object> SingletonMap(string key, object value)
        {
            return new Dictionary<string, object>() { { key, value } };
        }

        private class TestIdGenerator : IIDGenerator
        {
            public Guid Id;

            public Guid GenerateId()
            {
                return Id;
            }
        }

        private class TestMessageHeaderAccessor : MessageHeaderAccessor
        {
            public TestMessageHeaderAccessor()
            {
            }

            private TestMessageHeaderAccessor(IMessage message)
            : base(message)
            {
            }

            public static TestMessageHeaderAccessor Wrap(IMessage message)
            {
                return new TestMessageHeaderAccessor(message);
            }

            protected internal override MessageHeaderAccessor CreateAccessor(IMessage message)
            {
                return Wrap(message);
            }
        }
    }
}
