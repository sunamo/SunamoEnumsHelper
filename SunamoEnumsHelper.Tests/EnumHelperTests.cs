public enum Browsers
{
    None = 0,
    Chrome = 1,
    Firefox = 2,
    Edge = 4,
    Safari = 8
}

public class EnumHelperTests
{
    [Fact]
    public void ParseFromNumberTest()
    {
        var actual = EnumHelper.ParseFromNumber<Browsers, byte>(6, Browsers.Chrome);
    }
}
