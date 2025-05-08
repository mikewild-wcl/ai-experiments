using unit_test_playground.Extensions;

namespace unit_test_playground;

public class FileRegexExtensionsTests
{
    // Setting up ClassData or MemberData - https://hamidmosalla.com/2017/02/25/xunit-theory-working-with-inlinedata-memberdata-classdata/
    [Theory]
    //[MemberData(nameof(UserPrompts))]
    [ClassData(typeof(UserInputDataGenerator))]
    public void ExtractFilePath_ReturnsExpectedResult(string? input, IEnumerable<string?> expectedOutputs)
    {
        // Test should take a string and return zero or more file outputs
        /*
         * #url:https://www.test.co.uk/index output https://www.test.co.uk/index
         */

        var results = input.ExtractFilePath();
        results.Should().BeEquivalentTo(expectedOutputs);
    }

    //public record UserPromptTestData(string? Input, IEnumerable<string?> Outputs);

    //public static IEnumerable<object[]> UserPrompts =>
    //new List<UserPromptTestData>
    //{
    //    new UserPromptTestData(null, Enumerable.Empty<string>()),
    //    new UserPromptTestData(string.Empty, Enumerable.Empty<string>()),
    //    new UserPromptTestData(
    //        "#url:https://www.test.co.uk/index",
    //        ["https://www.test.co.uk/index"]
    //        )
    //}
    //;

    //public static IEnumerable<object[]> UserPrompts =>
    //    new List<object[]>
    //    {            
    //        new object[] { (string)null, Enumerable.Empty<string>() },
    //        new object[] { "", Enumerable.Empty<string>() },
    //        new object[]
    //        {
    //            "#url:https://www.test.co.uk/index",
    //            new string[] { "https://www.test.co.uk/index" }
    //        };

    //https://stackoverflow.com/questions/51544883/how-can-i-pass-values-to-xunit-tests-that-accept-a-nullable-decimal
    public class UserInputDataGenerator : TheoryData<string?, IEnumerable<string>>
    {
        public UserInputDataGenerator()
        {
            Add("987", Enumerable.Empty<string>());
            Add(null, Enumerable.Empty<string>());
            Add(string.Empty, Enumerable.Empty<string>());
            Add("#url:https://www.test.co.uk/index", ["https://www.test.co.uk/index"]);
        }
    }
}
