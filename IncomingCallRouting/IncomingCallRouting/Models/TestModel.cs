// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace IncomingCallRouting.Models
{
    public class TestModel
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}