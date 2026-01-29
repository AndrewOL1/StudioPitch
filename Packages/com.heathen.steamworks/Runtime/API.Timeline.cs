#if !DISABLESTEAMWORKS && (STEAM_161 || STEAM_162)
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.API
{
    public static class Timeline
    {
        /// <summary>
        /// See the SteamTimeline native documentation for more help
        /// In particular the diagram is useful to sort out where and what each element is
        /// https://partner.steamgames.com/doc/api/ISteamTimeline#functions:~:text=game%20is%20restarted.-,Diagrams,-Steamworks%20is%20the
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RunTimeInit()
            {
                timelineEvents.Clear();
                m_TimelineEventDataArguments.Clear();
                m_SteamTimelineEventRecordingExists_t = null;
                m_SteamTimelineGamePhaseRecordingExists_t = null;
            }

            public static List<TimelineEventData> timelineEvents = new();

            private static readonly Dictionary<ulong, TimelineEventDataArguments> m_TimelineEventDataArguments = new();

            private static CallResult<SteamTimelineEventRecordingExists_t> m_SteamTimelineEventRecordingExists_t = null;
            private static CallResult<SteamTimelineGamePhaseRecordingExists_t> m_SteamTimelineGamePhaseRecordingExists_t = null;

            public static TimelineEventDataArguments GetArguments(TimelineEventData timelineEvent)
            {
                if (m_TimelineEventDataArguments.TryGetValue(timelineEvent, out var args))
                    return args;
                else
                    return default;
            }
            /// <summary>
            /// Sets a description (B) for the current game state in the timeline. These help the user to find specific moments in the timeline when saving clips. Setting a new state description replaces any previous description.
            /// </summary>
            /// <param name="description"></param>
            /// <param name="timeDelta">The time offset in seconds to apply to this state change. Negative times indicate an event that happened in the past.</param>
            public static void SetTimelineTooltip(string description, float timeDelta) => SteamTimeline.SetTimelineTooltip(description, timeDelta);

            /// <summary>
            /// Clears the previous set game state in the timeline.
            /// </summary>
            /// <param name="timeDelta">The time offset in seconds to apply to this state change. Negative times indicate an event that happened in the past.</param>
            public static void ClearTimelineTooltip(float timeDelta) => SteamTimeline.ClearTimelineTooltip(timeDelta);

            /// <summary>
            /// Use this to mark an event (A) on the Timeline. This event will be instantaneous. (See AddRangeTimelineEvent to add events that happened over time.)
            /// </summary>
            /// <param name="title"></param>
            /// <param name="description"></param>
            /// <param name="icon">The name of the icon to show at the timeline at this point. This can be one of the icons uploaded through the Steamworks partner Site for your title, or one of the provided icons that start with steam_. The Steam Timelines overview includes a list of available icons.</param>
            /// <param name="priority">Provide the priority to use when the UI is deciding which icons to display in crowded parts of the timeline. Events with larger priority values will be displayed more prominently than events with smaller priority values. This value must be between 0 and k_unMaxTimelinePriority.</param>
            /// <param name="startOffsetSeconds">The time offset in seconds to apply to the start of the event. Negative times indicate an event that happened in the past.
            /// <para>One use of this parameter is to handle events whose significance is not clear until after the fact. For instance if the player starts a damage over time effect on another player, which kills them 3.5 seconds later, the game could pass -3.5 as the start offset and cause the event to appear in the timeline where the effect started.</para></param>
            /// <param name="possibleClip">Allows the game to describe events that should be suggested to the user as possible video clips.</param>
            /// <returns></returns>
            public static TimelineEventData AddInstantaneousTimelineEvent(string title, string description, string icon, uint priority, float startOffsetSeconds, ETimelineEventClipPriority possibleClip)
            {
                var handle = SteamTimeline.AddInstantaneousTimelineEvent(title, description, icon, priority, startOffsetSeconds, possibleClip);
                timelineEvents.Add(handle);
                m_TimelineEventDataArguments.TryAdd(handle.m_TimelineEventHandle, new()
                {
                    title = title,
                    description = description,
                    icon = icon,
                    priority = priority,
                    startSeconds = Time.time + startOffsetSeconds,
                    durationSeconds = 0,
                    possibleClip = possibleClip
                });
                return handle;
            }

            /// <summary>
            /// Use this to mark an event (A) on the Timeline that takes some amount of time to complete.
            /// </summary>
            /// <param name="title"></param>
            /// <param name="description"></param>
            /// <param name="icon">The name of the icon to show at the timeline at this point. This can be one of the icons uploaded through the Steamworks partner Site for your title, or one of the provided icons that start with steam_. The Steam Timelines overview includes a list of available icons.</param>
            /// <param name="priority">Provide the priority to use when the UI is deciding which icons to display in crowded parts of the timeline. Events with larger priority values will be displayed more prominently than events with smaller priority values. This value must be between 0 and k_unMaxTimelinePriority.</param>
            /// <param name="startOffsetSeconds">The time offset in seconds to apply to the start of the event. Negative times indicate an event that happened in the past.
            /// <para>One use of this parameter is to handle events whose significance is not clear until after the fact. For instance if the player starts a damage over time effect on another player, which kills them 3.5 seconds later, the game could pass -3.5 as the start offset and cause the event to appear in the timeline where the effect started.</para></param>
            /// <param name="durationSeconds">The duration of the event, in seconds. Pass 0 for instantaneous events.</param>
            /// <param name="possibleClip">Allows the game to describe events that should be suggested to the user as possible video clips.</param>
            /// <returns></returns>
            public static TimelineEventData AddRangeTimelineEvent(string title, string description, string icon, uint priority, float startOffsetSeconds, float durationSeconds, ETimelineEventClipPriority possibleClip)
            {
                var handle = SteamTimeline.AddRangeTimelineEvent(title, description, icon, priority, startOffsetSeconds, durationSeconds, possibleClip);
                timelineEvents.Add(handle);
                m_TimelineEventDataArguments.TryAdd(handle.m_TimelineEventHandle, new()
                {
                    title = title,
                    description = description,
                    icon = icon,
                    priority = priority,
                    startSeconds = Time.time + startOffsetSeconds,
                    durationSeconds = durationSeconds,
                    possibleClip = possibleClip
                });
                return handle;
            }

            /// <summary>
            /// Use this to mark the start of an event (A) on the Timeline that takes some amount of time to complete. The duration of the event is determined by a matching call to EndRangeTimelineEvent. If the game wants to cancel an event in progress, they can do that with a call to RemoveTimelineEvent.
            /// </summary>
            /// <param name="title"></param>
            /// <param name="description"></param>
            /// <param name="icon">The name of the icon to show at the timeline at this point. This can be one of the icons uploaded through the Steamworks partner Site for your title, or one of the provided icons that start with steam_. The Steam Timelines overview includes a list of available icons.</param>
            /// <param name="priority">Provide the priority to use when the UI is deciding which icons to display in crowded parts of the timeline. Events with larger priority values will be displayed more prominently than events with smaller priority values. This value must be between 0 and k_unMaxTimelinePriority.</param>
            /// <param name="startOffsetSeconds">The time offset in seconds to apply to the start of the event. Negative times indicate an event that happened in the past.
            /// <para>One use of this parameter is to handle events whose significance is not clear until after the fact. For instance if the player starts a damage over time effect on another player, which kills them 3.5 seconds later, the game could pass -3.5 as the start offset and cause the event to appear in the timeline where the effect started.</para></param>
            /// <param name="possibleClip">Allows the game to describe events that should be suggested to the user as possible video clips.</param>
            /// <returns></returns>
            public static TimelineEventData StartRangeTimelineEvent( string title, string description, string icon, uint priority, float startOffsetSeconds, ETimelineEventClipPriority possibleClip )
            {
                var handle = SteamTimeline.StartRangeTimelineEvent(title, description, icon, priority, startOffsetSeconds, possibleClip);
                timelineEvents.Add(handle);
                m_TimelineEventDataArguments.TryAdd(handle.m_TimelineEventHandle, new()
                {
                    title = title,
                    description = description,
                    icon = icon,
                    priority = priority,
                    startSeconds = Time.time + startOffsetSeconds,
                    durationSeconds = -1,
                    possibleClip = possibleClip
                });
                return handle;
            }

            /// <summary>
            /// Use this to update the details of an event that was started with <see cref="StartRangeTimelineEvent"/>.
            /// </summary>
            /// <param name="title"></param>
            /// <param name="description"></param>
            /// <param name="icon">The name of the icon to show at the timeline at this point. This can be one of the icons uploaded through the Steamworks partner Site for your title, or one of the provided icons that start with steam_. The Steam Timelines overview includes a list of available icons.</param>
            /// <param name="priority">Provide the priority to use when the UI is deciding which icons to display in crowded parts of the timeline. Events with larger priority values will be displayed more prominently than events with smaller priority values. This value must be between 0 and k_unMaxTimelinePriority.</param>
            /// <param name="possibleClip">Allows the game to describe events that should be suggested to the user as possible video clips.</param>
            public static void UpdateRangeTimelineEvent(TimelineEventData timelineEvent, string title, string description, string icon, uint priority, ETimelineEventClipPriority possibleClip)
            {
                SteamTimeline.UpdateRangeTimelineEvent(timelineEvent, title, description, icon, priority, possibleClip);

                if (m_TimelineEventDataArguments.TryGetValue(timelineEvent, out var handle))
                {
                    handle.title = title;
                    handle.description = description;
                    handle.icon = icon;
                    handle.priority = priority;
                    handle.possibleClip = possibleClip;
                    m_TimelineEventDataArguments[timelineEvent] = handle;
                }
            }

            /// <summary>
            /// Use this to end an event (A) of the timeline that was started with <see cref="StartRangeTimelineEvent"/>.
            /// </summary>
            /// <param name="timelineEvent"></param>
            /// <param name="endOffsetSeconds">The time offset in seconds to apply to the end of the event. Negative times indicate an event that happened in the past.</param>
            public static void EndRangeTimelineEvent(TimelineEventData timelineEvent, float endOffsetSeconds)
            {
                SteamTimeline.EndRangeTimelineEvent(timelineEvent, endOffsetSeconds);
                if(m_TimelineEventDataArguments.TryGetValue(timelineEvent, out var handle))
                {
                    var endTime = Time.time + endOffsetSeconds;
                    handle.durationSeconds = endTime - handle.startSeconds;
                    m_TimelineEventDataArguments[timelineEvent] = handle;
                }
            }

            /// <summary>
            /// Use this to remove an event that was added with <see cref="AddInstantaneousTimelineEvent"/> or <see cref="AddRangeTimelineEvent"/> or an event that is in progress and was started with <see cref="StartRangeTimelineEvent"/>.
            /// </summary>
            /// <param name="timelineEvent"></param>
            public static void RemoveTimelineEvent(TimelineEventData timelineEvent)
            {
                timelineEvents.Remove(timelineEvent);
                m_TimelineEventDataArguments.Remove(timelineEvent);
                SteamTimeline.RemoveTimelineEvent(timelineEvent);
            }

            /// <summary>
            /// Use this to determine if video recordings exist for the specified event.
            /// </summary>
            /// <param name="timelineEvent"></param>
            /// <param name="callback">Invoked when Steam responds with the result</param>
            public static void DoesEventRecordingExist(TimelineEventData timelineEvent, Action<bool> callback)
            {
                if(callback == null)
                    return;

                m_SteamTimelineEventRecordingExists_t ??= CallResult<SteamTimelineEventRecordingExists_t>.Create();

                var handle = SteamTimeline.DoesEventRecordingExist(timelineEvent);
                m_SteamTimelineEventRecordingExists_t.Set(handle, (r, e) => { callback.Invoke(r.m_bRecordingExists); });
            }

            /// <summary>
            /// Use this to start a game phase. Game phases allow the user to navigate their background recordings and clips. Exactly what a game phase means will vary game to game, but the game phase should be a section of gameplay that is usually between 10 minutes and a few hours in length, and should be the main way a user would think to divide up the game. These are presented to the user in a UI that shows the date the game was played, with one row per game slice. Game phases should be used to mark sections of gameplay that the user might be interested in watching.
            /// </summary>
            public static void StartGamePhase() => SteamTimeline.StartGamePhase();

            /// <summary>
            /// Use this to end a game phase
            /// </summary>
            public static void EndGamePhase() => SteamTimeline.EndGamePhase();

            /// <summary>
            /// The phase ID is used to let the game identify which phase it is referring to
            /// </summary>
            /// <param name="id"></param>
            public static void SetGamePhaseId(string id) => SteamTimeline.SetGamePhaseID(id);

            /// <summary>
            /// Use this to determine if video recordings exist for the specified game phase. Steam will sent a SteamTimelineGamePhaseRecordingExists_t callback with the result. This can be useful when the game needs to decide whether or not to show a control that will call OpenOverlayToGamePhase.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="callback">Invoked when Steam responds with the results</param>
            public static void DoesGamePhaseRecordingExist(string id, Action<SteamTimelineGamePhaseRecordingExists_t> callback)
            {
                if (callback == null)
                    return;

                m_SteamTimelineGamePhaseRecordingExists_t ??= CallResult<SteamTimelineGamePhaseRecordingExists_t>.Create();

                var handle = SteamTimeline.DoesGamePhaseRecordingExist(id);
                m_SteamTimelineGamePhaseRecordingExists_t.Set(handle, (r, e) => { callback.Invoke(r); });
            }

            /// <summary>
            /// Use this to add a game phase tag (F). Phase tags represent data with a well defined set of options, which could be data such as match resolution, hero played, game mode, etc. Tags can have an icon in addition to a text name. Multiple tags within the same group may be added per phase and all will be remembered. For example, AddGamePhaseTag may be called multiple times for a "Bosses Defeated" group, with different names and icons for each boss defeated during the phase, all of which will be shown to the user.
            /// </summary>
            /// <param name="tagName">Title-provided localized string in the language returned by SteamUtils()->GetSteamUILanguage().</param>
            /// <param name="tagIcon">The name of the icon to show when the tag is shown in the UI. This can be one of the icons uploaded through the Steamworks partner Site for your title, or one of the provided icons that start with steam_. The Steam Timelines overview includes a list of available icons.</param>
            /// <param name="tagGroup">Title-provided localized string in the language returned by SteamUtils()->GetSteamUILanguage(). Tags within the same group will be shown together in the UI.</param>
            /// <param name="priority">Provide the priority to use when the UI is deciding which icons to display. Tags with larger priority values will be displayed more prominently than tags with smaller priority values. This value must be between 0 and k_unMaxTimelinePriority.</param>
            public static void AddGamePhaseTag( string tagName, string tagIcon, string tagGroup, uint priority) => SteamTimeline.AddGamePhaseTag(tagName, tagIcon, tagGroup, priority);

            /// <summary>
            /// Use this to add a game phase attribute (E). Phase attributes represent generic text fields that can be updated throughout the duration of the phase. They are meant to be used for phase metadata that is not part of a well defined set of options. For example, a KDA attribute that starts with the value "0/0/0" and updates as the phase progresses, or something like a player-entered character name. Attributes can be set as many times as the game likes with SetGamePhaseAttribute, and only the last value will be shown to the user.
            /// </summary>
            /// <param name="attributeGroup"></param>
            /// <param name="attributeValue"></param>
            /// <param name="priority"></param>
            public static void SetGamePhaseAttribute(string attributeGroup, string attributeValue, uint priority) => SteamTimeline.SetGamePhaseAttribute(attributeGroup, attributeValue, priority);

            /// <summary>
            /// Changes the color of the timeline bar (C). See ETimelineGameMode for how to use each value.
            /// </summary>
            /// <param name="mode"></param>
            public static void SetTimelineGameMode(ETimelineGameMode mode) => SteamTimeline.SetTimelineGameMode(mode);

            /// <summary>
            /// Opens the Steam overlay to the section of the timeline represented by the game phase.
            /// </summary>
            /// <param name="phaseId"></param>
            public static void OpenOverlayToGamePhase(string phaseId) => SteamTimeline.OpenOverlayToGamePhase(phaseId);

            /// <summary>
            /// Opens the Steam overlay to the section of the timeline represented by the timeline event. This event must be in the current game session, since TimelineEvent values are not valid for future runs of the game.
            /// </summary>
            /// <param name="timelineEvent"></param>
            public static void OpenOverlayToTimelineEvent(TimelineEventData timelineEvent) => SteamTimeline.OpenOverlayToTimelineEvent(timelineEvent);
        }
    }
}
#endif