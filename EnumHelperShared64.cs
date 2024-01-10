namespace SunamoEnumsHelper;

public static partial class EnumHelper
{
    /// <summary>
    /// ignore case. 
    /// A1 must be, default(T) cant be returned because in comparing default(T) is always true for any value of T
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    /// <param name = "web"></param>
    public static T Parse<T>(string web, T _def, bool returnDefIfNull = false)
        where T : struct
    {
        if (returnDefIfNull)
        {
            return _def;
        }
        T result;
        if (Enum.TryParse<T>(web, true, out result))
        {
            return result;
        }

        return _def;
    }
}
