using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;

namespace DynamicStringConverter
{
    /// <summary>
    /// Dynamic Strings holder
    /// Represents (string, string) key value pairs, with dynamic auto-convert access to the values
    /// </summary>
    public class DynamicStrings : DynamicObject, IDictionary<string, string>
    {
        /// <summary>
        /// main map storage
        /// </summary>
        private ReadOnlyDictionary<string, DynamicString> Map { get; }

        /// <summary>
        /// Pull a stringform rep for interface adaptation
        /// </summary>
        private ReadOnlyDictionary<string, string> Stringform => new ReadOnlyDictionary<string, string>(Map.ToDictionary(x => x.Key, x => x.Value?.ToString()));

        /// <summary>
        /// optional custom type conv
        /// </summary>
        private ReadOnlyCollection<TypeConverter> CustomTypeConverters { get; }

      

        /// <summary>
        /// cons, given enumerable
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="dso">conversion options</param>
        /// <param name="tc">custom type converter; if supplied, we will try these converters prior to trying Convert.ChangeType, in order supplied.
        /// Supplied converters must support converting FROM string. Any converters not supporting this will be ignored.
        /// </param>
        public DynamicStrings(IEnumerable<KeyValuePair<string, string>> strings, IEqualityComparer<string> comparer = null, DynamicStringOptions dso = DynamicStringOptions.None, IEnumerable<TypeConverter> tc = null)
        {
            if (strings == null)
            {
                throw new ArgumentNullException("strings");
            }

            comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
            CustomTypeConverters = tc?.Where(x => x.CanConvertFrom(typeof(string))).ToList().AsReadOnly();
            //for nulls the val will be direct null.
            Map = new ReadOnlyDictionary<string, DynamicString>(strings.ToDictionary(x => x.Key, x => x.Value != null ? new DynamicString(x.Value, dso, CustomTypeConverters) : null, comparer));
        }

        /// <summary>
        /// cons, given value tuples
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="tc">custom type converter; if supplied, we will try these converters prior to trying Convert.ChangeType, in order supplied.
        /// Supplied converters must support converting FROM string. Any converters not supporting this will be ignored.
        /// </param>
        public DynamicStrings(IEnumerable<(string, string)> strings, IEqualityComparer<string> comparer = null, IEnumerable<TypeConverter> tc = null) 
            : this(AssertValid(strings).Select(x => new KeyValuePair<string, string>(x.Item1, x.Item2)), comparer, DynamicStringOptions.None, tc)
        {
        }

        /// <summary>
        /// extremely pedantic method to throw ArgumentNullException specifically if invalid
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        private static IEnumerable<(string, string)> AssertValid(IEnumerable<(string, string)> strings)
        {
            if (strings == null)
            {
                throw new ArgumentNullException("strings");
            }

            return strings;
        }

        /// <summary>
        /// try get
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Map.TryGetValue(binder.Name, out var sresult))
            {
                result = sresult;
                //result = (sresult.ToString() != null) ? sresult : null;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// no support
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new NotSupportedException("DynamicStrings is immutable");
        }

        /// <summary>
        /// no support
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            throw new NotSupportedException("DynamicStrings is immutable");
        }

        /// <summary>
        /// Get dynamic member names.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Map.Keys.ToList().AsReadOnly();
        }


        #region IDictionary impl

        public ICollection<string> Keys => ((IDictionary<string, string>)Stringform).Keys;

        public ICollection<string> Values => ((IDictionary<string, string>)Stringform).Values;

        public int Count => ((IDictionary<string, string>)Stringform).Count;

        public bool IsReadOnly => ((IDictionary<string, string>)Stringform).IsReadOnly;

        public string this[string key] { get => ((IDictionary<string, string>)Stringform)[key]; set => ((IDictionary<string, string>)Stringform)[key] = value; }

        public void Add(string key, string value)
        {
            ((IDictionary<string, string>)Stringform).Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, string>)Stringform).ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, string>)Stringform).Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return ((IDictionary<string, string>)Stringform).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            ((IDictionary<string, string>)Stringform).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, string>)Stringform).Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return ((IDictionary<string, string>)Stringform).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((IDictionary<string, string>)Stringform).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return ((IDictionary<string, string>)Stringform).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return ((IDictionary<string, string>)Stringform).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, string>)Stringform).GetEnumerator();
        }
        #endregion
    }
}
