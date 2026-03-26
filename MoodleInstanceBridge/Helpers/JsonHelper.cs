namespace MoodleInstanceBridge.Helpers
{
    using System.Text.Json;
    using static MoodleInstanceBridge.Services.WebServiceClient.MoodleWebServiceClient;

    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions Options;

        static JsonHelper()
        {
            Options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Options.Converters.Add(new BoolIntConverter());
            Options.Converters.Add(new NullableLongConverter());
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }

        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }
    }
}
