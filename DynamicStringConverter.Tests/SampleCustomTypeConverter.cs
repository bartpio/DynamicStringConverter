using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace DynamicStringConverter.Tests
{
    class SampleCustomTypeConverter : TypeConverter
    {
        /// <summary>
        /// conv from
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new SampleCustomType { Somestring = value.ToString() };
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((SampleCustomType)value).Somestring;
            }

            return null;
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            var canwe = value?.ToString().StartsWith("CUSTOM!");
            return canwe.GetValueOrDefault();
        }
    }
}
