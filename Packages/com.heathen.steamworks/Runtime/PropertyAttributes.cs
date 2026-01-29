#if !DISABLESTEAMWORKS && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public interface IModularField
    {
        int Priority { get; }
        bool Synchronized { get; }
        string Header { get; }
    }

    [AttributeUsage(System.AttributeTargets.Field)]
    public class SettingsFieldAttribute : PropertyAttribute, IModularField
    {
        public int priority;
        public bool synchronized;
        public string header;

        public SettingsFieldAttribute(int priority = 0, bool synchronized = false, string header = null)
        {
            this.priority = priority;
            this.synchronized = synchronized;
            this.header = header;
        }

        public int Priority => priority;

        public bool Synchronized => synchronized;

        public string Header => header;
    }

    [AttributeUsage(System.AttributeTargets.Field)]
    public class ElementFieldAttribute : PropertyAttribute, IModularField
    {
        public string header;
        public int priority; // lower = drawn first
        public ElementFieldAttribute(string header = null, int priority = 0)
        {
            this.header = header;
            this.priority = priority;
        }

        public int Priority => priority;

        public bool Synchronized => false;

        public string Header => header;
    }

    [AttributeUsage(System.AttributeTargets.Field)]
    public class TemplateFieldAttribute : PropertyAttribute, IModularField
    {
        public string header;
        public int priority; // lower = drawn first
        public TemplateFieldAttribute(string header = null, int priority = 0)
        {
            this.header = header;
            this.priority = priority;
        }

        public int Priority => priority;

        public bool Synchronized => false;

        public string Header => header;
    }

    [AttributeUsage(System.AttributeTargets.Field)]
    public class EventFieldAttribute : PropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModularComponentAttribute : Attribute
    {
        public Type ParentType { get; }
        public string Header { get; }
        public string FieldName { get; }

        public ModularComponentAttribute(Type type, string header, string field)
        {
            ParentType = type;
            Header = header;
            FieldName = field;
        }
    }

    // Marks a component as an Events container
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModularEventsAttribute : Attribute
    {
        public Type ParentType { get; }

        public ModularEventsAttribute(Type type)
        {
            ParentType = type;
        }
    }
}
#endif