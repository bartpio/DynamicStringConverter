using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DynamicStringConverter
{
    public class DynamicStrings : DynamicObject
    {
        private ReadOnlyDictionary<string, DynamicString> Map { get; }

        /// <summary>
        /// optional custom type conv
        /// </summary>
        private ReadOnlyCollection<TypeConverter> CustomTypeConverters { get; }

        /// <summary>
        /// cons, given enumerable
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="tc">custom type converter; if supplied, we will try these converters prior to trying Convert.ChangeType, in order supplied.
        /// Supplied converters must support converting FROM string. Any converters not supporting this will be ignored.
        /// </param>
        public DynamicStrings(IEnumerable<KeyValuePair<string, string>> strings, IEqualityComparer<string> comparer = null, IEnumerable<TypeConverter> tc = null)
        {
            if (strings == null)
            {
                throw new ArgumentNullException("strings");
            }

            comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
            CustomTypeConverters = tc?.Where(x => x.CanConvertFrom(typeof(string))).ToList().AsReadOnly();
            Map = new ReadOnlyDictionary<string, DynamicString>(strings.ToDictionary(x => x.Key, x => new DynamicString(x.Value, CustomTypeConverters), comparer));
        }

        /// <summary>
        /// cons, given value tuples
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="tc">custom type converter; if supplied, we will try these converters prior to trying Convert.ChangeType, in order supplied.
        /// Supplied converters must support converting FROM string. Any converters not supporting this will be ignored.
        /// </param>
        public DynamicStrings(IEnumerable<(string, string)> strings, IEqualityComparer<string> comparer = null, IEnumerable<TypeConverter> tc = null) : this(AssertValid(strings).Select(x => new KeyValuePair<string, string>(x.Item1, x.Item2)), comparer, tc)
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
    }
}
