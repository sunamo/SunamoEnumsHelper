namespace SunamoEnumsHelper;

public static class EnumHelper
{
    private static Type type = typeof(EnumHelper);

    public static string EnumToString<T>(T ds) where T : Enum
    {
        const string comma = ",";
        var sb = new StringBuilder();
        var v = Enum.GetValues(typeof(T));
        foreach (T item in v)
            if (ds.HasFlag(item))
            {
                var ts = item.ToString();
                if (ts != CodeElementsConstants.NopeValue) sb.Append(ts + comma);
            }

        return sb.ToString().TrimEnd(comma[0]);
    }

    public static List<string> GetNames(Type type)
    {
        return Enum.GetNames(type).ToList();
    }

    /// <summary>
    ///     Get values include zero and All
    ///     Pokud bude A1 null nebo nebude obsahovat žádný element T, vrátí A1
    ///     Pokud nebude obsahovat všechny, vrátí jen některé - nutno kontrolovat počet výstupních elementů pole
    ///     Pokud bude prvek duplikován, zařadí se jen jednou
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="v"></param>
    public static List<T> GetEnumList<T>(List<T> _def, List<string> v)
        where T : struct
    {
        if (v == null) return _def;

        var vr = new List<T>();
        foreach (var item in v)
        {
            T t;
            if (Enum.TryParse(item, out t)) vr.Add(t);
        }

        if (vr.Count == 0) return _def;

        return vr;
    }

    public static Dictionary<T, string> EnumToString<T>(Type enumType)
    {
        return Enum.GetValues(enumType).Cast<T>().Select(t => new
            {
                Key = t,
                // Must be lower due to EveryLine and e2sNamespaceCodeElements
                Value = t.ToString().ToLower()
            }
        ).ToDictionary(r => r.Key, r => r.Value);
    }

    /// <summary>
    ///     Get all without zero and All.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="secondIsAll"></param>
    public static List<T> GetAllCombinations<T>(bool secondIsAll = true)
        where T : struct
    {
        int def, max;
        int[] valuesInverted;
        List<T> result;
        GetValuesOfEnum(secondIsAll, out def, out valuesInverted, out result, out max);
        for (var i = def; i <= max; i++)
        {
            var unaccountedBits = i;
            for (var j = def; j < valuesInverted.Length; j++)
            {
                unaccountedBits &= valuesInverted[j];
                if (unaccountedBits == 0)
                {
                    result.Add((T)(dynamic)i);
                    break;
                }
            }
        }

        //Check for zero
        CheckForZero(result);
        return result;
    }

    public static T? ParseNullable<T>(string web, T? _def)
        where T : struct
    {
        T result;
        if (Enum.TryParse(web, true, out result)) return result;

        return _def;
    }


    /// <summary>
    ///     když se snažím přetypovat číslo na vyčet kde toto číslo není, tak přetypuje a při TS vrací číslo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idProvider"></param>
    /// <returns></returns>
    public static T ParseFromNumber<T, Number>(Number idProvider, T _def) where T : struct
    {
        var tn = (T)(dynamic)idProvider;
        var tns = tn.ToString();
        if (tns == idProvider.ToString()) return _def;

        var t = Parse(tns, _def);
        return t;
    }

    /// <summary>
    ///     Tested with EnumA
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    private static void CheckForZero<T>(List<T> result)
        where T : struct
    {
        try
        {
            // Here I get None
            var val = Enum.GetName(typeof(T), (T)(dynamic)0);
            if (string.IsNullOrEmpty(val)) result.Remove((T)(dynamic)0);
        }
        catch
        {
            result.Remove((T)(dynamic)0);
        }
    }

    private static void GetValuesOfEnumByte<T>(bool secondIsAll, out byte def, out byte[] valuesInverted,
        out List<T> result, out byte max)
    {
        def = 0;
        if (secondIsAll) def = 1;

        if (typeof(T).BaseType != typeof(Enum)) throw new Exception("Base type must be enum");
        //throw new Exception("  " + Translate.FromKey(XlfKeys.mustBeAnEnumType));
        var values = Enum.GetValues(typeof(T)).Cast<byte>().ToArray();
        valuesInverted = values.Select(v => ~v).Cast<byte>().ToArray();
        result = new List<T>();
        max = def;
        for (int i = def; i < values.Length; i++) max |= values[i];
    }

