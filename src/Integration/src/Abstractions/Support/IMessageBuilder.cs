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
using System;
using System.Collections.Generic;

namespace Steeltoe.Integration.Support
{
    public interface IMessageBuilder<T>
    {
        IMessageBuilder<T> SetExpirationDate(long expirationDate);

        IMessageBuilder<T> SetExpirationDate(DateTime expirationDate);

        IMessageBuilder<T> SetCorrelationId(object correlationId);

        IMessageBuilder<T> PushSequenceDetails(object correlationId, int sequenceNumber, int sequenceSize);

        IMessageBuilder<T> PopSequenceDetails();

        IMessageBuilder<T> SetReplyChannel(IMessageChannel replyChannel);

        IMessageBuilder<T> SetReplyChannelName(string replyChannelName);

        IMessageBuilder<T> SetErrorChannel(IMessageChannel errorChannel);

        IMessageBuilder<T> SetErrorChannelName(string errorChannelName);

        IMessageBuilder<T> SetSequenceNumber(int sequenceNumber);

        IMessageBuilder<T> SetSequenceSize(int sequenceSize);

        IMessageBuilder<T> SetPriority(int priority);

        IMessageBuilder<T> FilterAndCopyHeadersIfAbsent(IDictionary<string, object> headersToCopy, params string[] headerPatternsToFilter);

        T Payload { get; }

        IDictionary<string, object> Headers { get; }

        IMessageBuilder<T> SetHeader(string headerName, object headerValue);

        IMessageBuilder<T> SetHeaderIfAbsent(string headerName, object headerValue);

        IMessageBuilder<T> RemoveHeaders(params string[] headerPatterns);

        IMessageBuilder<T> RemoveHeader(string headerName);

        IMessageBuilder<T> CopyHeaders(IDictionary<string, object> headersToCopy);

        IMessageBuilder<T> CopyHeadersIfAbsent(IDictionary<string, object> headersToCopy);

        IMessage<T> Build();
    }
}
