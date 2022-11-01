﻿using System.Text.Json.Serialization;

namespace Hackney.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TargetType
    {
        Person,
        Asset,
        Tenure,
        Repair,
        Process
    }
}