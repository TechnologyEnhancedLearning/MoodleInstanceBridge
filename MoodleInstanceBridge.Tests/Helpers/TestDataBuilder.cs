using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Tests.Helpers;

/// <summary>
/// Helper class for creating test data in unit and integration tests
/// </summary>
public static class TestDataBuilder
{
    public static MoodleInstanceConfig CreateMoodleInstanceConfig(
        string shortName = "test-instance",
        string moodleUrl =  "https://moodle.test.com",
        string apiToken = "test-token-12345")
    {
        return new MoodleInstanceConfig
        {
            ShortName = shortName,
            BaseUrl = moodleUrl,
            ApiToken = apiToken
        };
    }

    public static List<MoodleInstanceConfig> CreateMultipleMoodleInstanceConfigs(int count = 3)
    {
        var configs = new List<MoodleInstanceConfig>();
        for (int i = 1; i <= count; i++)
        {
            configs.Add(CreateMoodleInstanceConfig(
                shortName: $"instance{i}",
                moodleUrl: $"https://moodle{i}.test.com",
                apiToken: $"token-{i}"
            ));
        }
        return configs;
    }
}
