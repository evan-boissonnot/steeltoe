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
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Messaging.Support
{
    public class MessageHeaderAccessor
    {
        public static readonly Encoding DEFAULT_CHARSET = Encoding.UTF8;

        private static readonly MimeType[] READABLE_MIME_TYPES = new MimeType[]
        {
            MimeTypeUtils.APPLICATION_JSON, MimeTypeUtils.APPLICATION_XML,
            new MimeType("text", "*"), new MimeType("application", "*+json"), new MimeType("application", "*+xml")
    };

        private readonly MutableMessageHeaders headers;

        private bool leaveMutable = false;

        private bool modified = false;

        private bool enableTimestamp = false;

        private IIDGenerator idGenerator;

        public MessageHeaderAccessor()
        : this(null)
        {
        }

        public MessageHeaderAccessor(IMessage message)
        {
            this.headers = new MutableMessageHeaders(this, message?.Headers);
        }

        public static T GetAccessor<T>(IMessage message, Type requiredType)
            where T : MessageHeaderAccessor
        {
            return GetAccessor<T>(message.Headers, requiredType);
        }

        public static T GetAccessor<T>(IMessageHeaders messageHeaders, Type requiredType)
                 where T : MessageHeaderAccessor
        {
            if (messageHeaders is MutableMessageHeaders)
            {
                MutableMessageHeaders mutableHeaders = (MutableMessageHeaders)messageHeaders;
                MessageHeaderAccessor headerAccessor = mutableHeaders.Accessor;
                if (requiredType == null || requiredType.IsInstanceOfType(headerAccessor))
                {
                    return (T)headerAccessor;
                }
            }

            return null;
        }

        public static MessageHeaderAccessor GetMutableAccessor(IMessage message)
        {
            if (message.Headers is MutableMessageHeaders)
            {
                MutableMessageHeaders mutableHeaders = (MutableMessageHeaders)message.Headers;
                MessageHeaderAccessor accessor = mutableHeaders.Accessor;
                return accessor.Mutable ? accessor : accessor.CreateAccessor(message);
            }

            return new MessageHeaderAccessor(message);
        }

        public virtual bool LeaveMutable
        {
            set
            {
                if (!this.headers.Mutable)
                {
                    throw new InvalidOperationException("Already immutable");
                }

                this.leaveMutable = value;
            }
        }

        public virtual void SetImmutable()
        {
            this.headers.SetImmutable();
        }

        public virtual bool Mutable
        {
            get { return this.headers.Mutable; }
        }

        public virtual bool Modified
        {
            get { return this.modified; }
            set { this.modified = value; }
        }

        public virtual IMessageHeaders MessageHeaders
        {
            get
            {
                if (!this.leaveMutable)
                {
                    SetImmutable();
                }

                return this.headers;
            }
        }

        public virtual IMessageHeaders ToMessageHeaders()
        {
            return new MessageHeaders(this.headers);
        }

        public virtual IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>(this.headers);
        }

        // Generic header accessors
        public virtual object GetHeader(string headerName)
        {
            if (this.headers.TryGetValue(headerName, out object value))
            {
                return value;
            }

            return null;
        }

        public virtual void SetHeader(string name, object value)
        {
            if (IsReadOnly(name))
            {
                throw new ArgumentException("'" + name + "' header is read-only");
            }

            VerifyType(name, value);

            if (value != null)
            {
                // Modify header if necessary
                if (!ObjectUtils.NullSafeEquals(value, GetHeader(name)))
                {
                    this.modified = true;
                    this.headers.RawHeaders[name] = value;
                }
            }
            else
            {
                // Remove header if available
                if (this.headers.ContainsKey(name))
                {
                    this.modified = true;
                    this.headers.RawHeaders.Remove(name);
                }
            }
        }

        public virtual void SetHeaderIfAbsent(string name, object value)
        {
            if (GetHeader(name) == null)
            {
                SetHeader(name, value);
            }
        }

        public virtual void RemoveHeader(string headerName)
        {
            if (!string.IsNullOrEmpty(headerName) && !IsReadOnly(headerName))
            {
                SetHeader(headerName, null);
            }
        }

        public virtual void RemoveHeaders(params string[] headerPatterns)
        {
            List<string> headersToRemove = new List<string>();
            foreach (string pattern in headerPatterns)
            {
                if (!string.IsNullOrEmpty(pattern))
                {
                    if (pattern.Contains("*"))
                    {
                        headersToRemove.AddRange(GetMatchingHeaderNames(pattern, this.headers));
                    }
                    else
                    {
                        headersToRemove.Add(pattern);
                    }
                }
            }

            foreach (string headerToRemove in headersToRemove)
            {
                RemoveHeader(headerToRemove);
            }
        }

        public virtual void CopyHeaders(IDictionary<string, object> headersToCopy)
        {
            if (headersToCopy != null)
            {
                foreach (var kvp in headersToCopy)
                {
                    if (!IsReadOnly(kvp.Key))
                    {
                        SetHeader(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        public virtual void CopyHeadersIfAbsent(IDictionary<string, object> headersToCopy)
        {
            if (headersToCopy != null)
            {
                foreach (var kvp in headersToCopy)
                {
                    if (!IsReadOnly(kvp.Key))
                    {
                        SetHeaderIfAbsent(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        public virtual Guid? Id
        {
            get
            {
                object value = GetHeader(Messaging.MessageHeaders.ID);
                if (value == null)
                {
                    return null;
                }

                return value is Guid ? (Guid)value : Guid.Parse(value.ToString());
            }
        }

        public virtual long? Timestamp
        {
            get
            {
                object value = GetHeader(Messaging.MessageHeaders.TIMESTAMP);
                if (value == null)
                {
                    return null;
                }

                return value is long ? (long)value : long.Parse(value.ToString());
            }
        }

        public virtual MimeType ContentType
        {
            get
            {
                object value = GetHeader(Messaging.MessageHeaders.CONTENT_TYPE);
                if (value == null)
                {
                    return null;
                }

                return value is MimeType ? (MimeType)value : MimeType.ToMimeType(value.ToString());
            }

            set
            {
                SetHeader(Messaging.MessageHeaders.CONTENT_TYPE, value);
            }
        }

        public virtual string ReplyChannelName
        {
            set { SetHeader(Messaging.MessageHeaders.REPLY_CHANNEL, value); }
        }

        public virtual object ReplyChannel
        {
            get { return GetHeader(Messaging.MessageHeaders.REPLY_CHANNEL); }
            set { SetHeader(Messaging.MessageHeaders.REPLY_CHANNEL, value); }
        }

        public virtual string ErrorChannelName
        {
            set { SetHeader(Messaging.MessageHeaders.ERROR_CHANNEL, value); }
        }

        public virtual object ErrorChannel
        {
            get { return GetHeader(Messaging.MessageHeaders.ERROR_CHANNEL); }
            set { SetHeader(Messaging.MessageHeaders.ERROR_CHANNEL, value); }
        }

        // Log message stuff
        public virtual string GetShortLogMessage(object payload)
        {
            return "headers=" + this.headers.ToString() + GetShortPayloadLogMessage(payload);
        }

        public virtual string GetDetailedLogMessage(object payload)
        {
            return "headers=" + this.headers.ToString() + GetDetailedPayloadLogMessage(payload);
        }

        public override string ToString()
        {
            return GetType().Name + " [headers=" + this.headers + "]";
        }

        internal bool EnableTimestamp
        {
            set { this.enableTimestamp = value; }
        }

        internal IIDGenerator IdGenerator
        {
            get { return this.idGenerator; }
            set { this.idGenerator = value; }
        }

        protected internal virtual MessageHeaderAccessor CreateAccessor(IMessage message)
        {
            return new MessageHeaderAccessor(message);
        }

        protected internal virtual bool IsReadOnly(string headerName)
        {
            return Messaging.MessageHeaders.ID.Equals(headerName) || Messaging.MessageHeaders.TIMESTAMP.Equals(headerName);
        }

        protected internal virtual void VerifyType(string headerName, object headerValue)
        {
            if (headerName != null && headerValue != null)
            {
                if (Messaging.MessageHeaders.ERROR_CHANNEL.Equals(headerName) ||
                        Messaging.MessageHeaders.REPLY_CHANNEL.EndsWith(headerName))
                {
                    if (!(headerValue is IMessageChannel || headerValue is string))
                    {
                        throw new ArgumentException(
                                "'" + headerName + "' header value must be a MessageChannel or string");
                    }
                }
            }
        }

        protected internal virtual string GetShortPayloadLogMessage(object payload)
        {
            if (payload is string)
            {
                string payloadText = (string)payload;
                return (payloadText.Length < 80) ?
                    " payload=" + payloadText :
                    " payload=" + payloadText.Substring(0, 80) + "...(truncated)";
            }
            else if (payload is byte[])
            {
                byte[] bytes = (byte[])payload;
                if (IsReadableContentType())
                {
                    return (bytes.Length < 80) ?
                            " payload=" + new string(this.Encoding.GetChars(bytes)) :
                            " payload=" + new string(this.Encoding.GetChars(bytes, 0, 80)) + "...(truncated)";
                }
                else
                {
                    return " payload=byte[" + bytes.Length + "]";
                }
            }
            else
            {
                string payloadText = payload.ToString();
                return (payloadText.Length < 80) ?
                        " payload=" + payloadText :
                        " payload=" + payload.GetType().Name + "@" + payload.ToString();
            }
        }

        protected internal virtual string GetDetailedPayloadLogMessage(object payload)
        {
            if (payload is string)
            {
                return " payload=" + payload;
            }
            else if (payload is byte[])
            {
                byte[] bytes = (byte[])payload;
                if (IsReadableContentType())
                {
                    return " payload=" + new string(this.Encoding.GetChars(bytes));
                }
                else
                {
                    return " payload=byte[" + bytes.Length + "]";
                }
            }
            else
            {
                return " payload=" + payload;
            }
        }

        protected internal virtual bool IsReadableContentType()
        {
            MimeType contentType = ContentType;
            foreach (MimeType mimeType in READABLE_MIME_TYPES)
            {
                if (mimeType.Includes(contentType))
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> GetMatchingHeaderNames(string pattern, IDictionary<string, object> headers)
        {
            if (headers == null)
            {
                return new List<string>();
            }

            List<string> matchingHeaderNames = new List<string>();
            foreach (string key in headers.Keys)
            {
                if (PatternMatchUtils.SimpleMatch(pattern, key))
                {
                    matchingHeaderNames.Add(key);
                }
            }

            return matchingHeaderNames;
        }

        private Encoding Encoding
        {
            get
            {
                MimeType contentType = ContentType;
                Encoding charset = contentType?.Encoding;
                return charset ?? DEFAULT_CHARSET;
            }
        }

        private class MutableMessageHeaders : MessageHeaders
        {
            private readonly MessageHeaderAccessor accessor;
            private bool mutable = true;

            public MutableMessageHeaders(MessageHeaderAccessor accessor, IDictionary<string, object> headers)
            : base(headers, ID_VALUE_NONE, -1L)
            {
                this.accessor = accessor;
            }

            public new IDictionary<string, object> RawHeaders
            {
                get
                {
                    if (!this.mutable)
                    {
                        throw new InvalidOperationException();
                    }

                    return base.RawHeaders;
                }
            }

            public bool Mutable
            {
                get { return this.mutable; }
            }

            public void SetImmutable()
            {
                if (!this.mutable)
                {
                    return;
                }

                if (Id == null)
                {
                    IIDGenerator idGenerator = accessor.IdGenerator != null ? accessor.IdGenerator : IdGenerator;
                    Guid id = idGenerator.GenerateId();
                    if (id != ID_VALUE_NONE)
                    {
                        RawHeaders[ID] = id;
                    }
                }

                if (Timestamp == null)
                {
                    if (accessor.enableTimestamp)
                    {
                        RawHeaders[TIMESTAMP] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                }

                this.mutable = false;
            }

            public MessageHeaderAccessor Accessor
            {
                get { return this.accessor; }
            }
        }
    }
}
