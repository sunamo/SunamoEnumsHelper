// variables names: ok
/// <summary>
/// Test enum representing different browser types with flag values.
/// </summary>
public enum Browsers
{
    /// <summary>
    /// No browser selected.
    /// </summary>
    None = 0,
    /// <summary>
    /// Google Chrome browser.
    /// </summary>
    Chrome = 1,
    /// <summary>
    /// Mozilla Firefox browser.
    /// </summary>
    Firefox = 2,
    /// <summary>
    /// Microsoft Edge browser.
    /// </summary>
    Edge = 4,
    /// <summary>
    /// Apple Safari browser.
    /// </summary>
    Safari = 8
}

/// <summary>
/// Tests for EnumHelper utility class.
/// </summary>
public class EnumHelperTests
{
    /// <summary>
    /// Tests ParseFromNumber method with a numeric value that doesn't exist in the enum.
    /// When parsing number 6 (which is not a defined enum value), it should return the default value (Chrome).
    /// </summary>
    [Fact]
    public void ParseFromNumberTest()
    {
        // Arrange
        byte numericValue = 6; // 6 is not a defined value in Browsers enum
        Browsers defaultValue = Browsers.Chrome;
        Browsers expected = Browsers.Chrome; // Should return default when number doesn't exist in enum

        // Act
        var actual = EnumHelper.ParseFromNumber<Browsers, byte>(numericValue, defaultValue);

        // Assert
        Assert.Equal(expected, actual);
    }
}
