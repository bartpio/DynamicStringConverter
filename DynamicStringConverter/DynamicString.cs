using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DynamicStringConverter
{
    /// <summary>
    /// dynamic string with magic conversion helper
    /// uses Convert.ChangeType or supplied converter
    /// </summary>
    internal class DynamicString : DynamicObject
    {
        /// <summary>
        /// internal rep
        /// </summary>
        private string Str { get;  }

        /// <summary>
        /// optional custom type conv
        /// </summary>
        private ReadOnlyCollection<TypeConverter> CustomTypeConverters { get; }

        /// <summary>
        /// cons
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tc">custom type converter; if supplied, we will try these converters prior to trying Convert.ChangeType, in order supplied.
        /// Supplied converters must support converting FROM string, for proper functionality.
        /// </param>
        public DynamicString(string str, ReadOnlyCollection<TypeConverter> tc = null)
        {
            Str = str;
            CustomTypeConverters = tc;
        }

        /// <summary>
        /// find a custom converter that can actually convert Str
        /// </summary>
        /// <returns>semi applicable converters</returns>
        private IEnumerable<TypeConverter> FindCustomConverters()
        {
            if (CustomTypeConverters != null)
            {
                return CustomTypeConverters.Where(x => x.IsValid(Str));
            }
            else
            {
                return new TypeConverter[] { };
            }
        }
        
        /// <summary>
        /// find custom converter by actually attempting conversion and testing for assignment compatibility
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        private TypeConverter FindCustomConverter(Type typ)
        {
            var q = from converter in FindCustomConverters()
                    let convresult = converter.ConvertFromString(Str)
                    where convresult != null
                    let candidate = convresult.GetType() //candidate converted type
                    where typ.IsAssignableFrom(candidate)
                    select converter;

            return q.FirstOrDefault();
        }

        /// <summary>
        /// try convert
        /// </summary>
        /// <param name="binder"></param>CustomTypeConverters
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            //attempt using a custom converter if destination type doesn't have a specific TypeCode, and we appear to have an appropriate converter
            if (Type.GetTypeCode(binder.Type) == TypeCode.Object)
            {
                var ctc = FindCustomConverter(binder.Type);
                if (ctc != null)
                {
                    //yeah, we can use custom type converter
                    result = ctc.ConvertFromString(Str);
                    return true;
                }
            }

            //Typical case where we use Convert.ChangeType
            result = Convert.ChangeType(Str, binder.Type);
            return true;
        }

        /// <summary>
        /// to str
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Str?.ToString();
        }

        /// <summary>
        /// valid equality types for Equals
        /// </summary>
        private static readonly Type[] eqtypes = new Type[] { typeof(string), typeof(DynamicString) };

        /// <summary>
        /// equals?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            //type check. If not null but not a valid type, equality is false
            var othertype = obj?.GetType();
            if (othertype != null && !eqtypes.Any(x => x.IsAssignableFrom(x)))
            {
                return false;
            }

            //string check; unusual logic here in an attempt to achieve useful string semantics
            var otherstring = obj?.ToString();
            if (otherstring == null)
            {
                //if the "other" object is null, and the wrapped string (Str) is null, we consider this EQUAL!
                return Str == null;
            }
            else
            {
                //regular string equality check here. note we are calling STRING.Equals here
                return otherstring.Equals(Str);
            }
        }

        /// <summary>
        /// pull hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Str?.GetHashCode() ?? 0; //null gets a zero hash.
        }
    }
}
