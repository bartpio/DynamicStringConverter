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
    internal class DynamicString : DynamicObject, IConvertible
    {
        /// <summary>
        /// internal rep
        /// </summary>
        private string Str { get;  }

        /// <summary>
        /// dynstring options
        /// </summary>
        private DynamicStringOptions Opts { get; }

        /// <summary>
        /// optional custom type conv
        /// </summary>
        private ReadOnlyCollection<TypeConverter> CustomTypeConverters { get; }

        /// <summary>
        /// cons
        /// </summary>
        /// <param name="str">string must not be null</param>
        /// <param name="opts">options</param>
        /// <param name="tc">custom type converter; if supplied, we will try these converters prior to trying Convert.ChangeType, in order supplied.
        /// Supplied converters must support converting FROM string, for proper functionality.
        /// </param>
        public DynamicString(string str, DynamicStringOptions opts, ReadOnlyCollection<TypeConverter> tc = null)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            Str = str;
            Opts = opts;
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
            //Null means null.
            if (Str == null)
            {
                throw new InvalidOperationException("Runtime assert failed; Str cannot be null"); //This cannot happen.
            }

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

            //Special case for emptystr if so configured!
            if (Opts.HasFlag(DynamicStringOptions.EmptyStringMeansDefault) && Str == String.Empty && !typeof(string).IsAssignableFrom(binder.Type))
            {
                result = GetDefaultValue(binder.Type);
                return true;
            }

            //Typical case where we use Convert.ChangeType
            result = Convert.ChangeType(Str, binder.Type);
            return true;
        }

        /// <summary>
        /// pull dflt value of a type at runtime.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// to str
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Str.ToString();
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
            if (obj == null)
            {
                return false;  //We can't possibly be null.
            }

            //type check. 
            var othertype = obj.GetType();
            if (othertype != null && !eqtypes.Any(x => x.IsAssignableFrom(othertype)))
            {
                return false;
            }

            //regular string equality check here. note we are calling STRING.Equals here
            return obj.ToString().Equals(Str);
        }

        /// <summary>
        /// pull hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Str.GetHashCode(); 
        }


        #region Auto implement via Str
        public TypeCode GetTypeCode()
        {
            return Str.GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToBoolean(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToByte(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToDateTime(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToDecimal(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToSingle(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return Str.ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)Str).ToType(conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)Str).ToUInt64(provider);
        }
        #endregion
    }
}
