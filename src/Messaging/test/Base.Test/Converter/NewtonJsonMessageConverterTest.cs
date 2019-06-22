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

using Newtonsoft.Json;
using Steeltoe.Common.Util;
using Steeltoe.Messaging.Support;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Steeltoe.Messaging.Converter.Test
{
    public class NewtonJsonMessageConverterTest
    {
        [Fact]
        public void DefaultConstructor()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            Assert.Contains(new MimeType("application", "json", Encoding.UTF8), converter.SupportedMimeTypes);
            Assert.Equal(MissingMemberHandling.Ignore, converter.Settings.MissingMemberHandling);
        }

        [Fact]
        public void MimetypeParametrizedConstructor()
        {
            MimeType mimetype = new MimeType("application", "xml", Encoding.UTF8);
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter(mimetype);
            Assert.Contains(mimetype, converter.SupportedMimeTypes);
            Assert.Equal(MissingMemberHandling.Ignore, converter.Settings.MissingMemberHandling);
        }

        [Fact]
        public void MimetypesParametrizedConstructor()
        {
            MimeType jsonMimetype = new MimeType("application", "json", Encoding.UTF8);
            MimeType xmlMimetype = new MimeType("application", "xml", Encoding.UTF8);
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter(jsonMimetype, xmlMimetype);
            Assert.Contains(jsonMimetype, converter.SupportedMimeTypes);
            Assert.Contains(xmlMimetype, converter.SupportedMimeTypes);
            Assert.Equal(MissingMemberHandling.Ignore, converter.Settings.MissingMemberHandling);
        }

        [Fact]
        public void FromMessage()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            string payload = "{" +
                    "\"bytes\":\"AQI=\"," +
                    "\"array\":[\"Foo\",\"Bar\"]," +
                    "\"number\":42," +
                    "\"string\":\"Foo\"," +
                    "\"bool\":true," +
                    "\"fraction\":42.0}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            IMessage message = MessageBuilder<byte[]>.WithPayload(bytes).Build();
            MyBean actual = (MyBean)converter.FromMessage(message, typeof(MyBean));

            Assert.Equal("Foo", actual.String);
            Assert.Equal(42, actual.Number);
            Assert.Equal(42F, actual.Fraction);
            Assert.Equal(new string[] { "Foo", "Bar" }, actual.Array);
            Assert.True(actual.Bool);
            Assert.Equal(new byte[] { 0x1, 0x2 }, actual.Bytes);
        }

        [Fact]
        public void FromMessageUntyped()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            string payload = "{\"bytes\":\"AQI=\",\"array\":[\"Foo\",\"Bar\"],"
                    + "\"number\":42,\"string\":\"Foo\",\"bool\":true,\"fraction\":42.0}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            IMessage message = MessageBuilder<byte[]>.WithPayload(bytes).Build();

            Dictionary<string, object> actual = converter.FromMessage<Dictionary<string, object>>(message);

            Assert.Equal("Foo", actual["string"]);
            Assert.Equal(42L, actual["number"]);
            Assert.Equal(42D, (double)actual["fraction"]);
            Assert.Equal(new string[] { "Foo", "Bar" }, actual["array"]);
            Assert.Equal(true, actual["bool"]);
            Assert.Equal("AQI=", actual["bytes"]);
        }

        [Fact]
        public void FromMessageMatchingInstance()
        {
            MyBean myBean = new MyBean();
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            IMessage message = MessageBuilder<MyBean>.WithPayload(myBean).Build();
            Assert.Same(myBean, converter.FromMessage(message, typeof(MyBean)));
        }

        [Fact]

        public void FromMessageInvalidJson()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            string payload = "FooBar";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            IMessage message = MessageBuilder<byte[]>.WithPayload(bytes).Build();
            Assert.Throws<MessageConversionException>(() => converter.FromMessage<MyBean>(message));
        }

        [Fact]
        public void FromMessageValidJsonWithUnknownProperty()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            string payload = "{\"string\":\"string\",\"unknownProperty\":\"value\"}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            IMessage message = MessageBuilder<byte[]>.WithPayload(bytes).Build();
            MyBean myBean = converter.FromMessage<MyBean>(message);
            Assert.Equal("string", myBean.String);
        }

        [Fact]
        public void FromMessageToList()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            string payload = "[1, 2, 3, 4, 5, 6, 7, 8, 9]";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            IMessage message = MessageBuilder<byte[]>.WithPayload(bytes).Build();

            var info = GetType().GetMethod("HandleList", BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            object actual = converter.FromMessage(message, typeof(List<long>), info);

            Assert.NotNull(actual);
            Assert.Equal(new List<long>() { 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L }, actual);
        }

        [Fact]
        public void FromMessageToMessageWithPojo()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            string payload = "{\"string\":\"foo\"}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            IMessage message = MessageBuilder<byte[]>.WithPayload(bytes).Build();

            var info = GetType().GetMethod("HandleMessage", BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            object actual = converter.FromMessage(message, typeof(MyBean), info);

            Assert.IsType<MyBean>(actual);
            Assert.Equal("foo", ((MyBean)actual).String);
        }

        [Fact]
        public void ToMessage()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            MyBean payload = new MyBean();
            payload.String = "Foo";
            payload.Number = 42;
            payload.Fraction = 42F;
            payload.Array = new string[] { "Foo", "Bar" };
            payload.Bool = true;
            payload.Bytes = new byte[] { 0x1, 0x2 };

            IMessage message = converter.ToMessage(payload, null);

            string actual = Encoding.UTF8.GetString((byte[])message.Payload);

            Assert.Contains("\"string\":\"Foo\"", actual);
            Assert.Contains("\"number\":42", actual);
            Assert.Contains("\"fraction\":42.0", actual);
            Assert.Contains("\"array\":[\"Foo\",\"Bar\"]", actual);
            Assert.Contains("\"bool\":true", actual);
            Assert.Contains("\"bytes\":\"AQI=\"", actual);
            Assert.Equal(new MimeType("application", "json", Encoding.UTF8), message.Headers[MessageHeaders.CONTENT_TYPE]);
        }

        [Fact]
        public void ToMessageUtf16()
        {
            var e = Encoding.Default;
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            var encoding = new UnicodeEncoding(true, false);
            MimeType contentType = new MimeType("application", "json", encoding);
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add(MessageHeaders.CONTENT_TYPE, contentType);
            MessageHeaders headers = new MessageHeaders(map);
            string payload = "H\u00e9llo W\u00f6rld";
            IMessage message = converter.ToMessage(payload, headers);
            string actual = encoding.GetString((byte[])message.Payload);
            string expected = "\"" + payload + "\"";
            Assert.Equal(expected, actual);
            Assert.Equal(contentType, message.Headers[MessageHeaders.CONTENT_TYPE]);
        }

        [Fact]
        public void ToMessageUtf16String()
        {
            NewtonJsonMessageConverter converter = new NewtonJsonMessageConverter();
            converter.SerializedPayloadClass = typeof(string);
            var encoding = new UnicodeEncoding(true, false);
            MimeType contentType = new MimeType("application", "json", encoding);
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add(MessageHeaders.CONTENT_TYPE, contentType);
            MessageHeaders headers = new MessageHeaders(map);
            string payload = "H\u00e9llo W\u00f6rld";
            IMessage message = converter.ToMessage(payload, headers);

            Assert.Equal("\"" + payload + "\"", message.Payload);
            Assert.Equal(contentType, message.Headers[MessageHeaders.CONTENT_TYPE]);
        }

        [Fact]
        public void GetIMessageGenericType()
        {
            var converter = new NewtonJsonMessageConverter();
            Assert.Null(converter.GetIMessageGenericType(typeof(T1)));
            Assert.Equal(typeof(MyBean), converter.GetIMessageGenericType(typeof(T2)));
            Assert.Equal(typeof(MyBean), converter.GetIMessageGenericType(typeof(T3<MyBean>)));
            Assert.Equal(typeof(MyBean), converter.GetIMessageGenericType(typeof(T4<MyBean>)));
            Assert.Equal(typeof(MyBean), converter.GetIMessageGenericType(typeof(IMessage<MyBean>)));
            Assert.Equal(typeof(MyBean), converter.GetIMessageGenericType(typeof(IMyInterface<MyBean>)));
            Assert.Null(converter.GetIMessageGenericType(typeof(IMessage)));
        }

        internal void HandleList(IList<long> payload)
        {
        }

        internal void HandleMessage(IMessage<MyBean> message)
        {
        }

        public interface IMyInterface<T> : IMessage<T>
        {
        }

        public class T1 : IMessage
        {
            public object Payload => throw new NotImplementedException();

            public IMessageHeaders Headers => throw new NotImplementedException();
        }

        public class T2 : IMessage<MyBean>
        {
            public MyBean Payload => throw new NotImplementedException();

            public IMessageHeaders Headers => throw new NotImplementedException();

            object IMessage.Payload => throw new NotImplementedException();
        }

        public class T4<T> : IMyInterface<T>
        {
            public T Payload => throw new NotImplementedException();

            public IMessageHeaders Headers => throw new NotImplementedException();

            object IMessage.Payload => throw new NotImplementedException();
        }

        public class T3<T> : IMessage<T>
        {
            public T Payload => throw new NotImplementedException();

            public IMessageHeaders Headers => throw new NotImplementedException();

            object IMessage.Payload => throw new NotImplementedException();
        }

        public class MyBean
        {
            public byte[] Bytes { get; set; }

            public bool Bool { get; set; }

            public string String { get; set; }

            public string[] Array { get; set; }

            public int Number { get; set; }

            public float Fraction { get; set; }
        }
    }
}
