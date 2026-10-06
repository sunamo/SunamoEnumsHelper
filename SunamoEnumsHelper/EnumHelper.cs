namespace SunamoEnumsHelper;

public static class EnumHelper
{

    public static string EnumToString<T>(T enumValue) where T : Enum
    {
        const string comma = ",";
        var stringBuilder = new StringBuilder();
        var allValues = Enum.GetValues(typeof(T));
        foreach (T item in allValues)
            if (enumValue.HasFlag(item))
            {
                var enumName = item.ToString();
                if (enumName != CodeElementsConstants.NopeValue) stringBuilder.Append(enumName + comma);
            }

        return stringBuilder.ToString().TrimEnd(comma[0]);
    }

    public static List<string> GetNames(Type type)
    {
        return Enum.GetNames(type).ToList();
    }

    // Get values include zero and All.
    // If parsing fails or list is null, returns the default list.
    // Duplicates are added only once.
    public static List<T> GetEnumList<T>(List<T> defaultValue, List<string> valuesToParse)
        where T : struct
    {
        if (valuesToParse == null) return defaultValue;

        var result = new List<T>();
        foreach (var item in valuesToParse)
        {
            if (Enum.TryParse(item, out T parsedEnum)) result.Add(parsedEnum);
        }

        if (result.Count == 0) return defaultValue;

        return result;
    }

    public static Dictionary<T, string> EnumToString<T>(Type type) where T : notnull
    {
        return Enum.GetValues(type).Cast<T>().Select(enumValue => new
            {
                Key = enumValue,
                // Must be lower due to EveryLine and e2sNamespaceCodeElements
                Value = enumValue?.ToString()?.ToLower() ?? string.Empty
            }
        ).ToDictionary(r => r.Key, r => r.Value);
    }

    // Gets all enum combinations without zero and All values.
    // If isSecondAll is true, starts from index [1]. Otherwise from [0].
    public static List<T> GetAllCombinations<T>(bool isSecondAll = true)
        where T : struct
    {
        GetValuesOfEnum(isSecondAll, out int defaultIndex, out int[] valuesInverted, out List<T> result, out int max);
        for (var i = defaultIndex; i <= max; i++)
        {
            var unaccountedBits = i;
            for (var j = defaultIndex; j < valuesInverted.Length; j++)
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

    public static T? ParseNullable<T>(string text, T? defaultValue)
        where T : struct
    {
        if (Enum.TryParse(text, true, out T result)) return result;

        return defaultValue;
    }

    // Parses a numeric value to an enum value.
    // When trying to cast a number to an enum where this number doesn't exist, it casts and returns the number as string.
    public static T ParseFromNumber<T, Number>(Number numericValue, T defaultValue) where T : struct
    {
        var convertedEnum = (T)(dynamic)numericValue!;
        var convertedEnumString = convertedEnum.ToString();
        var numericValueString = numericValue?.ToString();
        if (convertedEnumString == numericValueString) return defaultValue;

        var enumValue = Parse(convertedEnumString!, defaultValue);
        return enumValue;
    }

    // Checked with EnumA.
    private static void CheckForZero<T>(List<T> list)
        where T : struct
    {
        try
        {
            // Here I get None
            var enumName = Enum.GetName(typeof(T), (T)(dynamic)0);
            if (string.IsNullOrEmpty(enumName)) list.Remove((T)(dynamic)0);
        }
        catch
        {
            list.Remove((T)(dynamic)0);
        }
    }

    public static List<string> GetFlags<T>(T key) where T : Enum
    {
        var sourceList = new List<string>();
        var value = Enum.GetValues(typeof(T));

        foreach (Enum item in value)
            if (key.HasFlag(item))
                sourceList.Add(item.ToString());
        return sourceList;
    }

    // Default value must be provided - default(T) cannot be returned because in comparing default(T) is always true for any value of T.
    public static T Parse<T>(string text, T defaultValue, bool isReturningDefIfNull = false)
        where T : struct
    {
        if (isReturningDefIfNull) return defaultValue;
        if (Enum.TryParse(text, true, out T result)) return result;

        return defaultValue;
    }

    #region GetAllValues - unlike GetValues in EnumHelperShared.cs not exclude anything. GetValues can exclude Nope,Shared,etc.

    // Gets all enum values without zero and All.
    // If isSecondAll is true, will start from [1]. Otherwise from [0].
    public static List<T> GetAllValues<T>(bool isSecondAll = true)
        where T : struct
    {
        GetValuesOfEnum(isSecondAll, out int defaultIndex, out int[] valuesInverted, out List<T> result, out int max);
        var i = max;
        var unaccountedBits = i;
        for (var j = defaultIndex; j < valuesInverted.Length; j++)
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

    // Gets enum values as int arrays with inverted values for bit operations.
    // If isSecondAll is true, will start from [1]. Otherwise from [0].
    // Enum values must be castable to int.
    // Cannot use second generic parameter, due to difficult operations like ~v or |=.
    private static void GetValuesOfEnum<T>(bool isSecondAll, out int defaultIndex, out int[] valuesInverted, out List<T> result,
        out int max)
        where T : struct
    {
        defaultIndex = 0;
        if (isSecondAll) defaultIndex = 1;

        if (typeof(T).BaseType != typeof(Enum)) throw new Exception("T must be derived from Enum type");
        var values = Enum.GetValues(typeof(T)).Cast<int>().ToArray();
        valuesInverted = values.Select(value => ~value).ToArray();
        result = new List<T>();
        max = defaultIndex;
        for (var i = defaultIndex; i < values.Length; i++) max |= values[i];
    }

    #endregion

    #region GetValues - unlike GetAllValues in EnumHelper.cs can exclude Nope,Shared, etc.

    // Can be used only for int enums. For more control, use the overload with parameters.
    public static List<T> GetValues<T>()
        where T : struct
    {
        return GetValues<T>(false, true);
    }

    public static List<T> GetValues<T>(bool isIncludingNope, bool isIncludingShared)
        where T : struct
    {
        var type = typeof(T);
        var values = Enum.GetValues(type).Cast<T>().ToList();
        if (!isIncludingNope)
            if (Enum.TryParse(CodeElementsConstants.NopeValue, out T enumValueToRemove))
                values.Remove(enumValueToRemove);

        if (!isIncludingShared)
        {
            if (type.Name == "MySites")
            {
                if (Enum.TryParse("Shared", out T enumValueToRemove2)) values.Remove(enumValueToRemove2);
            }
            else
            {
                if (Enum.TryParse("Sha", out T enumValueToRemove2)) values.Remove(enumValueToRemove2);
            }
        }

        if (Enum.TryParse(CodeElementsConstants.NoneValue, out T noneValue)) values.Remove(noneValue);

        return values;
    }

    #endregion
}
