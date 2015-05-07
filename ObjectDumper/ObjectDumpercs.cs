using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectDumper
{
    internal class DictionaryWriter
    {
        private readonly IDictionary _dic;
        private const int MaxDataBlock = 4096;

        public DictionaryWriter(IDictionary dic)
        {
            _dic = dic;
        }

        public void Write(string s)
        {
            _dic.Add(s, "Not Provided");
        }
        public void Write(string name, object value, int? index)
        {
            var nameSegments = name.Split('.');
            var length = nameSegments.Length;

            var key = length >= 2 ? nameSegments[length - 2] + "." + nameSegments[length - 1] : name;

            var origKey = key;
            var count = 0;
            while (_dic.Contains(key))
            {
                key = origKey + "[" + ++count + "]";
            }

            var strValue = value.ToString();

            if (strValue.Length > MaxDataBlock)
            {
                strValue = "-- large data block --";
                _dic.Add(key, strValue);
            }
            else
            {
                _dic.Add(key, strValue);
            }
        }

        public static string GetLastOctet(string data)
        {
            return GetLastOctets(data);
        }
        public static string GetLastOctets(string data, int howMany = 1)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            var octets = data.Split('.');
            var length = octets.Length;
            if (howMany <= length)
                return data;

            var result = new StringBuilder();
            for (var i = length - howMany; i <= length; i++)
            {
                result.Append(octets[i]);
                if(i != length)
                    result.Append('.');
            }
            return result.ToString();
        }
    }

    public class ObjectDumper
    {
        public static void Write(object element, IDictionary<string, object> dic)
        {
            var dumper = new ObjectDumper(3, dic as IDictionary);
            dumper.WriteObject(null, element);
        }

        public static void Write(object element, IDictionary<string, object> dic, int depth)
        {
            var dumper = new ObjectDumper(depth, dic as IDictionary);
            dumper.WriteObject(null, element);
        }

        public static void Write(object element, IDictionary dic)
        {
            var dumper = new ObjectDumper(3, dic);
            dumper.WriteObject(null, element);
        }

        public static void Write(object element, IDictionary dic, int depth)
        {
            var dumper = new ObjectDumper(depth, dic);
            dumper.WriteObject(null, element);
        }

        readonly DictionaryWriter _writer;
        int _level;
        readonly int _depth;

        private ObjectDumper(int depth, IDictionary dic)
        {
            _depth = depth;
            _writer = new DictionaryWriter(dic);
        }

        /// <summary>
        /// Writes the object.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="element">The element.</param>
        /// <param name="index">The index.</param>
        private void WriteObject(string prefix, object element, int? index = null)
        {
            if (element == null || element is ValueType || element is string)
            {
                WriteValue(prefix, element, index);
            }
            else
            {
                var enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    if (enumerableElement.Cast<object>().Any(item => item is IEnumerable && !(item is string)))
                    {
                        WriteValue(prefix, enumerableElement, index);
                    }
                }
                else
                {
                    var members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var m in members)
                    {
                        var f = m as FieldInfo;
                        var p = m as PropertyInfo;
                        
                        if (f == null && p == null) 
                            continue;
                        
                        var name = element + "." + m.Name;
                        if (index.HasValue)
                        {
                            name = DictionaryWriter.GetLastOctets(prefix, 2) + "." + DictionaryWriter.GetLastOctet(m.Name);
                        }
                        WriteValue(name, f != null ? f.GetValue(element) : p.GetValue(element, null), index);
                    }

                    if (_level < _depth)
                    {
                        foreach (var m in members)
                        {
                            var f = m as FieldInfo;
                            var p = m as PropertyInfo;
                            if (f != null || p != null)
                            {
                                var t = f != null ? f.FieldType : p.PropertyType;
                                if (!(t.IsValueType || t == typeof(string)))
                                {
                                    var value = f != null ? f.GetValue(element) : p.GetValue(element, null);
                                    if (value != null)
                                    {
                                        _level++;
                                        WriteObject(element + "." + m.Name, value);
                                        _level--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteValue(string name, object o, int? ind)
        {
            if (o == null)
            {
                _writer.Write(name, "null", ind);
            }
            else if (o is DateTime)
            {
                _writer.Write(name, ((DateTime)o).ToShortDateString(), ind);
            }
            else if (o is ValueType || o is string)
            {
                _writer.Write(name, o, ind);
            }
            else if (o is IEnumerable)
            {
                var enumerable = o as IEnumerable;
                var listValues = new StringBuilder();
                var index = 0;
                foreach (var item in enumerable)
                {
                    if (item is ValueType || item is string)
                    {
                        listValues.Append(item);
                        listValues.Append(", ");
                    }
                    else
                    {
                        _writer.Write(name, string.Empty, ind);
                        _level++;
                        WriteObject(name, item, index++);
                        _level--;
                    }
                }

                if(listValues.Length > 0)
                    _writer.Write(name, listValues.ToString(), ind);
            }
        }
    }
}