    public static List<string> GetFlags<T>(T key) where T : Enum
    {
        var ls = new List<string>();
        var v = Enum.GetValues(typeof(T));

        foreach (Enum item in v)
            if (key.HasFlag(item))
                ls.Add(item.ToString());
        return ls;
    }

    /// <summary>
    ///     ignore case.
    ///     A1 must be, default(T) cant be returned because in comparing default(T) is always true for any value of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="web"></param>
    public static T Parse<T>(string web, T _def, bool returnDefIfNull = false)
        where T : struct
    {
        if (returnDefIfNull) return _def;
        T result;
        if (Enum.TryParse(web, true, out result)) return result;

        return _def;
    }

    #region GetAllValues - unlike GetValues in EnumHelperShared.cs not exclude anything. GetValues can exclude Nope,Shared,etc.

    /// <summary>
    ///     If A1, will start from [1]. Otherwise from [0]
    ///     Get all without zero and All.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="secondIsAll"></param>
    public static List<T> GetAllValues<T>(bool secondIsAll = true)
        where T : struct
    {
        int def, max;
        int[] valuesInverted;
        List<T> result;
        GetValuesOfEnum(secondIsAll, out def, out valuesInverted, out result, out max);
        var i = max;
        var unaccountedBits = i;
        for (var j = def; j < valuesInverted.Length; j++)
        {
            unaccountedBits &= valuesInverted[j];
            if (unaccountedBits == 0)
            {
                result.Add((T)(dynamic)i);
                break;
            }
        }

        CheckForZero(result);
        return result;
    }

    /// <summary>
    ///     If A1, will start from [1]. Otherwise from [0]
    ///     Enem values must be castable to int
    ///     Cant be use second generic parameter, due to difficult operations like ~v or |=
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="secondIsAll"></param>
    /// <param name="def"></param>
    /// <param name="valuesInverted"></param>
    /// <param name="result"></param>
    /// <param name="max"></param>
    private static void GetValuesOfEnum<T>(bool secondIsAll, out int def, out int[] valuesInverted, out List<T> result,
        out int max)
        where T : struct
    {
        def = 0;
        if (secondIsAll) def = 1;

        if (typeof(T).BaseType != typeof(Enum)) throw new Exception("T must be derived from Enum type");
        //throw new Exception("  " + Translate.FromKey(XlfKeys.mustBeAnEnumType));
        var values = Enum.GetValues(typeof(T)).Cast<int>().ToArray();
        valuesInverted = values.Select(v => ~v).ToArray();
        result = new List<T>();
        max = def;
        for (var i = def; i < values.Length; i++) max |= values[i];
    }

    #endregion

    /// <summary>
    /// GET WITHOUT NOPE (parse string, not numeric), USE METHOD WITH MORE ARGS
    /// Can be use only for int enums
    /// </summary>
    /// <typeparam name="T"></typeparam>

    #region GetValues - unlike GetAllValues in EnumHelper.cs can exclude Nope,Shared, etc.

    ///
    public static List<T> GetValues<T>()
        where T : struct
    {
        return GetValues<T>(false, true);
    }

    /// <summary>
    ///     Get all values expect of Nope/None
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    public static List<T> GetValues<T>(bool IncludeNope, bool IncludeShared)
        where T : struct
    {
        var type = typeof(T);
        var values = Enum.GetValues(type).Cast<T>().ToList();
        T nope;
        if (!IncludeNope)
            if (Enum.TryParse(CodeElementsConstants.NopeValue, out nope))
                values.Remove(nope);

        if (!IncludeShared)
        {
            if (type.Name == "MySites")
            {
                if (Enum.TryParse("Shared", out nope)) values.Remove(nope);
            }
            else
            {
                if (Enum.TryParse("Sha", out nope)) values.Remove(nope);
            }
        }

        if (Enum.TryParse(CodeElementsConstants.NoneValue, out nope)) values.Remove(nope);

        return values;
    }

    #endregion
}