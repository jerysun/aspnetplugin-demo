using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EndpintPDK;

public static class JsonSerializer
{
    public static string Serialize(object obj)
    {
        if (obj == null) return "null";

        var sb = new StringBuilder();

        switch (obj)
        {
            case string str:
                sb.Append('"');
                sb.Append(EscapeString(str));
                sb.Append('"');
                break;
            case bool b:
                sb.Append(b ? "true" : "false");
                break;
            case IConvertible num when IsNumeric(num):
                sb.Append(Convert.ToString(num, CultureInfo.InvariantCulture));
                break;
            case IDictionary dictionary:
                SerializeDictionary(dictionary, sb);
                break;
            case IEnumerable enumerable when !(obj is string):
                SerializeArray(enumerable, sb);
                break;
            default:
                SerializeObject(obj, sb);
                break;
        }

        return sb.ToString();
    }

    private static void SerializeObject(object obj, StringBuilder sb)
    {
        sb.Append('{');
        var first = true;
        var properties = obj.GetType().GetProperties();

        foreach (var prop in properties)
        {
            if (!first) sb.Append(',');
            first = false;

            sb.Append('"');
            sb.Append(prop.Name);
            sb.Append('"');
            sb.Append(':');

            var value = prop.GetValue(obj);
            sb.Append(Serialize(value));
        }

        sb.Append('}');
    }

    private static void SerializeDictionary(IDictionary dictionary, StringBuilder sb)
    {
        sb.Append('{');
        var first = true;

        foreach (DictionaryEntry entry in dictionary)
        {
            if (!first) sb.Append(',');
            first = false;

            sb.Append('"');
            sb.Append(entry.Key.ToString());
            sb.Append('"');
            sb.Append(':');
            sb.Append(Serialize(entry.Value));
        }

        sb.Append('}');
    }

    private static void SerializeArray(IEnumerable enumerable, StringBuilder sb)
    {
        sb.Append('[');
        var first = true;

        foreach (var item in enumerable)
        {
            if (!first) sb.Append(',');
            first = false;
            sb.Append(Serialize(item));
        }

        sb.Append(']');
    }

