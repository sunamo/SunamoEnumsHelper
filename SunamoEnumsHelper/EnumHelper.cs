namespace SunamoEnumsHelper;

/// <summary>
/// Provides helper methods for working with enums, including parsing, conversion, flag operations, and value retrieval.
/// </summary>
public static class EnumHelper
{

    /// <summary>
    /// Converts enum flags to a comma-separated string representation.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="ds">The enum value to convert.</param>
    /// <returns>A comma-separated string of enum flag names, excluding "Nope" values.</returns>
    public static string EnumToString<T>(T ds) where T : Enum
    {
        const string comma = ",";
        var stringBuilder = new StringBuilder();
        var value = Enum.GetValues(typeof(T));
        foreach (T item in value)
            if (ds.HasFlag(item))
            {
                var ts = item.ToString();
                if (ts != CodeElementsConstants.NopeValue) stringBuilder.Append(ts + comma);
            }

        return stringBuilder.ToString().TrimEnd(comma[0]);
    }

    /// <summary>
    /// Gets all enum value names for the specified enum type.
    /// </summary>
    /// <param name="type">The enum type to get names from.</param>
    /// <returns>A list of all enum value names.</returns>
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
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="_def">The default list to return if parsing fails.</param>
    /// <param name="value">The list of string values to parse into enum values.</param>
    /// <returns>A list of parsed enum values, or the default list if parsing fails.</returns>
    public static List<T> GetEnumList<T>(List<T> _def, List<string> value)
        where T : struct
    {
        if (value == null) return _def;

        var vr = new List<T>();
        foreach (var item in value)
        {
            T enumValue;
            if (Enum.TryParse(item, out enumValue)) vr.Add(enumValue);
        }

        if (vr.Count == 0) return _def;

        return vr;
    }

    /// <summary>
    /// Converts an enum type to a dictionary mapping enum values to their lowercase string representations.
    /// </summary>
    /// <typeparam name="T">The enum value type.</typeparam>
    /// <param name="enumType">The enum type to convert.</param>
    /// <returns>A dictionary mapping enum values to lowercase string names.</returns>
    public static Dictionary<T, string> EnumToString<T>(Type enumType) where T : notnull
    {
        return Enum.GetValues(enumType).Cast<T>().Select(enumValue => new
            {
                Key = enumValue,
                // Must be lower due to EveryLine and e2sNamespaceCodeElements
                Value = enumValue?.ToString()?.ToLower() ?? string.Empty
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

    /// <summary>
    /// Parses a string to a nullable enum value, ignoring case.
    /// </summary>
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <param name="web">The string to parse.</param>
    /// <param name="_def">The default nullable value to return if parsing fails.</param>
    /// <returns>The parsed enum value or the default value if parsing fails.</returns>
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
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <typeparam name="Number">The numeric type of the input value.</typeparam>
    /// <param name="idProvider">The numeric value to convert to enum.</param>
    /// <param name="_def">The default enum value to return if conversion fails.</param>
    /// <returns>The parsed enum value or the default value if conversion fails.</returns>
    public static T ParseFromNumber<T, Number>(Number idProvider, T _def) where T : struct
    {
        var tn = (T)(dynamic)idProvider!;
        var tns = tn.ToString();
        var idProviderString = idProvider?.ToString();
        if (tns == idProviderString) return _def;

        var enumValue = Parse(tns!, _def);
        return enumValue;
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
        var values = Enum.GetValues(typeof(T)).Cast<int>().Select(v => (byte)v).ToArray();
        valuesInverted = values.Select(value => (byte)~value).ToArray();
        result = new List<T>();
        max = def;
        for (int i = def; i < values.Length; i++) max |= values[i];
    }

    /// <summary>
    /// Gets all flag names that are set in the specified enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="key">The enum value to get flags from.</param>
    /// <returns>A list of string names for all flags that are set.</returns>
    public static List<string> GetFlags<T>(T key) where T : Enum
    {
        var sourceList = new List<string>();
        var value = Enum.GetValues(typeof(T));

        foreach (Enum item in value)
            if (key.HasFlag(item))
                sourceList.Add(item.ToString());
        return sourceList;
    }

    /// <summary>
    ///     ignore case.
    ///     A1 must be, default(T) cant be returned because in comparing default(T) is always true for any value of T
    /// </summary>
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <param name="web">The string to parse.</param>
    /// <param name="_def">The default enum value to return if parsing fails.</param>
    /// <param name="returnDefIfNull">If true, returns the default value immediately without parsing.</param>
    /// <returns>The parsed enum value or the default value if parsing fails.</returns>
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
        valuesInverted = values.Select(value => ~value).ToArray();
        result = new List<T>();
        max = def;
        for (var i = def; i < values.Length; i++) max |= values[i];
    }

    #endregion

    #region GetValues - unlike GetAllValues in EnumHelper.cs can exclude Nope,Shared, etc.

    /// <summary>
    /// Gets all enum values excluding "Nope" and "None" values.
    /// Can be used only for int enums. For more control, use the overload with parameters.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>A list of enum values excluding "Nope" and "None".</returns>
    public static List<T> GetValues<T>()
        where T : struct
    {
        return GetValues<T>(false, true);
    }

    /// <summary>
    ///     Get all values expect of Nope/None
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="IncludeNope">Whether to include "Nope" and "None" values.</param>
    /// <param name="IncludeShared">Whether to include "Shared" or "Sha" values.</param>
    /// <returns>A list of enum values based on the specified inclusion criteria.</returns>
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