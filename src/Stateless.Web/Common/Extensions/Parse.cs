namespace Stateless.Web
{
    using System;

    public static partial class Extensions
    {
        /// <summary>
        /// Converts the string representation of the name or numeric value of one or
        ///  more enumerated constants to an equivalent enumerated object.
        /// </summary>
        public static TEnum Parse<TEnum>(string value)
            where TEnum : struct, Enum
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }
    }
}
