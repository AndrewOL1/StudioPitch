#if !DISABLESTEAMWORKS  && (STEAM_161 || STEAM_162)
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct TimelineEventData : IEquatable<TimelineEventHandle_t>, IEquatable<ulong>, IComparable<TimelineEventHandle_t>, IComparable<ulong>
    {
        [SerializeField]
        private TimelineEventHandle_t handle;

        public readonly TimelineEventHandle_t Handle => handle;

        public readonly ulong Id => handle.m_TimelineEventHandle;

        public readonly TimelineEventDataArguments Arguments => API.Timeline.Client.GetArguments(this);

        #region Boilerplate
        public readonly int CompareTo(TimelineEventData other)
        {
            return Id.CompareTo(other.Id);
        }

        public readonly int CompareTo(TimelineEventHandle_t other)
        {
            return Id.CompareTo(other.m_TimelineEventHandle);
        }

        public readonly int CompareTo(ulong other)
        {
            return Id.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return Id.ToString();
        }

        public readonly bool Equals(TimelineEventData other)
        {
            return Id.Equals(other.Id);
        }

        public readonly bool Equals(TimelineEventHandle_t other)
        {
            return Id.Equals(other.m_TimelineEventHandle);
        }

        public readonly bool Equals(ulong other)
        {
            return Id.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return Id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TimelineEventData l, TimelineEventData r) => l.Id == r.Id;
        public static bool operator ==(TimelineEventHandle_t l, TimelineEventData r) => l.m_TimelineEventHandle == r.Id;
        public static bool operator ==(TimelineEventData l, TimelineEventHandle_t r) => l.Id == r.m_TimelineEventHandle;
        public static bool operator !=(TimelineEventData l, TimelineEventData r) => l.Id != r.Id;
        public static bool operator !=(TimelineEventHandle_t l, TimelineEventData r) => l.m_TimelineEventHandle != r.Id;
        public static bool operator !=(TimelineEventData l, TimelineEventHandle_t r) => l.Id != r.m_TimelineEventHandle;

        public static implicit operator TimelineEventData(TimelineEventHandle_t value) => new TimelineEventData { handle = value };
        public static implicit operator ulong(TimelineEventData c) => c.Id;
        public static implicit operator TimelineEventData(ulong id) => new TimelineEventData { handle = new TimelineEventHandle_t(id) };
        public static implicit operator TimelineEventHandle_t(TimelineEventData c) => c.handle;
        #endregion
    }

    [Serializable]
    public struct TimelineEventDataArguments
    {
        public string title;
        public string description;
        public string icon;
        public uint priority;
        public float startSeconds;
        public float durationSeconds;
        public ETimelineEventClipPriority possibleClip;
    }
}
#endif