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
    /// <param name="enumValue">The enum value to convert.</param>
    /// <returns>A comma-separated string of enum flag names, excluding "Nope" values.</returns>
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
    /// Parses a list of string values into enum values.
    /// Get values include zero and All.
    /// If parsing fails or list is null, returns the default list.
    /// Duplicates are added only once.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="defaultValue">The default list to return if parsing fails.</param>
    /// <param name="valuesToParse">The list of string values to parse into enum values.</param>
    /// <returns>A list of parsed enum values, or the default list if parsing fails.</returns>
    public static List<T> GetEnumList<T>(List<T> defaultValue, List<string> valuesToParse)
        where T : struct
    {
        if (valuesToParse == null) return defaultValue;

        var result = new List<T>();
        foreach (var item in valuesToParse)
        {
            T parsedEnum;
            if (Enum.TryParse(item, out parsedEnum)) result.Add(parsedEnum);
        }

        if (result.Count == 0) return defaultValue;

        return result;
    }

    /// <summary>
    /// Converts an enum type to a dictionary mapping enum values to their lowercase string representations.
    /// </summary>
    /// <typeparam name="T">The enum value type.</typeparam>
    /// <param name="type">The enum type to convert.</param>
    /// <returns>A dictionary mapping enum values to lowercase string names.</returns>
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

    /// <summary>
    /// Gets all enum combinations without zero and All values.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="isSecondAll">If true, starts from index [1]. Otherwise from [0].</param>
    /// <returns>A list of all valid enum combinations.</returns>
    public static List<T> GetAllCombinations<T>(bool isSecondAll = true)
        where T : struct
    {
        int defaultIndex, max;
        int[] valuesInverted;
        List<T> result;
        GetValuesOfEnum(isSecondAll, out defaultIndex, out valuesInverted, out result, out max);
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

    /// <summary>
    /// Parses a string to a nullable enum value, ignoring case.
    /// </summary>
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <param name="text">The string to parse.</param>
    /// <param name="defaultValue">The default nullable value to return if parsing fails.</param>
    /// <returns>The parsed enum value or the default value if parsing fails.</returns>
    public static T? ParseNullable<T>(string text, T? defaultValue)
        where T : struct
    {
        T result;
        if (Enum.TryParse(text, true, out result)) return result;

        return defaultValue;
    }


    /// <summary>
    /// Parses a numeric value to an enum value.
    /// When trying to cast a number to an enum where this number doesn't exist, it casts and returns the number as string.
    /// </summary>
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <typeparam name="Number">The numeric type of the input value.</typeparam>
    /// <param name="numericValue">The numeric value to convert to enum.</param>
    /// <param name="defaultValue">The default enum value to return if conversion fails.</param>
    /// <returns>The parsed enum value or the default value if conversion fails.</returns>
    public static T ParseFromNumber<T, Number>(Number numericValue, T defaultValue) where T : struct
    {
        var convertedEnum = (T)(dynamic)numericValue!;
        var convertedEnumString = convertedEnum.ToString();
        var numericValueString = numericValue?.ToString();
        if (convertedEnumString == numericValueString) return defaultValue;

        var enumValue = Parse(convertedEnumString!, defaultValue);
        return enumValue;
    }

    /// <summary>
    /// Checks for and removes zero value from the enum result list.
    /// Tested with EnumA.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="list">The list to check and potentially remove zero value from.</param>
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
    /// Parses a string to an enum value, ignoring case.
    /// Default value must be provided - default(T) cannot be returned because in comparing default(T) is always true for any value of T.
    /// </summary>
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <param name="text">The string to parse.</param>
    /// <param name="defaultValue">The default enum value to return if parsing fails.</param>
    /// <param name="isReturningDefIfNull">If true, returns the default value immediately without parsing.</param>
    /// <returns>The parsed enum value or the default value if parsing fails.</returns>
    public static T Parse<T>(string text, T defaultValue, bool isReturningDefIfNull = false)
        where T : struct
    {
        if (isReturningDefIfNull) return defaultValue;
        T result;
        if (Enum.TryParse(text, true, out result)) return result;

        return defaultValue;
    }

    #region GetAllValues - unlike GetValues in EnumHelperShared.cs not exclude anything. GetValues can exclude Nope,Shared,etc.

    /// <summary>
    /// Gets all enum values without zero and All.
    /// If isSecondAll is true, will start from [1]. Otherwise from [0].
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="isSecondAll">If true, starts from index [1]. Otherwise from [0].</param>
    /// <returns>A list of all enum values excluding zero and All.</returns>
    public static List<T> GetAllValues<T>(bool isSecondAll = true)
        where T : struct
    {
        int defaultIndex, max;
        int[] valuesInverted;
        List<T> result;
        GetValuesOfEnum(isSecondAll, out defaultIndex, out valuesInverted, out result, out max);
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

    /// <summary>
    /// Gets enum values as int arrays with inverted values for bit operations.
    /// If isSecondAll is true, will start from [1]. Otherwise from [0].
    /// Enum values must be castable to int.
    /// Cannot use second generic parameter, due to difficult operations like ~v or |=.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="isSecondAll">If true, starts from index [1]. Otherwise from [0].</param>
    /// <param name="defaultIndex">Output: The default starting index (0 or 1).</param>
    /// <param name="valuesInverted">Output: Array of bitwise inverted enum values.</param>
    /// <param name="result">Output: Empty result list to be filled.</param>
    /// <param name="max">Output: Maximum combined enum value.</param>
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
    /// Gets all enum values with options to exclude Nope/None and Shared values.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="isIncludingNope">Whether to include "Nope" and "None" values.</param>
    /// <param name="isIncludingShared">Whether to include "Shared" or "Sha" values.</param>
    /// <returns>A list of enum values based on the specified inclusion criteria.</returns>
    public static List<T> GetValues<T>(bool isIncludingNope, bool isIncludingShared)
        where T : struct
    {
        var type = typeof(T);
        var values = Enum.GetValues(type).Cast<T>().ToList();
        T enumValueToRemove;
        if (!isIncludingNope)
            if (Enum.TryParse(CodeElementsConstants.NopeValue, out enumValueToRemove))
                values.Remove(enumValueToRemove);

        if (!isIncludingShared)
        {
            if (type.Name == "MySites")
            {
                if (Enum.TryParse("Shared", out enumValueToRemove)) values.Remove(enumValueToRemove);
            }
            else
            {
                if (Enum.TryParse("Sha", out enumValueToRemove)) values.Remove(enumValueToRemove);
            }
        }

        if (Enum.TryParse(CodeElementsConstants.NoneValue, out enumValueToRemove)) values.Remove(enumValueToRemove);

        return values;
    }

    #endregion
}