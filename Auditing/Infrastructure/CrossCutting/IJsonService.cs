using Newtonsoft.Json.Linq;

namespace Auditing.Infrastructure.CrossCutting
{
    public interface IJsonService
    {
        JToken GetDifferences(JToken left, JToken right);
        JObject ExtractJsonJObject(string jsonString);
    }
}