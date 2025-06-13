using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Glitch9
{
    public static class EnumUtil
    {
        // cache the enum names
        private static readonly ConcurrentDictionary<Type, Dictionary<Enum, string>> _inspectorNames = new();

        public static string GetInspectorName(this Enum value)
        {
            if (value == null) return string.Empty;

            Type type = value.GetType();
            if (!_inspectorNames.TryGetValue(type, out Dictionary<Enum, string> displayNames))
            {
                displayNames = new Dictionary<Enum, string>();
                _inspectorNames[type] = displayNames;
            }

            if (displayNames.TryGetValue(value, out string displayName))
            {
                return displayName;
            }

            string enumAsString = value.ToString();
            FieldInfo field = type.GetField(enumAsString);

            if (field != null)
            {
                InspectorNameAttribute nameAttribute = AttributeCache<InspectorNameAttribute>.Get(field);

                if (nameAttribute != null)
                {
                    displayNames[value] = nameAttribute.displayName;
                    return nameAttribute.displayName;
                }
            }

            displayNames[value] = enumAsString; // Cache the default name if custom name is not found.
            _inspectorNames[type] = displayNames;

            return enumAsString;
        }

        public static string[] GetDisplayNames(Type enumType)
        {
            string[] enumNames = Enum.GetNames(enumType);
            string[] displayedOptions = new string[enumNames.Length];
            for (int i = 0; i < enumNames.Length; i++)
            {
                Enum value = (Enum)Enum.Parse(enumType, enumNames[i]);
                displayedOptions[i] = value.GetInspectorName();
            }

            return displayedOptions;
        }
    }
}