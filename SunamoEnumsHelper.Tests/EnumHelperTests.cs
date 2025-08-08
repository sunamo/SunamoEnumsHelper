public class EnumHelperTests
{
    [Fact]
    public void ParseFromNumberTest()
    {
        var actual = EnumHelper.ParseFromNumber<Browsers, byte>(6, Browsers.Chrome);
    }
}
