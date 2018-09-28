using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicStringConverter
{
    /// <summary>
    /// DynamicString conversion options
    /// </summary>
    [Flags]
    public enum DynamicStringOptions
    {
        /// <summary>
        /// Nothing special.
        /// </summary>
        None = 0,

        /// <summary>
        /// If set, we'll convert empty strings to defaults (such as zero for int)
        /// If not set, empty strings will usually throw
        /// </summary>
        EmptyStringMeansDefault = 1
    }
}