    private static string EscapeString(string str)
    {
        var sb = new StringBuilder();
        foreach (var c in str)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < ' ')
                    {
                        sb.AppendFormat("\\u{0:x4}", (int)c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    private static bool IsNumeric(IConvertible obj)
    {
        return obj is sbyte || obj is byte || obj is short || obj is ushort
            || obj is int || obj is uint || obj is long || obj is ulong
            || obj is float || obj is double || obj is decimal;
    }

    public static T Deserialize<T>(string json)
    {
        return (T)Deserialize(json, typeof(T));
    }

    public static object Deserialize(string json, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON string cannot be null or empty.");

        var index = 0;
        return ParseValue(json, ref index, targetType);
    }

    private static object ParseValue(string json, ref int index, Type targetType)
    {
        SkipWhitespace(json, ref index);

        var currentChar = json[index];
        if (currentChar == '"')
        {
            return ParseString(json, ref index);
        }
        else if (currentChar == '{')
        {
            return ParseObject(json, ref index, targetType);
        }
        else if (currentChar == '[')
        {
            return ParseArray(json, ref index, targetType);
        }
        else if (char.IsDigit(currentChar) || currentChar == '-')
        {
            return ParseNumber(json, ref index, targetType);
        }
        else if (currentChar == 't' || currentChar == 'f')
        {
            return ParseBoolean(json, ref index);
        }
        else if (currentChar == 'n')
        {
            ParseNull(json, ref index);
            return null;
        }

        throw new FormatException($"Unexpected character '{currentChar}' at position {index}");
    }

    private static void SkipWhitespace(string json, ref int index)
    {
        while (index < json.Length && char.IsWhiteSpace(json[index]))
        {
            index++;
        }
    }

    private static string ParseString(string json, ref int index)
    {
        index++; // skip opening quote
        var sb = new StringBuilder();

        while (index < json.Length)
        {
            var c = json[index++];

            if (c == '"')
            {
                return sb.ToString();
            }
            else if (c == '\\')
            {
                if (index >= json.Length) break;

                c = json[index++];
                switch (c)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (index + 4 <= json.Length)
                        {
                            var hex = json.Substring(index, 4);
                            sb.Append((char)Convert.ToInt32(hex, 16));
                            index += 4;
                        }
                        break;
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        throw new FormatException("Unterminated string");
    }

    private static object ParseNumber(string json, ref int index, Type targetType)
    {
        var startIndex = index;

        if (json[index] == '-') index++;

        while (index < json.Length && char.IsDigit(json[index]))
        {
            index++;
        }

        if (index < json.Length && json[index] == '.')
        {
            index++;
            while (index < json.Length && char.IsDigit(json[index]))
            {
                index++;
            }
        }

        if (index < json.Length && (json[index] == 'e' || json[index] == 'E'))
        {
            index++;
            if (index < json.Length && (json[index] == '+' || json[index] == '-'))
            {
                index++;
            }
            while (index < json.Length && char.IsDigit(json[index]))
            {
                index++;
            }
        }

        var numStr = json.Substring(startIndex, index - startIndex);

        if (targetType == typeof(int) || targetType == typeof(int?))
            return int.Parse(numStr, CultureInfo.InvariantCulture);
        if (targetType == typeof(long) || targetType == typeof(long?))
            return long.Parse(numStr, CultureInfo.InvariantCulture);
        if (targetType == typeof(float) || targetType == typeof(float?))
            return float.Parse(numStr, CultureInfo.InvariantCulture);
        if (targetType == typeof(double) || targetType == typeof(double?))
            return double.Parse(numStr, CultureInfo.InvariantCulture);
        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            return decimal.Parse(numStr, CultureInfo.InvariantCulture);

        return double.Parse(numStr, CultureInfo.InvariantCulture);
    }

    private static bool ParseBoolean(string json, ref int index)
    {
        if (json.Substring(index, 4) == "true")
        {
            index += 4;
            return true;
        }
        else if (json.Substring(index, 5) == "false")
        {
            index += 5;
            return false;
        }

        throw new FormatException("Invalid boolean value");
    }

    private static void ParseNull(string json, ref int index)
    {
        if (json.Substring(index, 4) == "null")
        {
            index += 4;
            return;
        }

        throw new FormatException("Invalid null value");
    }

    private static object ParseObject(string json, ref int index, Type targetType)
    {
        index++; // skip opening brace

        var obj = Activator.CreateInstance(targetType);
        var properties = targetType.GetProperties();
        var propertyMap = new Dictionary<string, System.Reflection.PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in properties)
        {
            propertyMap[prop.Name] = prop;
        }

        SkipWhitespace(json, ref index);

        while (index < json.Length && json[index] != '}')
        {
            var propName = ParseString(json, ref index);

            SkipWhitespace(json, ref index);
            if (json[index++] != ':')
                throw new FormatException("Expected ':' after property name");

            SkipWhitespace(json, ref index);

            if (propertyMap.TryGetValue(propName, out var property))
            {
                var value = ParseValue(json, ref index, property.PropertyType);
                property.SetValue(obj, value);
            }
            else
            {
                // Skip the value if property not found
                ParseValue(json, ref index, typeof(object));
            }

            SkipWhitespace(json, ref index);
            if (index < json.Length && json[index] == ',')
            {
                index++;
                SkipWhitespace(json, ref index);
            }
        }

        if (index >= json.Length || json[index] != '}')
            throw new FormatException("Unterminated object");

        index++; // skip closing brace
        return obj;
    }

    private static object ParseArray(string json, ref int index, Type targetType)
    {
        index++; // skip opening bracket

        var elementType = targetType.IsArray ?
            targetType.GetElementType() :
            (targetType.IsGenericType ? targetType.GetGenericArguments()[0] : typeof(object));

        var list = new List<object>();

        SkipWhitespace(json, ref index);

        while (index < json.Length && json[index] != ']')
        {
            var value = ParseValue(json, ref index, elementType);
            list.Add(value);

            SkipWhitespace(json, ref index);
            if (index < json.Length && json[index] == ',')
            {
                index++;
                SkipWhitespace(json, ref index);
            }
        }

        if (index >= json.Length || json[index] != ']')
            throw new FormatException("Unterminated array");

        index++; // skip closing bracket

        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                array.SetValue(list[i], i);
            }
            return array;
        }
        else if (targetType.IsGenericType)
        {
            var constructedList = typeof(List<>).MakeGenericType(elementType);
            var addMethod = constructedList.GetMethod("Add");
            var result = Activator.CreateInstance(constructedList);

            foreach (var item in list)
            {
                addMethod.Invoke(result, new[] { item });
            }

            return result;
        }

        return list.ToArray();
    }
}