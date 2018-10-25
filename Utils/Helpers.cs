using System;
using System.Linq;

namespace CalMan.Utils
{
    public static class EnumHelper<T> where T : struct
    {
        public static T FromString(string val)
        {
            try
            {
                return Enum.GetValues(typeof(T)).Cast<T>().First(v => v.ToString().ToLower() == val.ToLower());
            }
            catch (Exception ex)
            {
                throw new System.ArgumentException($"{val} cannot be casted to {typeof(T).Name}", ex);
            }
        }
        public static T FromNumber(int val)
        {
            try
            {
                return (T)Enum.ToObject(typeof(T), val);
            }
            catch (Exception ex)
            {
                throw new System.ArgumentException($"{val} cannot be casted to {typeof(T).Name}", ex);
            }
        }
    }
}