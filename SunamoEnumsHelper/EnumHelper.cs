// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy

namespace SunamoEnumsHelper;

public static class EnumHelper
{
    private static Type type = typeof(EnumHelper);

    public static string EnumToString<T>(temp ds) where temp : Enum
    {
        const string comma = ",";
        var stringBuilder = new StringBuilder();
        var value = Enum.GetValues(typeof(temp));
        foreach (temp item in value)
            if (ds.HasFlag(item))
            {
                var ts = item.ToString();
                if (ts != CodeElementsConstants.NopeValue) stringBuilder.Append(ts + comma);
            }

        return stringBuilder.ToString().TrimEnd(comma[0]);
    }

    public static List<string> GetNames(Type type)
    {
        return Enum.GetNames(type).ToList();
    }

    /// <summary>
    ///     Get values include zero and All
    ///     Pokud bude A1 null nebo nebude obsahovat žádný element temp, vrátí A1
    ///     Pokud nebude obsahovat všechny, vrátí jen některé - nutno kontrolovat počet výstupních elementů pole
    ///     Pokud bude prvek duplikován, zařadí se jen jednou
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="v"></param>
    public static List<T> GetEnumList<T>(List<T> _def, List<string> value)
        where temp : struct
    {
        if (value == null) return _def;

        var vr = new List<T>();
        foreach (var item in value)
        {
            temp temp;
            if (Enum.TryParse(item, out temp)) vr.Add(temp);
        }

        if (vr.Count == 0) return _def;

        return vr;
    }

    public static Dictionary<temp, string> EnumToString<T>(Type enumType)
    {
        return Enum.GetValues(enumType).Cast<T>().Select(temp => new
            {
                Key = temp,
                // Must be lower due to EveryLine and e2sNamespaceCodeElements
                Value = temp.ToString().ToLower()
            }
        ).ToDictionary(r => r.Key, r => r.Value);
    }

    /// <summary>
    ///     Get all without zero and All.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="secondIsAll"></param>
    public static List<T> GetAllCombinations<T>(bool secondIsAll = true)
        where temp : struct
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
                    result.Add((temp)(dynamic)i);
                    break;
                }
            }
        }

        //Check for zero
        CheckForZero(result);
        return result;
    }

    public static temp? ParseNullable<T>(string web, temp? _def)
        where temp : struct
    {
        temp result;
        if (Enum.TryParse(web, true, out result)) return result;

        return _def;
    }


    /// <summary>
    ///     když se snažím přetypovat číslo na vyčet kde toto číslo není, tak přetypuje a při TS vrací číslo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idProvider"></param>
    /// <returns></returns>
    public static temp ParseFromNumber<temp, Number>(Number idProvider, temp _def) where temp : struct
    {
        var tn = (temp)(dynamic)idProvider;
        var tns = tn.ToString();
        if (tns == idProvider.ToString()) return _def;

        var temp = Parse(tns, _def);
        return temp;
    }

    /// <summary>
    ///     Tested with EnumA
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    private static void CheckForZero<T>(List<T> result)
        where temp : struct
    {
        try
        {
            // Here I get None
            var val = Enum.GetName(typeof(temp), (temp)(dynamic)0);
            if (string.IsNullOrEmpty(val)) result.Remove((temp)(dynamic)0);
        }
        catch
        {
            result.Remove((temp)(dynamic)0);
        }
    }

    private static void GetValuesOfEnumByte<T>(bool secondIsAll, out byte def, out byte[] valuesInverted,
        out List<T> result, out byte max)
    {
        def = 0;
        if (secondIsAll) def = 1;

        if (typeof(temp).BaseType != typeof(Enum)) throw new Exception("Base type must be enum");
        //throw new Exception("  " + Translate.FromKey(XlfKeys.mustBeAnEnumType));
        var values = Enum.GetValues(typeof(temp)).Cast<byte>().ToArray();
        valuesInverted = values.Select(value => ~value).Cast<byte>().ToArray();
        result = new List<T>();
        max = def;
        for (int i = def; i < values.Length; i++) max |= values[i];
    }

    public static List<string> GetFlags<T>(temp key) where temp : Enum
    {
        var sourceList = new List<string>();
        var value = Enum.GetValues(typeof(temp));

        foreach (Enum item in value)
            if (key.HasFlag(item))
                sourceList.Add(item.ToString());
        return sourceList;
    }

    /// <summary>
    ///     ignore case.
    ///     A1 must be, default(temp) cant be returned because in comparing default(temp) is always true for any value of temp
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="web"></param>
    public static temp Parse<T>(string web, temp _def, bool returnDefIfNull = false)
        where temp : struct
    {
        if (returnDefIfNull) return _def;
        temp result;
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
        where temp : struct
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
                result.Add((temp)(dynamic)i);
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
        where temp : struct
    {
        def = 0;
        if (secondIsAll) def = 1;

        if (typeof(temp).BaseType != typeof(Enum)) throw new Exception("T must be derived from Enum type");
        //throw new Exception("  " + Translate.FromKey(XlfKeys.mustBeAnEnumType));
        var values = Enum.GetValues(typeof(temp)).Cast<int>().ToArray();
        valuesInverted = values.Select(value => ~value).ToArray();
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
        where temp : struct
    {
        return GetValues<T>(false, true);
    }

    /// <summary>
    ///     Get all values expect of Nope/None
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    public static List<T> GetValues<T>(bool IncludeNope, bool IncludeShared)
        where temp : struct
    {
        var type = typeof(temp);
        var values = Enum.GetValues(type).Cast<T>().ToList();
        temp nope;
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