namespace Stateless.Web
{
    using System;

    public static partial class Extensions
    {
        public static bool IsNumeric(this Type type)
        {
            if (type.IsArray)
            {
                return false;
            }

            if (type == typeof(byte) ||
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(sbyte) ||
                type == typeof(float) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong))
            {
                return true;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }

            return false;
        }
    }
}
