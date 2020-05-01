using System;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace Auditing.Infrastructure.CrossCutting
{
    public class JsonService : IJsonService
    {
        private readonly JsonDiffPatch _jdp;
        public JsonService()
        {
            _jdp = new JsonDiffPatch();
        }

        public JToken GetDifferences(JToken left, JToken right)
        {
            return _jdp.Diff(left, right);
        }

        public JObject ExtractJsonJObject(string jsonString)
        {
            try
            {
                return JObject.Parse(jsonString);
            }
            catch (Exception)
            {
                throw new FormatException($"An error has occurred trying to extract JsonObject from 'jsonString'. Check Json format");
            }
        }
    }
}